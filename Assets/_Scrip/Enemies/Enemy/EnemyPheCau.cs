using UnityEngine;

/// <summary>
/// Virus PheCau (Phế Cầu): Tấn công cận chiến. Khi chạm/ở gần Tế bào Player sẽ gây sát thương 1 giây 1 lần.
/// </summary>
public class EnemyPheCau : MonoBehaviour
{
    [Header("Melee Settings")]
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackInterval = 1.0f; // 1s / 1 lần gây sát thương

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
        HeroBase target = FindTargetCellInRange();

        if (target != null)
        {
            // 🚫 Dừng lại khi có Tế bào trong tầm đánh cận chiến
            if (waypointMovement != null) waypointMovement.isStopped = true;

            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                PerformMeleeAttack(target);
                attackTimer = 0f;
            }
        }
        else
        {
            // ✅ Tiếp tục di chuyển khi không có Tế bào trước mặt
            if (waypointMovement != null) waypointMovement.isStopped = false;
            attackTimer = 0f;
        }
    }

    private HeroBase FindTargetCellInRange()
    {
        HeroBase[] heroes = FindObjectsOfType<HeroBase>();
        HeroBase closest = null;
        float minDistanceX = attackRange;

        Vector3 pheCauPos = transform.position;

        foreach (var hero in heroes)
        {
            if (hero == null || !hero.gameObject.activeInHierarchy || hero.health <= 0) continue;

            // 🚫 Bỏ qua các tướng chưa mua / đang trong shop
            if (!DragAndDrop.IsUnitActiveOnBoard(hero.gameObject)) continue;

            Vector3 heroPos = hero.transform.position;

            // Kiểm tra theo hàng (Y chênh lệch <= 0.6f)
            float deltaY = Mathf.Abs(heroPos.y - pheCauPos.y);
            if (deltaY > 0.6f) continue;

            float dist = Vector2.Distance(pheCauPos, heroPos);
            if (dist <= minDistanceX)
            {
                minDistanceX = dist;
                closest = hero;
            }
        }

        return closest;
    }

    private void PerformMeleeAttack(HeroBase target)
    {
        int damage = enemyBase != null ? enemyBase.damage : 10;
        if (damage <= 0) damage = 10;

        target.TakeDamage(damage);
        Debug.Log($"🦠 PheCau đánh cận chiến {target.name} gây {damage} sát thương (1s/lần)!");
    }
}
