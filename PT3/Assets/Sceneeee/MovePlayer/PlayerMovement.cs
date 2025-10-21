using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;
    private Vector3 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Nhập hướng từ bàn phím
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Vector di chuyển theo hướng camera (nếu cần)
        moveInput = new Vector3(horizontal, 0f, vertical).normalized;
    }

    void FixedUpdate()
    {
        // Di chuyển bằng MovePosition để tránh xung đột vật lý
        Vector3 moveVelocity = moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveVelocity);
    }
}
