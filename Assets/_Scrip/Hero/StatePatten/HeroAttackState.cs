// Import thư viện Unity cơ bản
using UnityEngine;


// Class HeroAttackState - implement trạng thái Attack (tấn công) của Hero
// Khi ở Attack State, Hero sẽ lock vào target và tấn công theo attackRate
public class HeroAttackState : IHeroState
{
    // Tham chiếu đến state machine để có thể chuyển state
    private HeroStateMachine stateMachine;
    // Tham chiếu đến HeroBase để truy cập thông tin Hero
    private HeroBase hero;

    // Biến lưu trữ mục tiêu hiện tại đang tấn công
    private GameObject currentTarget;
    // Timer để đếm thời gian giữa các lần tấn công
    private float attackTimer;

    // Constructor - khởi tạo state với stateMachine và hero
    public HeroAttackState(HeroStateMachine stateMachine, HeroBase hero)
    {
        // Lưu tham chiếu stateMachine
        this.stateMachine = stateMachine;
        // Lưu tham chiếu hero
        this.hero = hero;
    }

    // Hàm được gọi khi chuyển vào Attack State
    public void EnterState()
    {
        // Log ra console để debug
        Debug.Log("Hero: Enter Attack State");
        // Lấy Animator component từ Hero
        Animator animator = hero.GetComponent<Animator>();
        // Nếu có Animator thì set bool IsAttack = true để chạy animation đánh
        if (animator != null) animator.SetBool("IsAttack", true);
        // Reset attack timer về 0
        attackTimer = 0f;
    }

    // Hàm được gọi mỗi frame khi đang ở Attack State
    public void UpdateState()
    {
        // Nếu có target cũ, kiểm tra target đó trước
        if (currentTarget != null)
        {
            // Kiểm tra target còn sống không (còn active trong hierarchy)
            if (!currentTarget.activeInHierarchy)
            {
                // Target đã chết → xóa target
                currentTarget = null;
            }
            else
            {
                // Tính khoảng cách đến target
                float distanceToTarget = Vector2.Distance(hero.transform.position, currentTarget.transform.position);

                // Nếu target nằm ngoài phạm vi tấn công (attackRange)
                if (distanceToTarget > stateMachine.attackRange)
                {
                    // Target đã đi ra khỏi tầm đánh → bỏ target, về lại Idle chờ đợi
                    currentTarget = null;
                    stateMachine.ChangeState(stateMachine.idleState);
                }
                else
                {
                    // Luôn quay mặt về phía target
                    stateMachine.FaceTowards(currentTarget.transform.position);
                }
            }
        }

        // Nếu không có target, tìm target mới
        if (currentTarget == null)
        {
            // Tìm enemy gần nhất
            GameObject nearestEnemy = stateMachine.FindTarget();

            // Nếu không tìm thấy enemy nào
            if (nearestEnemy == null)
            {
                // Không có enemy nào → quay về idle
                Debug.Log("No enemy found, switching to idle");
                stateMachine.ChangeState(stateMachine.idleState);
                return;
            }

            // Lock vào enemy gần nhất
            currentTarget = nearestEnemy;

            // Quay mặt ngay về phía target mới
            stateMachine.FaceTowards(currentTarget.transform.position);
        }
    }

    // Hàm được gọi khi thoát khỏi Attack State
    public void ExitState()
    {
        // Log ra console để debug
        Debug.Log("Hero: Exit Attack State");
        // Xóa target hiện tại
        currentTarget = null;
        // Reset attack animation
        Animator animator = hero.GetComponent<Animator>();
        // Nếu có Animator thì set bool IsAttack = false để quay về idleState
        if (animator != null) animator.SetBool("IsAttack", false);
    }

    // Hàm được gọi từ Animation Event để thực hiện tấn công
    // Hàm này sẽ được gọi từ HeroStateMachine.OnAnimationAttack()
    public void PerformAttack()
    {
        // Kiểm tra target còn tồn tại không
        if (currentTarget == null) return;
 
        // Log ra console để debug
        Debug.Log($"Hero attacks {currentTarget.name} for {hero.damage} damage!");
 
        // Nếu hero có ProjectileLauncher → bắn đạn tự dẫn
        var launcher = hero.GetComponent<ProjectileLauncher>();
        if (launcher != null)
        {
            launcher.Launch(currentTarget.transform, hero.damage);
            return;
        }

        // Mặc định: gây damage trực tiếp (cận chiến)
        _Enemy enemy = currentTarget.GetComponent<_Enemy>() ?? currentTarget.GetComponentInChildren<_Enemy>();
        
        // Kiểm tra lại khoảng cách 1 lần nữa cho chắc trước khi gây sát thương
        float dist = Vector2.Distance(hero.transform.position, currentTarget.transform.position);
        if (enemy != null && dist <= stateMachine.attackRange)
        {
            enemy.TakeDamage(hero.damage);
        }
    }
}
