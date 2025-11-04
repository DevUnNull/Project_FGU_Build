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

                // Nếu target nằm ngoài phạm vi phát hiện
                if (distanceToTarget > stateMachine.detectionRange)
                {
                    // Target quá xa → bỏ target
                    Debug.Log($"Target too far ({distanceToTarget}), releasing target");
                    currentTarget = null;
                }
                // Nếu target nằm ngoài phạm vi tấn công thì cũng không làm gì
                // Animation Event sẽ tự động gọi PerformAttack() khi animation chạy
            }
        }

        // Nếu không có target, tìm target mới
        if (currentTarget == null)
        {
            // Tìm enemy gần nhất
            GameObject nearestEnemy = FindNearestEnemy();

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
            Debug.Log($"Locked on new target: {currentTarget.name}");
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
 
        // Lấy component _Enemy để gây damage
        _Enemy enemy = currentTarget.GetComponent<_Enemy>();
        
        // Debug: Kiểm tra xem có tìm thấy component _Enemy không
        if (enemy == null)
        {
            Debug.LogWarning($"Cannot find _Enemy component on {currentTarget.name}!");
            // Thử lấy từ child nếu component ở child
            enemy = currentTarget.GetComponentInChildren<_Enemy>();
        }
        
        // Nếu enemy có component _Enemy
        if (enemy != null)
        {
            // Gọi hàm TakeDamage của enemy với damage từ hero
            enemy.TakeDamage(hero.damage);
            Debug.Log($"{currentTarget.name} took {hero.damage} damage! Health remaining: {enemy.health}");
        }
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
                Debug.Log($"Found potential target: {nearestEnemy.name} at distance {distance}");
            }
        }

        // Trả về enemy gần nhất (null nếu không có enemy nào)
        return nearestEnemy;
    }
}
