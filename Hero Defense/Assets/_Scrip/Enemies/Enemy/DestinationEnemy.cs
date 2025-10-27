using UnityEngine;

public class DestinationEnemy : MonoBehaviour
{
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

            // Sau đó return enemy về pool
            MultiEnemyPool.Instance.ReturnToPool(other.gameObject);
        }
    }
}
