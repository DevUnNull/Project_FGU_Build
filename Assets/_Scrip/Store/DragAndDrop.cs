using UnityEngine;

/// <summary>
/// Cơ chế Kéo & Đặt Tế Bào (Cell) chuẩn PvZ:
/// - Đặt Tế bào CHÍNH XÁC VÀO TÂM CHÍNH GIỮA ô Tile (transform.position = tile.CenterPosition).
/// - Hiệu ứng Hover Highlight thực thời (Xanh lá = Đặt được, Đỏ = Ô bận/không hợp lệ).
/// - Quản lý chiếm giữ ô (Occupied State), ngăn không cho đặt chồng nhiều tướng lên 1 ô.
/// - Hoàn tác vị trí cũ an toàn nếu kéo thả sai hoặc không đủ tiền.
/// </summary>
public class DragAndDrop : MonoBehaviour
{
    private Vector3 offset;               // Khoảng cách giữa con trỏ và nhân vật
    private bool isDragging = false;      // Đang kéo hay không
    private Vector3 previousPosition;     // Vị trí cũ (để hoàn tác khi thả sai)

    // 💰 Mua & Đặt
    private bool isPlacedOnBoard = false; // Đã đặt trên bàn cờ chưa
    public bool isBuy = false;            // Đã thanh toán mua chưa
    public bool IsPlacedOnBoard => isPlacedOnBoard;

    private TileCell currentTile;         // Ô TileCell hiện tại đang đứng
    public TileCell CurrentTile => currentTile;

    private _Hero priceData;
    private int price;
    private Vector3 originalScale;

    /// <summary>
    /// Kiểm tra xem Tướng này đã được mua và đặt hợp lệ trên bàn cờ chưa.
    /// </summary>
    public static bool IsUnitActiveOnBoard(GameObject go)
    {
        if (go == null || !go.activeInHierarchy) return false;
        DragAndDrop drag = go.GetComponent<DragAndDrop>();
        if (drag != null)
        {
            return drag.isBuy && drag.IsPlacedOnBoard;
        }
        return true;
    }

    private void Start()
    {
        previousPosition = transform.position;
        originalScale = transform.localScale;

        priceData = GetComponent<_Hero>();
        price = priceData != null ? priceData.price : 0;

        // Auto-attach HongCauCoinSpawner nếu là tướng Hồng Cầu
        if (gameObject.name.Contains("HongCau") || gameObject.name.Contains("Hồng Cầu"))
        {
            if (GetComponent<HongCauCoinSpawner>() == null)
            {
                gameObject.AddComponent<HongCauCoinSpawner>();
            }
        }
    }

