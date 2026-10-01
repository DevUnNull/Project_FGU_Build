using UnityEngine;

/// <summary>
/// Trạng thái Attack của Hero:
/// Khi ở Attack State, Hero chuyển animation sang attack.
/// Khi Animation Event ở frame chỉ định được gọi (hoặc safety timer trigger), đạn được bắn ra.
/// Ngay sau khi bắn đạn xong, Hero lập tức quay về trạng thái Idle để nạp đạn (cooldownTimer).
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
                    stateMachine.ChangeState(stateMachine.idleState);
                    return;
                }
                else
                {
                    stateMachine.FaceTowards(currentTarget.transform.position);
                }
            }
        }

        if (currentTarget == null)
        {
            GameObject nearestEnemy = stateMachine.FindTarget();
            if (nearestEnemy == null)
            {
                stateMachine.ChangeState(stateMachine.idleState);
                return;
            }

            currentTarget = nearestEnemy;
            stateMachine.FaceTowards(currentTarget.transform.position);
        }

        // Safety Fallback: Nếu không có Animation Event được gọi sau 0.8s -> Tự động gọi PerformAttack
        if (!hasFiredInThisAttack && attackTimer >= 0.8f)
        {
            PerformAttack();
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
    }

    // Hàm được gọi từ Animation Event để thực hiện bắn đạn
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

        // ✅ Sau khi bắn đạn xong -> Chuyển sang thời gian nạp đạn (cooldownTimer) và về Idle ngay lập tức
        float reloadTime = stateMachine.attackRate > 0f ? (1f / stateMachine.attackRate) : 1f;
        stateMachine.cooldownTimer = reloadTime;
        stateMachine.ChangeState(stateMachine.idleState);
    }
}
