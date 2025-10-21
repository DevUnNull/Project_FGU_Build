using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementForce : MonoBehaviour
{
    public float moveForce = 10f;
    public float maxSpeed = 5f;
    public float jumpForce = 5f;
    public Transform groundCheck;      // điểm kiểm tra chạm đất
    public float groundRadius = 0.2f;  // bán kính kiểm tra
    public LayerMask groundLayer;      // layer mặt đất

    private Rigidbody2D rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Kiểm tra nhảy ở đây để phản hồi nhanh hơn (Update chứ không phải FixedUpdate)
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");

        // Thêm lực để di chuyển ngang
        if (Mathf.Abs(rb.linearVelocity.x) < maxSpeed)
        {
            rb.AddForce(Vector2.right * h * moveForce);
        }

        // Giữ nhân vật không bị trôi (giảm tốc khi không nhấn)
        if (Mathf.Abs(h) < 0.1f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.9f, rb.linearVelocity.y);
        }

        // Kiểm tra chạm đất bằng OverlapCircle
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
    }

    void Jump()
    {
        // Xóa vận tốc rơi cũ trước khi nhảy
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
}
