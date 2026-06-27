using UnityEngine;

public class WayEnemy : MonoBehaviour, IPoolable  // Giả sử kế thừa _Enemy nếu cần
{
    public WaypointMovement waypointMovement;
    private EnemyType myType;  // Set ở prefab Inspector

    EnemyType IPoolable.GetEnemyType() => myType;

    void Reset()
    {
        waypointMovement.setCurrentIndex();
        // Reset health, etc. từ IEnemy
    }

    public void Init(WaypointPath path)  // Public để Spawner gọi
    {
        waypointMovement.waypoints = path.GetWaypoints();
    }

    // Start/Update giữ nguyên, nhưng di chuyển Init vào public method
    void Start()
    {
        waypointMovement = GetComponent<WaypointMovement>();
        // Không set waypoints ở đây nữa!
    }

    void IPoolable.Reset()
    {
        waypointMovement.setCurrentIndex();
    }
}