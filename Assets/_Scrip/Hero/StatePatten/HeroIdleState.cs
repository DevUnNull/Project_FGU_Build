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
        // Tìm kiếm enemy gần nhất trong phạm vi
        GameObject enemy = FindNearestEnemy();

        // Nếu tìm thấy enemy thì chuyển sang Attack State
        if (enemy != null)
        {
            // Chuyển sang attack state để bắt đầu tấn công
            stateMachine.ChangeState(stateMachine.attackState);
        }
        // Nếu không tìm thấy enemy thì tiếp tục ở Idle State
    }

    // Hàm được gọi khi thoát khỏi Idle State
    public void ExitState()
    {
        // Log ra console để debug
        Debug.Log("Hero: Exit Idle State");
    }

    // Hàm tìm enemy gần nhất trong phạm vi phát hiện
    private GameObject FindNearestEnemy()
    {
        // Tìm tất cả enemy trong phạm vi phát hiện bằng OverlapCircleAll
        // Sử dụng detectionRange và enemyLayer từ stateMachine
        Collider2D[] enemies = Physics2D.OverlapCircleAll(hero.transform.position, stateMachine.detectionRange, stateMachine.enemyLayer);

        // Biến lưu enemy gần nhất và khoảng cách đến enemy đó
        GameObject nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        // Duyệt qua tất cả enemy tìm được
        foreach (Collider2D enemyCollider in enemies)
        {
            // Tính khoảng cách từ Hero đến enemy hiện tại
            float distance = Vector2.Distance(hero.transform.position, enemyCollider.transform.position);
            // Nếu enemy này gần hơn enemy gần nhất trước đó
            if (distance < nearestDistance)
            {
                // Cập nhật enemy gần nhất và khoảng cách
                nearestDistance = distance;
                nearestEnemy = enemyCollider.gameObject;
            }
        }

        // Trả về enemy gần nhất (null nếu không có enemy nào)
        return nearestEnemy;
    }
}
