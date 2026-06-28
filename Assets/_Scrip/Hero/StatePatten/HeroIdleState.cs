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
        // Tìm kiếm enemy gần nhất trong phạm vi nhìn thấy (detectionRange)
        GameObject enemy = stateMachine.FindTarget();

        if (enemy != null)
        {
            // Đã thấy địch -> Quay mặt về phía địch chuẩn bị
            stateMachine.FaceTowards(enemy.transform.position);

            // Kiểm tra xem địch đã vào tầm đánh (attackRange) chưa
            float distance = Vector2.Distance(hero.transform.position, enemy.transform.position);
            if (distance <= stateMachine.attackRange)
            {
                // Địch đã vào tầm đánh -> Chuyển sang trạng thái tấn công
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
