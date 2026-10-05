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

    [HideInInspector] public Vector3 originalLocalScale;

    // Các thông số cấu hình cho state machine
    
    // LayerMask định nghĩa layer nào được coi là enemy
    public LayerMask enemyLayer;
    
    // Phạm vi phát hiện enemy (khoảng cách Hero có thể phát hiện enemy)
    public float detectionRange = 3f;
    
    // Phạm vi tấn công (khoảng cách tối đa Hero có thể tấn công)
    public float attackRange = 1.5f;
    
    [Tooltip("Khoảng thời gian giữa 2 lần đánh (tính bằng giây). Đặt 1 = sau 1s đánh lại, 2 = sau 2s đánh lại")]
    public float attackInterval = 1f;

    // Cờ bật/tắt hiển thị tầm trong Scene View của Unity Editor
    public bool showRanges = true;

    public enum DetectionType { Radial, Line }
    [Header("Detection Settings")]
    public DetectionType detectionType = DetectionType.Radial;
    
    [Header("Line Detection Specific Settings")]
    public Vector2 checkDirection = Vector2.right;
    public float checkHeight = 1f;

    // Hàm Awake được gọi trước Start, dùng để khởi tạo
    void Awake()
    {
        originalLocalScale = transform.localScale;
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

    [HideInInspector] public float cooldownTimer = 0f;

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

    public void OnAttackEvent() => OnAnimationAttack();
    public void Shoot() => OnAnimationAttack();
    public void FireBullet() => OnAnimationAttack();

    // Hàm tìm target (kết hợp Physics detection + Vector Projection search + Fallback)
    public virtual GameObject FindTarget()
    {
        // 1. Nếu tướng chưa được mua/chưa đặt lên bàn (đang ở trong shop) -> Không tìm mục tiêu
        if (!DragAndDrop.IsUnitActiveOnBoard(gameObject)) return null;

        // 2. Nếu người chơi chưa ấn nút GỌI WAVE (StartWave) -> Chưa tìm mục tiêu / chưa tấn công
        if (WaveManager.Instance != null && !WaveManager.Instance.IsWaveStarted) return null;

        GameObject nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        // Tính khoảng cách hiệu lực (tính theo độ dài Check Direction nếu > 1, ngược lại dùng Detection Range)
        float absRange = Mathf.Abs(detectionRange);
        float effectiveRange = (checkDirection.magnitude > 1.001f) ? checkDirection.magnitude : (absRange > 0.1f ? absRange : 7f);

        // Chuẩn hóa hướng checkDirection (ví dụ: (24.2, 0) -> hướng (1, 0) với chiều dài 24.2)
        Vector2 dir = checkDirection != Vector2.zero ? checkDirection.normalized : Vector2.right;

        // 2. Physics Detection (Radial hoặc Line theo cấu hình Inspector)
        LayerMask mask = (enemyLayer.value != 0) ? enemyLayer : ~0;

        if (detectionType == DetectionType.Radial)
        {
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, effectiveRange, mask);
            foreach (Collider2D enemyCollider in enemies)
            {
                if (enemyCollider == null || enemyCollider.gameObject == gameObject) continue;
                _Enemy enemyComp = enemyCollider.GetComponent<_Enemy>() ?? enemyCollider.GetComponentInParent<_Enemy>();
                if (enemyComp == null || enemyComp.health <= 0) continue;

                float distance = Vector2.Distance(transform.position, enemyCollider.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemyCollider.gameObject;
                }
            }
        }
        else if (detectionType == DetectionType.Line)
        {
            float boxSize = checkHeight > 0f ? checkHeight : 1f;
            RaycastHit2D[] hits = Physics2D.BoxCastAll(
                transform.position,
                new Vector2(boxSize, boxSize),
                0f,
                dir,
                effectiveRange,
                mask
            );

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null || hit.collider.gameObject == gameObject) continue;
                _Enemy enemyComp = hit.collider.GetComponent<_Enemy>() ?? hit.collider.GetComponentInParent<_Enemy>();
                if (enemyComp == null || enemyComp.health <= 0) continue;

                float distance = Vector2.Distance(transform.position, hit.collider.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = hit.collider.gameObject;
                }
            }
        }

        // 3. Fallback: Tìm trực tiếp tất cả _Enemy bằng phép chiếu Vector (Hỗ trợ 100% mọi hướng Check Direction & Layer)
        if (nearestEnemy == null)
        {
            _Enemy[] allEnemies = FindObjectsOfType<_Enemy>();
            Vector3 heroPos = transform.position;
            Vector2 perp = new Vector2(-dir.y, dir.x);

            foreach (var enemy in allEnemies)
            {
                if (enemy == null || !enemy.gameObject.activeInHierarchy || enemy.health <= 0) continue;

                Vector2 offset = (Vector2)enemy.transform.position - (Vector2)heroPos;

                if (detectionType == DetectionType.Radial)
                {
                    float dist = offset.magnitude;
                    if (dist <= effectiveRange && dist < nearestDistance)
                    {
                        nearestDistance = dist;
                        nearestEnemy = enemy.gameObject;
                    }
                }
                else // Line detection fallback
                {
                    // Khoảng cách tiến tới theo đúng hướng checkDirection
                    float forwardDist = Vector2.Dot(offset, dir);
                    if (forwardDist < -0.3f || forwardDist > effectiveRange) continue;

                    // Khoảng cách vuông góc (chênh lệch hàng/làn)
                    float sideDist = Mathf.Abs(Vector2.Dot(offset, perp));
                    float maxSide = checkHeight > 0f ? (checkHeight * 0.8f) : 1.0f;
                    if (sideDist > maxSide) continue;

                    float dist = offset.magnitude;
                    if (dist < nearestDistance)
                    {
                        nearestDistance = dist;
                        nearestEnemy = enemy.gameObject;
                    }
                }
            }
        }

        return nearestEnemy;
    }

    // Quay mặt hero về phía 1 điểm (2D): Sử dụng SpriteRenderer.flipX thay vì thay đổi transform.localScale
    // Đảm bảo giữ nguyên 100% tỉ lệ kích thước (Aspect Ratio) của Tế bào và các UI con (không bị méo hình)
    public void FaceTowards(Vector3 targetPosition)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();

        if (sr != null)
        {
            float deltaX = targetPosition.x - transform.position.x;
            if (!Mathf.Approximately(deltaX, 0f))
            {
                sr.flipX = (deltaX < 0f);
            }
        }
    }

    // Hàm vẽ Gizmos để hiển thị tầm trong Scene View của Unity Editor
    void OnDrawGizmosSelected()
    {
        // Nếu tắt hiển thị range thì return
        if (!showRanges) return;

        float absRange = Mathf.Abs(detectionRange);
        float effectiveRange = (checkDirection.magnitude > 1.001f) ? checkDirection.magnitude : (absRange > 0.1f ? absRange : 7f);
        Vector2 dir = checkDirection != Vector2.zero ? checkDirection.normalized : Vector2.right;

        if (detectionType == DetectionType.Radial)
        {
            // Tầm phát hiện (màu vàng) - Hình tròn
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, effectiveRange);

            // Tầm tấn công (màu đỏ) - Hình tròn
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
        else if (detectionType == DetectionType.Line)
        {
            float boxSize = checkHeight > 0f ? checkHeight : 1f;
            Vector3 start = transform.position;

            // 1. Tầm phát hiện (màu vàng) - Khung hình chữ nhật đi thẳng
            Vector3 endDetect = start + (Vector3)dir * effectiveRange;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(start, endDetect);
            Gizmos.DrawWireCube((start + endDetect) * 0.5f, new Vector3(effectiveRange, boxSize, 0f));

            // 2. Tầm tấn công (màu đỏ) - Đường thẳng và khung hình chữ nhật đi thẳng
            float effAttackRange = attackRange > 0.1f ? attackRange : effectiveRange;
            Vector3 endAttack = start + (Vector3)dir * effAttackRange;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(start, endAttack);
            Gizmos.DrawWireCube((start + endAttack) * 0.5f, new Vector3(effAttackRange, boxSize, 0f));
        }
    }
}