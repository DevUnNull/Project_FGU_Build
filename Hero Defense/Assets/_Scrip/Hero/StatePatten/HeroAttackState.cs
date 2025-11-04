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

    // Lưu scale gốc để flip hướng nhìn đúng trên trục X
    private Vector3 originalLocalScale;

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

        // Ghi lại scale gốc của hero để dùng khi flip hướng
        originalLocalScale = hero.transform.localScale;
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
                // Luôn quay mặt về phía target (nếu target vẫn còn hợp lệ)
                if (currentTarget != null)
                {
                    FaceTowards(currentTarget.transform.position);
                }

                // Nếu target nằm ngoài phạm vi tấn công thì cũng không làm gì ở đây
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

            // Quay mặt ngay về phía target mới
            FaceTowards(currentTarget.transform.position);
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
        if (enemy != null) enemy.TakeDamage(hero.damage);
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

    // Quay mặt hero về phía 1 điểm (2D): flip localScale X theo hướng deltaX
    private void FaceTowards(Vector3 targetPosition)
    {
        float deltaX = targetPosition.x - hero.transform.position.x;
        if (Mathf.Approximately(deltaX, 0f)) return;

        Vector3 scale = hero.transform.localScale;
        float absX = Mathf.Abs(originalLocalScale.x) > 0f ? Mathf.Abs(originalLocalScale.x) : Mathf.Abs(scale.x);
        scale.x = deltaX > 0f ? absX : -absX; // nhìn phải nếu target ở bên phải, ngược lại nhìn trái
        hero.transform.localScale = scale;
    }
}
