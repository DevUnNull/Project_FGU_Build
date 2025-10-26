using UnityEngine;

public class WaypointMovement : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed;
    public bool loop = true;

    private int currentIndex = 0;
    private bool isForward = true;

    public _Enemy enemy;

    private Vector3 lastPosition;

    public void Start()
    {
        enemy = GetComponent<_Enemy>();
        speed = enemy.speed;
        lastPosition = transform.position;
        Debug.Log("Speed " + speed);
    }

    public void UpdateMovement()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        Transform targetPoint = waypoints[currentIndex];

        // ✅ Xoay hướng ngay lập tức theo hướng di chuyển tới targetPoint
        Vector3 moveDir = (targetPoint.position - transform.position).normalized;
        if (moveDir.x > 0.01f)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (moveDir.x < -0.01f)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        // ✅ Di chuyển nhân vật
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPoint.position,
            speed * Time.deltaTime
        );

        // ✅ Cập nhật vị trí (có thể giữ lại nếu script khác cần)
        lastPosition = transform.position;

        // ✅ Kiểm tra đến waypoint thì đổi hướng
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

    void Update()
    {
        UpdateMovement();
    }
}
