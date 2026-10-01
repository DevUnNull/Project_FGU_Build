using UnityEngine;

/// <summary>
/// Gắn lên hero AD (như CellB) để bắn đạn tự dẫn.
/// </summary>
public class ProjectileLauncher : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private HomingProjectile projectilePrefab;
    [SerializeField] private Transform firePoint; // Nơi spawn đạn (nếu null dùng transform của hero)
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private bool allowFallbackBullet = true; // Tự động tạo đạn nếu prefab chưa gán

    private static HomingProjectile cachedFallbackPrefab;

    public bool CanLaunch => projectilePrefab != null || allowFallbackBullet;

    public void Launch(Transform target, int damage)
    {
        if (target == null) return;

        Transform spawnPoint = firePoint != null ? firePoint : transform;
        HomingProjectile prefabToUse = projectilePrefab;

        if (prefabToUse == null && allowFallbackBullet)
        {
            prefabToUse = GetOrCreateFallbackPrefab();
        }

        if (prefabToUse != null)
        {
            HomingProjectile proj = Instantiate(prefabToUse, spawnPoint.position, Quaternion.identity);
            proj.Initialize(target, damage, projectileSpeed);
        }
    }

    private HomingProjectile GetOrCreateFallbackPrefab()
    {
        if (cachedFallbackPrefab != null) return cachedFallbackPrefab;

        GameObject fallbackObj = new GameObject("CellB_FallbackBullet");
        DontDestroyOnLoad(fallbackObj);

        // Tạo Sprite hình tròn cho đạn
        SpriteRenderer sr = fallbackObj.AddComponent<SpriteRenderer>();
        Texture2D texture = new Texture2D(16, 16);
        Color circleColor = new Color(0.2f, 0.8f, 1f, 1f);
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 16; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(7.5f, 7.5f));
                if (dist <= 7f)
                    texture.SetPixel(x, y, circleColor);
                else
                    texture.SetPixel(x, y, Color.clear);
            }
        }
        texture.Apply();
        sr.sprite = Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);
        sr.sortingOrder = 10;

        CircleCollider2D col = fallbackObj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        fallbackObj.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

        cachedFallbackPrefab = fallbackObj.AddComponent<HomingProjectile>();
        fallbackObj.SetActive(false);

        return cachedFallbackPrefab;
    }
}