    private void OnMouseDown()
    {
        if (!enabled) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        // 🥇 ƯU TIÊN #1: Nếu nhấp trúng Coin, thu thập Coin ngay lập tức và KHÔNG kéo tướng
        Collider2D hitCol = Physics2D.OverlapCircle(mouseWorld, 0.6f);
        if (hitCol != null)
        {
            HongCauDroppedCoinItem coinItem = hitCol.GetComponent<HongCauDroppedCoinItem>();
            if (coinItem != null)
            {
                coinItem.CollectCoin();
                return;
            }
        }

        // Tắt Animator khi bắt đầu kéo
        Animator anim = GetComponent<Animator>();
        if (anim) anim.enabled = false;

        // Tạm thời giải phóng ô Tile cũ nếu tướng đã ở trên bàn
        if (currentTile != null && GridManager.Instance != null)
        {
            GridManager.Instance.FreeTile(currentTile);
        }

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

        // 🌟 Hiệu ứng Hover Highlight làm sáng ô Tile đang ngắm tới
        if (GridManager.Instance != null)
        {
            GridManager.Instance.ClearAllHighlights();

            TileCell hoverTile = GridManager.Instance.GetTileAtWorldPosition(mouseWorld);
            if (hoverTile != null)
            {
                bool canPlace = GridManager.Instance.IsTileFree(hoverTile);
                if (!isBuy && GoldManager.Instance != null && !GoldManager.Instance.HasEnoughGold(price))
                {
                    canPlace = false;
                }

                // Xanh lá = Đặt được vào giữa ô, Đỏ = Không hợp lệ
                Color highlightColor = canPlace ? new Color(0.3f, 1f, 0.3f, 0.8f) : new Color(1f, 0.3f, 0.3f, 0.8f);
                hoverTile.SetHighlight(highlightColor);
            }
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;

        // Reset toàn bộ màu Highlight của các ô Tile
        if (GridManager.Instance != null)
        {
            GridManager.Instance.ClearAllHighlights();
        }

        // 🧩 1. Kiểm tra Vùng Bán / Thùng Rác
        if (DestroyUnitTrigger.isOverDestroyZone)
        {
            if (isBuy)
            {
                Debug.Log("🗑️ Đã bán tướng - Hoàn tiền: " + price);
                DestroyUnitTrigger.isOverDestroyZone = false;
                if (currentTile != null && GridManager.Instance != null)
                {
                    GridManager.Instance.FreeTile(currentTile);
                }
                TrashSellAnimation.Play(transform.position, price);
                Destroy(gameObject);
                return;
            }
            else
            {
                Debug.Log("❌ Chưa mua tướng, không thể bán!");
                RevertToPreviousPosition();
                return;
            }
        }

        // 🔍 2. Tìm ô TileCell mục tiêu ngay tại vị trí thả chuột
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        TileCell targetTile = null;
        if (GridManager.Instance != null)
        {
            targetTile = GridManager.Instance.GetTileAtWorldPosition(mouseWorld);
        }

        // 🚫 3. Kiểm tra tính hợp lệ của ô Tile
        bool isValidTile = targetTile != null && GridManager.Instance != null && GridManager.Instance.IsTileFree(targetTile);

        // 💰 4. Kiểm tra tiền nếu chưa mua
        if (isValidTile && !isBuy)
        {
            if (GoldManager.Instance != null && !GoldManager.Instance.HasEnoughGold(price))
            {
                Debug.Log($"❌ Không đủ tiền mua {gameObject.name}, cần {price}");
                isValidTile = false;
            }
        }

        // ✅ 5. ĐẶT THÀNH CÔNG VÀO CHÍNH GIỮA TÂM Ô TILE (Trực tiếp 100% không lệch)
        if (isValidTile)
        {
            Vector3 finalPos = targetTile.CenterPosition;
            finalPos.z = -1f; // 🥈 ƯU TIÊN #2: Tướng đứng ở Z = -1f (Phía trước Tile Z=0f, đằng sau Coin Z=-5f)

            // Gỡ tướng khỏi ô Shop slot container cũ để không bị giật lắc khi bấm Roll
            transform.SetParent(targetTile.transform);

            transform.position = finalPos;
            previousPosition = finalPos;

            // Cập nhật occupied state
            currentTile = targetTile;
            GridManager.Instance.OccupyTile(targetTile, gameObject);
            isPlacedOnBoard = true;

            // Trừ tiền nếu vừa mua từ shop
            if (!isBuy)
            {
                isBuy = true;
                if (GoldManager.Instance != null)
                {
                    GoldManager.Instance.SpendGold(price);
                    Debug.Log($"💰 Đã mua {gameObject.name} giá {price} vàng");
                }
            }

            PlayHeroSpawnSound();

            Animator anim = GetComponent<Animator>();
            if (anim) anim.enabled = true;
        }
        else
        {
            // ❌ Đặt không hợp lệ -> Trả về vị trí cũ
            RevertToPreviousPosition();
        }
    }

    /// <summary>
    /// Trả nhân vật về vị trí hợp lệ trước đó và chiếm giữ lại ô cũ.
    /// </summary>
    private void RevertToPreviousPosition()
    {
        transform.position = previousPosition;

        if (currentTile != null && GridManager.Instance != null)
        {
            GridManager.Instance.OccupyTile(currentTile, gameObject);
        }

        Animator anim = GetComponent<Animator>();
        if (anim) anim.enabled = true;
    }

    private void PlayHeroSpawnSound()
    {
        HeroAudio heroAudio = GetComponent<HeroAudio>();
        if (heroAudio != null)
        {
            heroAudio.PlaySpawnSound();
            return;
        }

        _Hero hero = GetComponent<_Hero>();
        if (hero != null && hero.HeroData != null)
        {
            HeroData heroData = hero.HeroData;
            if (heroData.spawnSound != null)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(heroData.spawnSound, heroData.spawnSoundVolume, 1f);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(heroData.spawnSound, transform.position, heroData.spawnSoundVolume);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
}
