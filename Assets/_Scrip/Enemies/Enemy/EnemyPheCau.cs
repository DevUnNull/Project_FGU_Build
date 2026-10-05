using System.Collections;
using UnityEngine;

/// <summary>
/// Virus PheCau (Phế Cầu): Tấn công cận chiến. Khi chạm/ở gần Tế bào Player sẽ kích hoạt animation attack,
/// lướt nhẹ sang trái 1 chút và lùi về đúng vị trí (khớp nhịp animation), đồng thời gây sát thương qua Animation Event.
/// </summary>
public class EnemyPheCau : MonoBehaviour
{
    [Header("Melee Settings")]
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackInterval = 1.0f; // 1s / 1 lần tấn công

    [Header("Lunge Movement (Lướt sang trái khi đánh)")]
    [SerializeField] private float lungeDistance = 0.4f; // Khoảng cách lướt sang trái
    [SerializeField] private float lungeForwardDuration = 0.15f; // Thời gian lao sang trái
    [SerializeField] private float lungeReturnDuration = 0.25f; // Thời gian lùi về vị trí cũ

    private _Enemy enemyBase;
    private WaypointMovement waypointMovement;
    private Animator animator;

    private float attackTimer = 0f;
    private HeroBase currentTarget;
    private bool isLunging = false;

    private void Awake()
    {
        enemyBase = GetComponent<_Enemy>();
        if (enemyBase != null && enemyBase.attackInterval > 0f)
        {
            attackInterval = enemyBase.attackInterval;
        }
        waypointMovement = GetComponent<WaypointMovement>();
        animator = GetComponent<Animator>();
    }

    public void SetAttackInterval(float interval)
    {
        if (interval > 0f) attackInterval = interval;
    }

    private void Update()
    {
        currentTarget = FindTargetCellInRange();

        if (currentTarget != null)
        {
            // 🚫 Dừng di chuyển đường dài khi có Tế bào trong tầm đánh
            if (waypointMovement != null) waypointMovement.isStopped = true;

            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                TriggerAttackSequence(currentTarget);
                attackTimer = 0f;
            }
        }
        else
        {
            // ✅ Tiếp tục di chuyển khi không có Tế bào
            if (waypointMovement != null && !isLunging) waypointMovement.isStopped = false;
            if (animator != null && !isLunging)
            {
                animator.SetBool("isAttacking", false);
            }
            attackTimer = 0f;
        }
    }

    private void TriggerAttackSequence(HeroBase target)
    {
        // 1. Phát animation attack
        if (animator != null)
        {
            animator.SetBool("isAttacking", true);
            animator.Play("attack", 0, 0f);
        }

        // 2. Chạy hiệu ứng lướt nhẹ sang trái 1 chút và lùi về vị trí cũ
        if (!isLunging)
        {
            StartCoroutine(Co_LungeAttack());
        }
    }

    private IEnumerator Co_LungeAttack()
    {
        isLunging = true;
        Vector3 startPos = transform.position;
        Vector3 targetLungePos = startPos + Vector3.left * lungeDistance;

        // Phase 1: Di chuyển lướt nhanh sang trái
        float elapsed = 0f;
        while (elapsed < lungeForwardDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lungeForwardDuration;
            transform.position = Vector3.Lerp(startPos, targetLungePos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        transform.position = targetLungePos;

        // Phase 2: Lùi trở về vị trí ban đầu
        elapsed = 0f;
        while (elapsed < lungeReturnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lungeReturnDuration;
            transform.position = Vector3.Lerp(targetLungePos, startPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        transform.position = startPos;
        isLunging = false;
    }

    // =========================================================================
    // các hàm nhận Animation Event từ Unity (hỗ trợ nhiều tên hàm phổ biến trong Event Dropdown)
    // =========================================================================
    public void OnAttackHit() => ExecuteDamageToTarget();
    public void OnHit() => ExecuteDamageToTarget();
    public void AttackEvent() => ExecuteDamageToTarget();
    public void Hit() => ExecuteDamageToTarget();
    public void DealDamage() => ExecuteDamageToTarget();
    public void PerformMeleeAttack() => ExecuteDamageToTarget();

    public void ExecuteDamageToTarget()
    {
        HeroBase target = currentTarget != null ? currentTarget : FindTargetCellInRange();
        if (target != null && target.health > 0)
        {
            int damage = enemyBase != null ? enemyBase.damage : 10;
            if (damage <= 0) damage = 10;

            target.TakeDamage(damage);
            Debug.Log($"🦠 PheCau tung đòn Animation Event gây {damage} sát thương lên {target.name}!");
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
            if (hero == null || !hero || hero.gameObject == null || !hero.gameObject.activeInHierarchy || hero.health <= 0) continue;
            if (!DragAndDrop.IsUnitActiveOnBoard(hero.gameObject)) continue;

            Vector3 heroPos = hero.transform.position;

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
}
