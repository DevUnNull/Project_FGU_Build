// Import thư viện Unity Visual Scripting (có thể không dùng trong file này)
using Unity.VisualScripting;
// Import thư viện Unity cơ bản
using UnityEngine;

// Class HeroIdleState - implement trạng thái Idle (đứng yên) của Hero
// Khi ở Idle State, Hero sẽ đứng yên và tìm kiếm enemy gần nhất
public class HeroIdleState : IHeroState
{
    // Tham chiếu đến state machine để có thể chuyển state
    private HeroStateMachine stateMachine;
    // Tham chiếu đến HeroBase để truy cập thông tin Hero
    private HeroBase hero;

    // Constructor - khởi tạo state với stateMachine và hero
    public HeroIdleState(HeroStateMachine stateMachine, HeroBase hero)
    {
        // Lưu tham chiếu stateMachine
        this.stateMachine = stateMachine;
        // Lưu tham chiếu hero
        this.hero = hero;
    }

    // Hàm được gọi khi chuyển vào Idle State
    public void EnterState()
    {
        // Log ra console để debug
        Debug.Log("Hero: Enter Idle State");
        // Lấy Animator component từ Hero
        Animator animator = hero.GetComponent<Animator>();
        // Nếu có Animator thì set bool IsAttack = false để quay về idleState
        if (animator != null) animator.SetBool("IsAttack", false);
    }

    // Hàm được gọi mỗi frame khi đang ở Idle State
    public void UpdateState()
    {
        // 1. Nếu đang trong thời gian nạp đạn (cooldownTimer > 0) -> Tiếp tục ở lại Idle state và đếm ngược
        if (stateMachine.cooldownTimer > 0f)
        {
            stateMachine.cooldownTimer -= Time.deltaTime;
            return;
        }

        // 2. Nạp đạn xong -> Tìm kiếm enemy gần nhất trong phạm vi
        GameObject enemy = stateMachine.FindTarget();

        if (enemy != null)
        {
            // Đã thấy địch -> Quay mặt về phía địch chuẩn bị
            stateMachine.FaceTowards(enemy.transform.position);

            // Kiểm tra xem địch đã vào tầm đánh chưa
            float distance = Vector2.Distance(hero.transform.position, enemy.transform.position);
            var launcher = hero.GetComponent<ProjectileLauncher>();
            float effectiveRange = (launcher != null && launcher.CanLaunch) 
                ? Mathf.Max(stateMachine.attackRange, stateMachine.detectionRange) 
                : stateMachine.attackRange;

            if (distance <= effectiveRange)
            {
                // Địch đã vào tầm đánh và đã nạp đạn xong -> Chuyển sang trạng thái tấn công
                stateMachine.ChangeState(stateMachine.attackState);
            }
        }
    }

    // Hàm được gọi khi thoát khỏi Idle State
    public void ExitState()
    {
        // Log ra console để debug
        Debug.Log("Hero: Exit Idle State");
    }

}
