using UnityEngine;

/// <summary>
/// Quản lý từng ô Tile trên bàn cờ PvZ:
/// - Trạng thái chiếm ô (occupiedUnit)
/// - Đặt nhân vật chính xác tại TÂM ô Tile (transform.position)
/// - Hiệu ứng Hover Highlight (Xanh lá = hợp lệ, Đỏ = ô bận/không hợp lệ)
/// </summary>
public class TileCell : MonoBehaviour
{
    public int row;
    public int col;
    public bool isEdge = false;

    private GameObject occupiedUnit;
    public GameObject OccupiedUnit => occupiedUnit;
    public bool IsOccupied => occupiedUnit != null && occupiedUnit.activeInHierarchy;

    private SpriteRenderer spriteRenderer;
    private Color originalColor = Color.white;
    private bool hasOriginalColor = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            hasOriginalColor = true;
        }

        // Tự động gán Tag nếu chưa gán
        if (!CompareTag("Tile") && !CompareTag("TileEdge"))
        {
            tag = isEdge ? "TileEdge" : "Tile";
        }
    }

    /// <summary>
    /// Lấy vị trí TÂM CHÍNH XÁC của ô Tile để đặt nhân vật vào giữa.
    /// </summary>
    public Vector3 CenterPosition
    {
        get
        {
            Vector3 pos = transform.position;
            pos.z = 0f;
            return pos;
        }
    }

    public void Occupy(GameObject unit)
    {
        occupiedUnit = unit;
    }

    public void Free()
    {
        occupiedUnit = null;
    }

    public void SetHighlight(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }

    public void ResetHighlight()
    {
        if (spriteRenderer != null && hasOriginalColor)
        {
            spriteRenderer.color = originalColor;
        }
    }
}
