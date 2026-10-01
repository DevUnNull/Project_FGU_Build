using UnityEngine;

/// <summary>
/// Virus E.coli:
/// Trường hợp 1: Va chạm vào bất kỳ Cell nào của Player (hoặc chạm tường) -> Nổ và để lại 1 bãi Axit gây sát thương theo giây.
/// Trường hợp 2: Bị đánh bại trước khi va chạm -> Nổ ngay tại chỗ đứng gây sát thương lan (AoE).
/// </summary>
public class EnemyEcoli : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 1.8f;
    [SerializeField] private int instantExplosionDamage = 25;

    [Header("Acid Puddle Settings")]
    [SerializeField] private float acidPuddleDuration = 8.0f;
    [SerializeField] private float acidPuddleRadius = 1.5f;
    [SerializeField] private int acidDps = 5;

    [Header("Visual Customization")]
    [Tooltip("Prefab Vũng Axit tùy chỉnh (kéo Prefab có script AcidPuddle vào đây)")]
    [SerializeField] private AcidPuddle acidPuddlePrefab;
    [Tooltip("Hình ảnh Sprite vũng Axit (kéo Sprite vũng axit vào đây nếu không dùng Prefab)")]
    [SerializeField] private Sprite acidPuddleSprite;
    [Tooltip("Hình ảnh Sprite hiệu ứng nổ Ecoli")]
    [SerializeField] private Sprite explosionSprite;

    private _Enemy enemyBase;
    private bool hasExploded = false;

    private void Awake()
    {
        enemyBase = GetComponent<_Enemy>();
    }

    private void Update()
    {
        if (hasExploded) return;

        // Trường hợp 2: Kiểm tra nếu máu bị đánh về <= 0 bởi Player trước khi va chạm -> Nổ tại chỗ
        if (enemyBase != null && enemyBase.health <= 0)
        {
            ExplodeOnSpot();
            return;
        }

        // Tự động kiểm tra va chạm gần với bất kỳ Tế bào Player nào trên bàn
        CheckProximityCollisionWithCell();
    }

    private void CheckProximityCollisionWithCell()
    {
        if (hasExploded) return;

        HeroBase[] heroes = FindObjectsOfType<HeroBase>();
        Vector3 myPos = transform.position;

        foreach (var hero in heroes)
        {
            if (hero == null || !hero.gameObject.activeInHierarchy || hero.health <= 0) continue;
            if (!DragAndDrop.IsUnitActiveOnBoard(hero.gameObject)) continue;

            float dist = Vector2.Distance(myPos, hero.transform.position);
            if (dist <= 0.8f) // Khoảng cách va chạm với Cell
            {
                ExplodeAndCreateAcid(myPos);
                break;
            }
        }
    }

    /// <summary>
    /// Trường hợp 1: Va chạm Cell hoặc Tường -> Nổ tức thời + Để lại bãi Axit dưới chân.
    /// </summary>
    public void ExplodeAndCreateAcid(Vector3 hitPosition)
    {
        if (hasExploded) return;
        hasExploded = true;

        Debug.Log("💥 E.coli va chạm Cell/Tường! Nổ và để lại bãi Axit!");

        // Gây sát thương nổ tức thời xung quanh vị trí nổ
        DealAoEDamage(hitPosition, instantExplosionDamage);

        // Tạo bãi Axit ngay tại chỗ nổ
        AcidPuddle puddle = null;
        if (acidPuddlePrefab != null)
        {
            puddle = Instantiate(acidPuddlePrefab, hitPosition, Quaternion.identity);
        }
        else
        {
            GameObject acidObj = new GameObject("AcidPuddle_Ecoli");
            acidObj.transform.position = hitPosition;
            puddle = acidObj.AddComponent<AcidPuddle>();
        }

        if (puddle != null)
        {
            if (acidPuddleSprite != null)
            {
                puddle.SetCustomSprite(acidPuddleSprite);
            }
            puddle.Init(acidPuddleDuration, acidPuddleRadius, acidDps);
        }

        VisualExplosionEffect(hitPosition, new Color(0.2f, 0.9f, 0.1f, 0.8f));

        Despawn();
    }

    /// <summary>
    /// Trường hợp 2: Bị tiêu diệt trước khi chạm Cell -> Nổ ngay tại chỗ đứng.
    /// </summary>
    public void ExplodeOnSpot()
    {
        if (hasExploded) return;
        hasExploded = true;

        Debug.Log("💥 E.coli bị tiêu diệt! Nổ ngay tại chỗ!");

        Vector3 pos = transform.position;
        DealAoEDamage(pos, instantExplosionDamage);
        VisualExplosionEffect(pos, Color.yellow);

        Despawn();
    }

    private void Despawn()
    {
        if (MultiEnemyPool.Instance != null)
        {
            MultiEnemyPool.Instance.ReturnToPool(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void DealAoEDamage(Vector3 center, int damage)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, explosionRadius);
        foreach (var col in hits)
        {
            HeroBase hero = col.GetComponent<HeroBase>() ?? col.GetComponentInParent<HeroBase>();
            if (hero != null && hero.health > 0 && DragAndDrop.IsUnitActiveOnBoard(hero.gameObject))
            {
                hero.TakeDamage(damage);
                Debug.Log($"💥 Sức nổ E.coli gây {damage} sát thương lên {hero.name}");
            }
        }
    }

    private void VisualExplosionEffect(Vector3 center, Color color)
    {
        GameObject vfx = new GameObject("EcoliExplosionVFX");
        vfx.transform.position = center;

        SpriteRenderer sr = vfx.AddComponent<SpriteRenderer>();

        if (explosionSprite != null)
        {
            sr.sprite = explosionSprite;
            sr.sortingOrder = 12;
        }
        else
        {
            Texture2D texture = new Texture2D(32, 32);
            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 32; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f));
                    if (dist <= 15f)
                        texture.SetPixel(x, y, color);
                    else
                        texture.SetPixel(x, y, Color.clear);
                }
            }
            texture.Apply();
            sr.sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 16);
            sr.sortingOrder = 12;
        }

        Destroy(vfx, 0.4f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded || other == null) return;

        // 1. Va chạm với Cell của Player đã đặt trên bàn
        HeroBase hero = other.GetComponent<HeroBase>() ?? other.GetComponentInParent<HeroBase>();
        if (hero != null && hero.health > 0 && DragAndDrop.IsUnitActiveOnBoard(hero.gameObject))
        {
            ExplodeAndCreateAcid(transform.position);
            return;
        }

        // 2. Va chạm với Tường / Destination
        bool isWall = other.GetComponent<DestinationEnemy>() != null ||
                      other.name.ToLower().Contains("destination") ||
                      other.name.ToLower().Contains("wall") ||
                      other.name.ToLower().Contains("finish");

        if (isWall)
        {
            ExplodeAndCreateAcid(transform.position);
        }
    }
}
