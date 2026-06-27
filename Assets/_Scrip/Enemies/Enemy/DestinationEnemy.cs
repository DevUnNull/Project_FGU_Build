using UnityEngine;

/// <summary>
/// Xử lý khi enemy đến đích (Destination Point)
/// Tuân theo Single Responsibility - chỉ xử lý logic khi enemy đến đích
/// </summary>
public class DestinationEnemy : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damagePerEnemy = 1; // Máu bị mất mỗi enemy đến đích

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Lấy WaypointMovement của enemy vừa chạm
            WaypointMovement enemyMovement = other.GetComponent<WaypointMovement>();
            if (enemyMovement != null)
            {
                enemyMovement.setCurrentIndex();
            }

            // Trừ máu khi enemy đến đích (Single Responsibility - chỉ làm 1 việc)
            if (LifeManager.Instance != null)
            {
                LifeManager.Instance.TakeDamage(damagePerEnemy);
            }
            else
            {
                Debug.LogWarning("LifeManager.Instance is null! Không thể trừ máu.");
            }

            // Sau đó return enemy về pool
            // (Tracking đã được thực hiện trong MultiEnemyPool.ReturnToPool)
            MultiEnemyPool.Instance.ReturnToPool(other.gameObject);
        }
    }
}
