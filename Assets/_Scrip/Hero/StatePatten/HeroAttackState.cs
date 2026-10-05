using UnityEngine;

/// <summary>
/// Trạng thái Attack của Hero (Tế bào):
/// - Khi vào Attack State: Bật animator "IsAttack" = true.
/// - Tại thời điểm chém (0.25s): Tự động/Animation Event gọi PerformAttack() để gây sát thương chuẩn xác.
/// - Khi động tác hoàn tất (0.42s): Tự động gọi FinishAttackAndRest() để tắt "IsAttack",
///   đặt cooldownTimer (1.2s+) và chuyển về Idle State đứng nghỉ.
/// </summary>
public class HeroAttackState : IHeroState
{
    private HeroStateMachine stateMachine;
    private HeroBase hero;

    private GameObject currentTarget;
    private float attackTimer;
    private bool hasFiredInThisAttack = false;

    public HeroAttackState(HeroStateMachine stateMachine, HeroBase hero)
    {
        this.stateMachine = stateMachine;
        this.hero = hero;
    }

    public void EnterState()
    {
        Debug.Log("Hero: Enter Attack State");
        hasFiredInThisAttack = false;
        attackTimer = 0f;

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

        // 🎯 3. Đảm bảo gây sát thương đúng frame chém (0.25f) nếu Animation Event bị bỏ qua
        if (!hasFiredInThisAttack && attackTimer >= 0.25f)
        {
            PerformAttack();
        }

        // 🛑 4. ĐỘNG TÁC CHÉM HOÀN TẤT (0.42f) -> Bắt buộc kết thúc đợt đánh & về Idle nghỉ
        if (attackTimer >= 0.42f)
        {
            FinishAttackAndRest();
        }
    }

    public void ExitState()
    {
        Debug.Log("Hero: Exit Attack State");
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
            Debug.Log($"Hero attacks {currentTarget.name} for {hero.damage} damage!");

            string unitName = hero.gameObject.name;
            bool isMeleeUnit = unitName.Contains("BachCau") || unitName.Contains("DaiThucBao");

            var launcher = hero.GetComponent<ProjectileLauncher>();
            if (launcher != null && launcher.CanLaunch && !isMeleeUnit)
            {
                launcher.Launch(currentTarget.transform, hero.damage);
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
                    enemy.TakeDamage(hero.damage);
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
