using UnityEngine;

public class WaypointMovement : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed ;
    public bool loop = true;

    private int currentIndex = 0;
    private bool isForward = true;

    public _Enemy enemy;
    // Phương thức công khai để các lớp khác có thể gọi
    public void Start()
    {
        enemy = GetComponent<_Enemy>();
        speed = enemy.speed;
        Debug.Log("Speed " + speed);
    }
    public void UpdateMovement()
    {
        // Thêm kiểm tra null tại đây
        if (waypoints == null || waypoints.Length == 0)
        {
            return;
        }

        Transform targetPoint = waypoints[currentIndex];
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPoint.position,
            speed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.05f)
        {
            if (loop)
            {
                currentIndex = (currentIndex + 1) % waypoints.Length;
            }
            else
            {
                if (isForward)
                {
                    currentIndex++;
                    if (currentIndex >= waypoints.Length)
                    {
                        currentIndex = waypoints.Length - 2;
                        isForward = false;
                    }
                }
                else
                {
                    currentIndex--;
                    if (currentIndex < 0)
                    {
                        currentIndex = 1;
                        isForward = true;
                    }
                }
            }
        }
    }
    public void setCurrentIndex()
    {
        currentIndex = 0;
    }
}