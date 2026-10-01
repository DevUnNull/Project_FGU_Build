using UnityEngine;

/// <summary>
/// Viên đạn tự dẫn mục tiêu: di chuyển liên tục về transform của target.
/// Khi chạm enemy (hoặc đủ gần), gây damage và tự hủy.
/// </summary>
public class HomingProjectile : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float hitRadius = 0.4f;

    private Transform target;     // mục tiêu hiện tại (enemy)
    private int damage;           // sát thương gây ra khi trúng
    private float targetX = 100f;
    private float initialY;

    public void Initialize(Transform target, int damage, float speedOverride = -1f)
    {
        this.target = target;
        this.damage = damage;
        if (target != null) targetX = target.position.x;
        if (speedOverride > 0f) speed = speedOverride;

        initialY = transform.position.y;
        Destroy(gameObject, 4f); // Tự huỷ sau 4s nếu không trúng
    }

    private void Update()
    {
        if (target != null && target.gameObject.activeInHierarchy)
        {
            targetX = target.position.x;
        }

        // ✅ Di chuyển BẮN THẲNG THEO TRỤC X NGANG (bên phải)
        Vector3 currentPos = transform.position;
        currentPos.x += speed * Time.deltaTime;
        currentPos.y = initialY; // Giữ nguyên hàng Y
        transform.position = currentPos;

        // Check trúng mục tiêu khi vượt qua hoặc gần vị trí targetX
        if (transform.position.x >= targetX || IsEnemyNear())
        {
            ApplyDamage();
        }
    }

    private bool IsEnemyNear()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, hitRadius);
        foreach (var col in hits)
        {
            _Enemy enemy = col.GetComponent<_Enemy>() ?? col.GetComponentInParent<_Enemy>();
            if (enemy != null && enemy.health > 0)
            {
                enemy.TakeDamage(damage);
                Destroy(gameObject);
                return true;
            }
        }
        return false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        _Enemy enemy = other.GetComponent<_Enemy>() ?? other.GetComponentInParent<_Enemy>();
        if (enemy != null && enemy.health > 0)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    private void ApplyDamage()
    {
        if (target != null)
        {
            var enemy = target.GetComponent<_Enemy>() ?? target.GetComponentInChildren<_Enemy>();
            if (enemy != null && enemy.health > 0)
            {
                enemy.TakeDamage(damage);
            }
        }
        Destroy(gameObject);
    }
}


