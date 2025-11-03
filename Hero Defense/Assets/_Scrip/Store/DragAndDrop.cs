using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    // 🎯 Kéo thả
    private Vector3 offset;               // Khoảng cách giữa chuột và quân cờ
    private bool isDragging = false;      // Đang kéo hay không
    private Vector2 previousPosition;     // Vị trí trước đó (dùng để hoàn tác)

    // 💰 Mua & đặt
    private bool isPlacedOnBoard = false; // Đã đặt vào bàn chưa
    public bool isBuy = false;            // Đã mua chưa (mặc định: chưa mua)
    private Transform originalParent;

    // 💵 Dữ liệu giá
    private _Hero priceData;
    private int price;

    private void Start()
    {
        previousPosition = transform.position;

        priceData = GetComponent<_Hero>();
        price = priceData != null ? priceData.price : 0;
    }

    private void OnMouseDown()
    {
        if (!enabled) return;

        // Tắt Animator khi kéo
        Animator anim = GetComponent<Animator>();
        if (anim) anim.enabled = false;

        originalParent = transform.parent;
        transform.SetParent(null);

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - mouseWorld;
        offset.z = 0;

        isDragging = true;
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 newPosition = mouseWorld + offset;
        newPosition.z = 0;
        transform.position = newPosition;
    }

    private void OnMouseUp()
    {
        isDragging = false;

        // 🧩 Kiểm tra vùng huỷ trước
        if (DestroyUnitTrigger.isOverDestroyZone)
        {
            // ✅ Chỉ bán nếu đã mua
            if (isBuy)
            {
                Debug.Log("🗑️ Tướng bị huỷ do thả vào vùng huỷ - Hoàn tiền: " + price);
                GoldManager.Instance.AddGold(price);
                Destroy(gameObject);
                return;
            }
            else
            {
                // ❌ Chưa mua, không cho bán
                Debug.Log("❌ Chưa mua tướng, không thể bán!");
                transform.position = previousPosition;
                return;
            }
        }

        // 🔍 Tìm Tile gần nhất
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, 0.5f);
        Transform closestTile = null;
        float shortestDistance = Mathf.Infinity;

        foreach (var col in nearbyColliders)
        {
            if (col == null || col.gameObject == null) continue;
            if (!col.CompareTag("Tile") && !col.CompareTag("TileEdge")) continue;

            float distance = Vector2.Distance(transform.position, col.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestTile = col.transform;
            }
        }

        // ⚠️ Không tìm thấy tile hợp lệ
        if (closestTile == null)
        {
            HandleInvalidPlacement();
            return;
        }

        // 🚫 Không cho đặt ở viền ngoài
        if (closestTile.CompareTag("TileEdge"))
        {
            Debug.Log("⛔ Không thể đặt tướng ở viền ngoài");
            transform.position = previousPosition;
            return;
        }

        // 💰 Kiểm tra tiền nếu chưa mua
        if (!isBuy && !GoldManager.Instance.HasEnoughGold(price))
        {
            Debug.Log($"❌ Không đủ tiền mua {gameObject.name}, cần {price}");
            transform.position = previousPosition;
            return;
        }

        // ✅ Đặt thành công
        transform.position = closestTile.position;
        previousPosition = closestTile.position;
        isPlacedOnBoard = true;

        // Nếu vừa mua → trừ tiền
        if (!isBuy) 
        {
            isBuy = true;
            GoldManager.Instance.SpendGold(price);
            Debug.Log("ban da bi tru:" + price);
        }

        // Bật lại Animator
        Animator animator = GetComponent<Animator>();
        if (animator) animator.enabled = true;
    }

    /// <summary>
    /// Xử lý khi thả sai vị trí (không có tile)
    /// </summary>
    private void HandleInvalidPlacement()
    {
        if (!isPlacedOnBoard)
        {
            Debug.Log("Kéo ra nhưng không thả vào bàn → huỷ / trả lại vị trí cũ");
            transform.position = previousPosition;
        }
        else
        {
            // Đã từng được đặt → trở lại chỗ cũ
            transform.position = previousPosition;
        }
    }

    // Debug hỗ trợ hiển thị vùng check tile
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
