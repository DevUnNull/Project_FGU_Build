using System.Collections;
using UnityEngine;

/// <summary>
/// Virus E.coli:
/// khi chết -> phát animation "die" 1 lần.
/// Sau khi animation die chạy xong -> nổ gây sát thương AoE + để lại vũng Axit phát animation 4 frame (baiAxit).
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
    [Tooltip("Prefab Vũng Axit tùy chỉnh")]
    [SerializeField] private AcidPuddle acidPuddlePrefab;
    [Tooltip("Hình ảnh Sprite vũng Axit")]
    [SerializeField] private Sprite acidPuddleSprite;
    [Tooltip("Hình ảnh Sprite hiệu ứng nổ Ecoli")]
    [SerializeField] private Sprite explosionSprite;

    private _Enemy enemyBase;
    private WaypointMovement waypointMovement;
    private Animator animator;
    private bool hasExploded = false;

    private void Awake()
    {
        enemyBase = GetComponent<_Enemy>();
        waypointMovement = GetComponent<WaypointMovement>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (hasExploded) return;

        // Trường hợp 2: Bị tiêu diệt trước khi chạm -> Nổ tại chỗ
        if (enemyBase != null && enemyBase.health <= 0)
        {
            ExplodeOnSpot();
            return;
        }

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
    /// Trường hợp 1: Va chạm Cell hoặc Tường -> Phát animation die 1 lần -> Nổ + Để lại bãi Axit
    /// </summary>
    public void ExplodeAndCreateAcid(Vector3 hitPosition)
    {
        if (hasExploded) return;
        hasExploded = true;

        if (waypointMovement != null) waypointMovement.isStopped = true;

        StartCoroutine(Co_DieSequence(hitPosition, spawnAcid: true));
    }

    /// <summary>
    /// Trường hợp 2: Bị tiêu diệt trước khi chạm Cell -> Phát animation die 1 lần -> Nổ tại chỗ
    /// </summary>
    public void ExplodeOnSpot()
    {
        if (hasExploded) return;
        hasExploded = true;

        if (waypointMovement != null) waypointMovement.isStopped = true;

        StartCoroutine(Co_DieSequence(transform.position, spawnAcid: false));
    }

    private IEnumerator Co_DieSequence(Vector3 position, bool spawnAcid)
    {
        // 1. Phát animation "die" 1 lần
        float dieAnimDuration = 0.8f;
        if (animator != null)
        {
            animator.Play("die", 0, 0f);

            // Tìm độ dài clip die nếu có
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.length > 0)
            {
                dieAnimDuration = stateInfo.length;
            }
        }

        // Chờ animation "die" chạy hết 1 lần
        yield return new WaitForSeconds(dieAnimDuration);

        // 2. Nổ gây sát thương AoE xung quanh
        DealAoEDamage(position, instantExplosionDamage);
        VisualExplosionEffect(position, new Color(0.2f, 0.9f, 0.1f, 0.8f));

        // 3. Tạo bãi Axit hoạt họa nếu có yêu cầu
        if (spawnAcid)
        {
            CreateAcidPuddle(position);
        }

        // 4. Biến mất / trả về Pool
        Despawn();
    }

    private void CreateAcidPuddle(Vector3 position)
    {
        AcidPuddle puddle = null;
        if (acidPuddlePrefab != null)
        {
            puddle = Instantiate(acidPuddlePrefab, position, Quaternion.identity);
        }
        else
        {
            GameObject acidObj = new GameObject("AcidPuddle_Ecoli");
            acidObj.transform.position = position;
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

        HeroBase hero = other.GetComponent<HeroBase>() ?? other.GetComponentInParent<HeroBase>();
        if (hero != null && hero.health > 0 && DragAndDrop.IsUnitActiveOnBoard(hero.gameObject))
        {
            ExplodeAndCreateAcid(transform.position);
            return;
        }

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
