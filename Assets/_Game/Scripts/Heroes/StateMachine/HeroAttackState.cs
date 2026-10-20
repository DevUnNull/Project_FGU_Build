using UnityEngine;

/// <summary>
/// Trạng thái Attack của Hero (Tế bào):
/// - Khi vào Attack State: Bật animator "IsAttack" = true.
/// - Tại thời điểm chém (60% độ dài clip tấn công): Tự động/Animation Event gọi PerformAttack() để gây sát thương chuẩn xác.
/// - Khi động tác hoàn tất (hết clip tấn công): Tự động gọi FinishAttackAndRest() để tắt "IsAttack",
///   đặt cooldownTimer (1.2s+) và chuyển về Idle State đứng nghỉ.
/// </summary>
public class HeroAttackState : IHeroState
{
    private HeroStateMachine stateMachine;
    private HeroBase hero;

    private GameObject currentTarget;
    private float attackTimer;
    private bool hasFiredInThisAttack = false;

    // Thời điểm bắn & kết thúc đòn đánh, tính theo độ dài clip tấn công thật (đã tính tốc độ Animator)
    private const float FirePointRatio = 0.6f;
    private float attackDuration;
    private float fireTime;

    public HeroAttackState(HeroStateMachine stateMachine, HeroBase hero)
    {
        this.stateMachine = stateMachine;
        this.hero = hero;
    }

    public void EnterState()
    {
        hasFiredInThisAttack = false;
        attackTimer = 0f;
        attackDuration = stateMachine.GetAttackDuration();
        fireTime = attackDuration * FirePointRatio;

        Animator animator = hero.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("IsAttack", true);
        }
    }

    public void UpdateState()
    {
        if (!DragAndDrop.IsUnitActiveOnBoard(hero.gameObject))
        {
            currentTarget = null;
            stateMachine.ChangeState(stateMachine.idleState);
            return;
        }

        attackTimer += Time.deltaTime;

        // 1. Kiểm tra mục tiêu hiện tại
        if (currentTarget != null)
        {
            if (!currentTarget.activeInHierarchy)
            {
                currentTarget = null;
            }
            else
            {
                float deltaY = Mathf.Abs(currentTarget.transform.position.y - hero.transform.position.y);
                float distanceToTarget = Vector2.Distance(hero.transform.position, currentTarget.transform.position);

                var launcher = hero.GetComponent<ProjectileLauncher>();
                float effectiveRange = (launcher != null && launcher.CanLaunch) 
                    ? Mathf.Max(stateMachine.attackRange, stateMachine.detectionRange) 
                    : stateMachine.attackRange;

                if (deltaY > 0.6f || distanceToTarget > effectiveRange)
                {
                    currentTarget = null;
                    FinishAttackAndRest();
                    return;
                }
                else
                {
                    stateMachine.FaceTowards(currentTarget.transform.position);
                }
            }
        }

        // 2. Nếu chưa có mục tiêu -> Tìm mục tiêu mới
        if (currentTarget == null)
        {
            GameObject nearestEnemy = stateMachine.FindTarget();
            if (nearestEnemy == null)
            {
                FinishAttackAndRest();
                return;
            }

            currentTarget = nearestEnemy;
            stateMachine.FaceTowards(currentTarget.transform.position);
        }

        // 🎯 3. Đảm bảo gây sát thương đúng frame chém nếu Animation Event bị bỏ qua
        if (!hasFiredInThisAttack && attackTimer >= fireTime)
        {
            PerformAttack();
        }

        // 🛑 4. ĐỘNG TÁC CHÉM HOÀN TẤT (hết clip) -> Bắt buộc kết thúc đợt đánh & về Idle nghỉ
        if (attackTimer >= attackDuration)
        {
            FinishAttackAndRest();
        }
    }

    public void ExitState()
    {
        currentTarget = null;
        Animator animator = hero.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("IsAttack", false);
        }

        // Thời gian nạp đạn / khoảng nghỉ giữa 2 đợt đánh (tính bằng giây)
        float reloadTime = stateMachine.attackInterval > 0f ? stateMachine.attackInterval : 1f;
        stateMachine.cooldownTimer = reloadTime;
    }

    /// <summary>
    /// Hàm thực hiện gây sát thương / bắn đạn (Gọi từ Animation Event hoặc tự động ở frame 0.25s)
    /// </summary>
    public void PerformAttack()
    {
        if (hasFiredInThisAttack || !DragAndDrop.IsUnitActiveOnBoard(hero.gameObject)) return;
        hasFiredInThisAttack = true;

        if (currentTarget == null)
        {
            currentTarget = stateMachine.FindTarget();
        }

        if (currentTarget != null)
        {
            // Hiệu ứng ra đòn: vươn cao & thon lại một nhịp ngắn
            UnitJuice.GetOrAdd(hero.gameObject).Punch(new Vector2(-0.08f, 0.1f), 0.16f);

            bool isMeleeUnit = hero.HeroData != null && hero.HeroData.isMelee;

            var launcher = hero.GetComponent<ProjectileLauncher>();
            if (launcher != null && launcher.CanLaunch && !isMeleeUnit)
            {
                launcher.Launch(currentTarget.transform, hero.CurrentDamage);
            }
            else
            {
                _Enemy enemy = currentTarget.GetComponent<_Enemy>() ?? currentTarget.GetComponentInChildren<_Enemy>();
                float dist = Vector2.Distance(hero.transform.position, currentTarget.transform.position);
                float effectiveRange = (launcher != null && launcher.CanLaunch) 
                    ? Mathf.Max(stateMachine.attackRange, stateMachine.detectionRange) 
                    : stateMachine.attackRange;

                if (enemy != null && dist <= effectiveRange)
                {
                    enemy.TakeDamage(hero.CurrentDamage);
                }
            }
        }
    }

    /// <summary>
    /// Hoàn tất lượt đánh và bắt buộc đưa Hero về trạng thái Idle nghỉ ngơi
    /// </summary>
    private void FinishAttackAndRest()
    {
        if (!hasFiredInThisAttack)
        {
            PerformAttack();
        }

        float reloadTime = stateMachine.attackInterval > 0f ? stateMachine.attackInterval : 1f;
        stateMachine.cooldownTimer = reloadTime;
        stateMachine.ChangeState(stateMachine.idleState);
    }
}
