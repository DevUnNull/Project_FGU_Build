using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor script tự động tạo Bullet Prefab cho CumA và Ecoli, đồng thời gán vào các Enemy Prefabs tương ứng.
/// </summary>
public class SetupVirusPrefabs
{
    [MenuItem("Tools/Setup Virus Bullets & Prefabs")]
    [InitializeOnLoadMethod]
    public static void Setup()
    {
        EnsureDirectories();

        // 1. Tạo Prefab đạn cho Cúm A (Bullet_CumA.prefab)
        GameObject cumABulletPrefab = CreateOrUpdateCumABulletPrefab();

        // 2. Tạo Prefab vũng axit cho Ecoli (AcidPuddle_Ecoli.prefab)
        GameObject acidPuddlePrefab = CreateOrUpdateAcidPuddlePrefab();

        // 3. Gán Bullet_CumA vào Enemy_CumA.prefab
        AssignBulletToCumAPrefab(cumABulletPrefab);

        // 4. Gán và cập nhật Enemy_Ecoli.prefab
        SetupEcoliPrefab(acidPuddlePrefab);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✅ [Tools] Đã tạo thành công và gán đạn/vũng axit cho Cúm A & Ecoli!");
    }

    private static void EnsureDirectories()
    {
        if (!AssetDatabase.IsValidFolder("Assets/_Prefabs/Bullets"))
        {
            AssetDatabase.CreateFolder("Assets/_Prefabs", "Bullets");
        }
    }

    private static GameObject CreateOrUpdateCumABulletPrefab()
    {
        string path = "Assets/_Prefabs/Bullets/Bullet_CumA.prefab";
        GameObject bulletObj = new GameObject("Bullet_CumA");

        SpriteRenderer sr = bulletObj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite(new Color(1f, 0.25f, 0.25f, 1f)); // Đạn màu đỏ virus
        sr.sortingOrder = 10;

        CircleCollider2D col = bulletObj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.35f;

        EnemyProjectile proj = bulletObj.AddComponent<EnemyProjectile>();
        bulletObj.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(bulletObj, path);
        Object.DestroyImmediate(bulletObj);
        return savedPrefab;
    }

    private static GameObject CreateOrUpdateAcidPuddlePrefab()
    {
        string path = "Assets/_Prefabs/Bullets/AcidPuddle_Ecoli.prefab";
        GameObject puddleObj = new GameObject("AcidPuddle_Ecoli");

        SpriteRenderer sr = puddleObj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite(new Color(0.2f, 0.9f, 0.1f, 0.5f)); // Vũng axit màu xanh lá trong suốt
        sr.sortingOrder = 1;

        AcidPuddle puddle = puddleObj.AddComponent<AcidPuddle>();
        puddleObj.transform.localScale = new Vector3(3f, 3f, 1f);

        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(puddleObj, path);
        Object.DestroyImmediate(puddleObj);
        return savedPrefab;
    }

    private static void AssignBulletToCumAPrefab(GameObject bulletPrefab)
    {
        string cumAPath = "Assets/_Prefabs/PvZ/Enemy_CumA.prefab";
        GameObject cumAPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(cumAPath);
        if (cumAPrefab == null) return;

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(cumAPrefab);

        if (instance.GetComponent<_Enemy>() == null) instance.AddComponent<_Enemy>();

        EnemyCumA cumA = instance.GetComponent<EnemyCumA>();
        if (cumA == null) cumA = instance.AddComponent<EnemyCumA>();

        if (bulletPrefab != null)
        {
            SerializedObject so = new SerializedObject(cumA);
            SerializedProperty prop = so.FindProperty("projectilePrefab");
            if (prop != null)
            {
                prop.objectReferenceValue = bulletPrefab.GetComponent<EnemyProjectile>();
                so.ApplyModifiedProperties();
            }
        }

        PrefabUtility.SaveAsPrefabAsset(instance, cumAPath);
        Object.DestroyImmediate(instance);
    }

    private static void SetupEcoliPrefab(GameObject acidPuddlePrefab)
    {
        string ecoliPath = "Assets/_Prefabs/PvZ/Enemy_Ecoli.prefab";
        GameObject ecoliPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ecoliPath);
        if (ecoliPrefab == null) return;

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(ecoliPrefab);

        if (instance.GetComponent<_Enemy>() == null) instance.AddComponent<_Enemy>();
        EnemyEcoli ecoli = instance.GetComponent<EnemyEcoli>();
        if (ecoli == null) ecoli = instance.AddComponent<EnemyEcoli>();

        if (acidPuddlePrefab != null)
        {
            SerializedObject so = new SerializedObject(ecoli);
            SerializedProperty prop = so.FindProperty("acidPuddlePrefab");
            if (prop != null)
            {
                prop.objectReferenceValue = acidPuddlePrefab.GetComponent<AcidPuddle>();
                so.ApplyModifiedProperties();
            }
        }

        PrefabUtility.SaveAsPrefabAsset(instance, ecoliPath);
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
