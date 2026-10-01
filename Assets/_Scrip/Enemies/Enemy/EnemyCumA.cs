using UnityEngine;

/// <summary>
/// Virus CumA (Cúm A): Bắn đạn từ xa gây sát thương lên các tế bào Player.
/// </summary>
public class EnemyCumA : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 7.0f;
    [SerializeField] private float attackInterval = 1.5f;
    [SerializeField] private float projectileSpeed = 8.0f;

    [Header("Bullet Visual Customization")]
    [Tooltip("Prefab viên đạn tùy chỉnh (kéo Prefab có script EnemyProjectile vào đây)")]
    [SerializeField] private EnemyProjectile projectilePrefab;
    [Tooltip("Hình ảnh Sprite của viên đạn (kéo Sprite viên đạn vào đây nếu không dùng Prefab)")]
    [SerializeField] private Sprite bulletSprite;

    private _Enemy enemyBase;
    private WaypointMovement waypointMovement;
    private float attackTimer = 0f;

    private void Awake()
    {
        enemyBase = GetComponent<_Enemy>();
        waypointMovement = GetComponent<WaypointMovement>();
    }

    private void Update()
    {
        HeroBase target = FindTargetCell();

        if (target != null)
        {
            // 🚫 Dừng lại để bắn đạn khi phát hiện Tế bào trong tầm
            if (waypointMovement != null) waypointMovement.isStopped = true;

            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                ShootAt(target);
                attackTimer = 0f;
            }
        }
        else
        {
            // ✅ Tiếp tục di chuyển khi không có Tế bào trong tầm bắn
            if (waypointMovement != null) waypointMovement.isStopped = false;
            attackTimer = 0f;
        }
    }

    private HeroBase FindTargetCell()
    {
        HeroBase[] heroes = FindObjectsOfType<HeroBase>();
        HeroBase closest = null;
        float minDistanceX = attackRange;

        Vector3 cumAPos = transform.position;

        foreach (var hero in heroes)
        {
            if (hero == null || !hero.gameObject.activeInHierarchy || hero.health <= 0) continue;

            // 🚫 Bỏ qua các tướng chưa mua / đang ở trong shop
            if (!DragAndDrop.IsUnitActiveOnBoard(hero.gameObject)) continue;

            Vector3 heroPos = hero.transform.position;

            // ✅ BẮN THEO TRỤC X NGANG: Chỉ bắn Tế bào trên CÙNG HÀNG (Y chênh lệch <= 0.6f)
            float deltaY = Mathf.Abs(heroPos.y - cumAPos.y);
            if (deltaY > 0.6f) continue;

            // Tế bào phải ở phía trước (bên trái CumA)
            float deltaX = cumAPos.x - heroPos.x;
            if (deltaX < -0.2f) continue;

            if (deltaX <= attackRange && deltaX < minDistanceX)
            {
                minDistanceX = deltaX;
                closest = hero;
            }
        }

        return closest;
    }

    private void ShootAt(HeroBase target)
    {
        int damage = enemyBase != null ? enemyBase.damage : 10;
        if (damage <= 0) damage = 10;

        if (projectilePrefab == null)
        {
#if UNITY_EDITOR
            projectilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<EnemyProjectile>("Assets/_Prefabs/Bullets/Bullet_CumA.prefab");
#endif
        }

        EnemyProjectile proj = null;

        if (projectilePrefab != null)
        {
            proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        }
        else
        {
            GameObject projObj = new GameObject("CumA_Bullet");
            projObj.transform.position = transform.position;
            proj = projObj.AddComponent<EnemyProjectile>();
        }

        if (proj != null)
        {
            if (bulletSprite != null)
            {
                proj.SetCustomSprite(bulletSprite);
            }
            proj.Initialize(target.transform, damage, projectileSpeed);
        }

        Debug.Log($"👾 CumA bắn đạn vào {target.name} gây {damage} sát thương!");
    }
}
