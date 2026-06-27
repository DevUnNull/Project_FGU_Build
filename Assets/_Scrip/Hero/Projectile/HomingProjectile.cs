using UnityEngine;

/// <summary>
/// Viên đạn tự dẫn mục tiêu: di chuyển liên tục về transform của target.
/// Khi chạm enemy (hoặc đủ gần), gây damage và tự hủy.
/// </summary>
public class HomingProjectile : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float rotateSpeed = 720f; // độ/giây nếu cần quay hướng
    [SerializeField] private float hitRadius = 0.1f;   // khoảng cách coi như trúng mục tiêu
    [SerializeField] private LayerMask enemyLayer;

    private Transform target;     // mục tiêu hiện tại (enemy)
    private int damage;           // sát thương gây ra khi trúng
    private System.Action onHit;  // callback khi trúng mục tiêu (optional)

    public void Initialize(Transform target, int damage, float speedOverride = -1f)
    {
        this.target = target;
        this.damage = damage;
        if (speedOverride > 0f) speed = speedOverride;
    }

    private void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }

        // Hướng đến mục tiêu
        Vector3 dir = (target.position - transform.position);
        float distanceThisFrame = speed * Time.deltaTime;

        // Xoay hướng (nếu có sprite/mesh muốn quay theo)
        if (dir.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }

        // Di chuyển theo hướng mục tiêu
        transform.position += dir.normalized * distanceThisFrame;

        // Kiểm tra trúng mục tiêu theo bán kính
        if (dir.magnitude <= hitRadius)
        {
            ApplyDamage();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            // Nếu dùng collider, chạm enemy bất kỳ trong layer
            var enemy = other.GetComponent<_Enemy>() ?? other.GetComponentInChildren<_Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }

    private void ApplyDamage()
    {
        if (target == null) return;
        var enemy = target.GetComponent<_Enemy>() ?? target.GetComponentInChildren<_Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}


