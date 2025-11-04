// Import thư viện Unity cơ bản
using UnityEngine;

// Class HeroStateMachine - đây là lớp quản lý state machine pattern cho Hero
// State Machine Pattern cho phép Hero chuyển đổi giữa các trạng thái khác nhau (Idle, Attack, v.v.)
public class HeroStateMachine : MonoBehaviour
{
    // Biến lưu trữ state hiện tại mà Hero đang ở
    private IHeroState currentState;

    // Khai báo các state cụ thể của Hero
    // Idle state - trạng thái đứng yên chờ tìm mục tiêu
    public HeroIdleState idleState;
    // Attack state - trạng thái tấn công kẻ địch
    public HeroAttackState attackState;

    // Tham chiếu đến HeroBase component để truy cập thông tin Hero
    private HeroBase hero;

    // Các thông số cấu hình cho state machine
    
    // LayerMask định nghĩa layer nào được coi là enemy
    public LayerMask enemyLayer;
    
    // Phạm vi phát hiện enemy (khoảng cách Hero có thể phát hiện enemy)
    public float detectionRange = 3f;
    
    // Phạm vi tấn công (khoảng cách tối đa Hero có thể tấn công)
    public float attackRange = 1.5f;
    
    // Tốc độ tấn công (số lần tấn công mỗi giây)
    public float attackRate = 1f; // Attacks per second

    // Cờ bật/tắt hiển thị tầm trong Scene View của Unity Editor
    public bool showRanges = true;

    // Hàm Awake được gọi trước Start, dùng để khởi tạo
    void Awake()
    {
        // Lấy component HeroBase từ GameObject hiện tại
        hero = GetComponent<HeroBase>();
        // Kiểm tra nếu không có HeroBase component thì báo lỗi
        if (hero == null)
        {
            Debug.LogError($"{nameof(HeroStateMachine)} requires a HeroBase component on the same GameObject.", this);
        }

        // Khởi tạo các state ở Awake để đảm bảo sẵn sàng trước Start của các component khác
        // Truyền this (stateMachine) và hero vào constructor
        idleState = new HeroIdleState(this, hero);
        attackState = new HeroAttackState(this, hero);
    }

    // Hàm Start được gọi sau tất cả Awake, khởi tạo state ban đầu
    void Start()
    {
        // Nếu idleState null thì sẽ log ở Awake, tránh ChangeState(null)
        // Chuyển sang idle state làm state khởi tạo
        if (idleState != null)
            ChangeState(idleState);
        else
            Debug.LogWarning("Idle state is not initialized.", this);
    }

    // Hàm Update được gọi mỗi frame
    void Update()
    {
        // Gọi UpdateState của state hiện tại nếu không null
        // Toán tử ?. giúp tránh null reference exception
        currentState?.UpdateState();
    }

    // Hàm chuyển đổi state - đây là trái tim của State Machine Pattern
    public void ChangeState(IHeroState newState)
    {
        // Kiểm tra nếu state mới là null thì cảnh báo và return
        if (newState == null)
        {
            Debug.LogWarning("Attempted to change to a null state.", this);
            return;
        }
        // Nếu state mới trùng với state hiện tại thì không cần chuyển
        if (currentState == newState) return;

        // Gọi ExitState để cleanup state cũ (nếu có)
        currentState?.ExitState();
        
        // Cập nhật state hiện tại
        currentState = newState;
        
        // Gọi EnterState để khởi tạo state mới
        currentState.EnterState();
    }

    // Hàm được gọi từ Animation Event để thực hiện tấn công
    // Hàm này sẽ được Animation Event trong animation tấn công gọi khi đến frame đánh
    public void OnAnimationAttack()
    {
        // Chỉ gọi hàm tấn công nếu đang ở Attack State
        if (currentState == attackState)
        {
            attackState.PerformAttack();
        }
    }

    // Hàm vẽ Gizmos để hiển thị tầm trong Scene View của Unity Editor
    void OnDrawGizmosSelected()
    {
        // Nếu tắt hiển thị range thì return
        if (!showRanges) return;

        // Vẽ tầm phát hiện (màu vàng) - Sphere wireframe
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Vẽ tầm tấn công (màu đỏ) - Sphere wireframe
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}