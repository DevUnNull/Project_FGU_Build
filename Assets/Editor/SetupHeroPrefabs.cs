using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor script tự động cấu hình tất cả các Tế bào Hero (Cell) và tạo Bullet Prefab tạm thời.
/// </summary>
public class SetupHeroPrefabs
{
    [MenuItem("Tools/Setup Hero Prefabs & Bullets")]
    [InitializeOnLoadMethod]
    public static void Setup()
    {
        EnsureDirectories();

        // 1. Tạo các Bullet Prefab tạm (Placeholder Bullets) cho các tướng
        GameObject cellBBullet = CreatePlaceholderBullet("Bullet_CellB", new Color(0.2f, 0.7f, 1f));
        GameObject genericHeroBullet = CreatePlaceholderBullet("Bullet_GenericHero", new Color(1f, 0.8f, 0.2f));

        // 2. Cấu hình tất cả Hero Prefabs trong Assets/_Prefabs/PvZ/ và Assets/_Prefabs/Hero/
        SetupHeroPrefab("Assets/_Prefabs/PvZ/CellB.prefab", cellBBullet);
        SetupHeroPrefab("Assets/_Prefabs/PvZ/CellBachCau.prefab", null); // Melee
        SetupHeroPrefab("Assets/_Prefabs/PvZ/CellDaiThucBao.prefab", null); // Melee
        SetupHeroPrefab("Assets/_Prefabs/PvZ/CellHongCau.prefab", null); // Coin producer

        // Tìm thêm các hero prefabs khác nếu có
        string[] extraHeroPaths = new string[] {
            "Assets/_Prefabs/Hero/Tris.prefab",
            "Assets/_Prefabs/Hero/kai'sa.prefab",
            "Assets/_Prefabs/Hero/Senna.prefab",
            "Assets/_Prefabs/Hero/Zeri.prefab"
        };

        foreach (var path in extraHeroPaths)
        {
            SetupHeroPrefab(path, genericHeroBullet);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✅ [Tools] Đã tự động cấu hình và tạo Bullet Prefab cho tất cả các Cell!");
    }

    private static void EnsureDirectories()
    {
        if (!AssetDatabase.IsValidFolder("Assets/_Prefabs/Bullets"))
        {
            AssetDatabase.CreateFolder("Assets/_Prefabs", "Bullets");
        }
    }

    private static GameObject CreatePlaceholderBullet(string bulletName, Color color)
    {
        string path = $"Assets/_Prefabs/Bullets/{bulletName}.prefab";

        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        GameObject bulletObj = new GameObject(bulletName);

        SpriteRenderer sr = bulletObj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite(color);
        sr.sortingOrder = 10;

        CircleCollider2D col = bulletObj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.35f;

        HomingProjectile proj = bulletObj.AddComponent<HomingProjectile>();
        bulletObj.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(bulletObj, path);
        Object.DestroyImmediate(bulletObj);
        return savedPrefab;
    }

    private static void SetupHeroPrefab(string prefabPath, GameObject bulletPrefab)
    {
        GameObject heroPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (heroPrefab == null) return;

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(heroPrefab);

        if (instance.GetComponent<HeroBase>() == null) instance.AddComponent<HeroBase>();

        HeroStateMachine sm = instance.GetComponent<HeroStateMachine>();
        if (sm == null) sm = instance.AddComponent<LineHeroStateMachine>();

        // Nếu là tướng bắn đạn (có bulletPrefab hoặc không phải Melee)
        if (!prefabPath.Contains("BachCau") && !prefabPath.Contains("DaiThucBao") && !prefabPath.Contains("HongCau"))
        {
            ProjectileLauncher launcher = instance.GetComponent<ProjectileLauncher>();
            if (launcher == null) launcher = instance.AddComponent<ProjectileLauncher>();

            if (bulletPrefab != null)
            {
                SerializedObject so = new SerializedObject(launcher);
                SerializedProperty prop = so.FindProperty("projectilePrefab");
                if (prop != null)
                {
                    prop.objectReferenceValue = bulletPrefab.GetComponent<HomingProjectile>();
                    so.ApplyModifiedProperties();
                }
            }
        }

        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        Object.DestroyImmediate(instance);
    }

    private static Sprite CreateCircleSprite(Color color)
    {
        Texture2D tex = new Texture2D(32, 32);
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f));
                if (dist <= 14f)
                    tex.SetPixel(x, y, color);
                else
                    tex.SetPixel(x, y, Color.clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 16);
    }
}
