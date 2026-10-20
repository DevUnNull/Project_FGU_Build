using UnityEngine;

/// <summary>
/// Cơ chế Kéo & Đặt Tế Bào (Cell) chuẩn PvZ:
/// - Đặt Tế bào CHÍNH XÁC VÀO TÂM CHÍNH GIỮA ô Tile (transform.position = tile.CenterPosition).
/// - Hiệu ứng Hover Highlight thực thời (Xanh lá = Đặt được, Đỏ = Ô bận/không hợp lệ).
/// - Quản lý chiếm giữ ô (Occupied State), ngăn không cho đặt chồng nhiều tướng lên 1 ô.
/// - Hoàn tác vị trí cũ an toàn nếu kéo thả sai hoặc không đủ tiền.
/// - Ghép: thả lên tế bào CÙNG LOẠI, CÙNG CẤP đang đứng trên bàn -> con bên dưới lên 1 cấp (ô sáng vàng).
/// </summary>
public class DragAndDrop : MonoBehaviour
{
    private Vector3 offset;               // Khoảng cách giữa con trỏ và nhân vật
    private bool isDragging = false;      // Đang kéo hay không

    // Phân biệt "nhấp" và "kéo": chuột phải di chuyển quá ngưỡng (pixel) mới tính là kéo thật
    private const float DragThresholdPixels = 15f;
    private Vector3 mouseDownScreenPos;
    private bool hasMovedBeyondThreshold = false;

    // Cảm ứng: tướng nổi lên trên đầu ngón tay khi kéo để không bị ngón tay che (ô đích vẫn là ô dưới ngón tay)
    private const float TouchLiftWorld = 1.0f;
    private bool isTouchDrag = false;

    // Ngưỡng kéo theo mật độ điểm ảnh: màn hình điện thoại nét cao cần ngưỡng pixel lớn hơn
    private static float DragThreshold => Screen.dpi > 0f ? Mathf.Max(DragThresholdPixels, Screen.dpi * 0.08f) : DragThresholdPixels;
    private Vector3 previousPosition;     // Vị trí cũ (để hoàn tác khi thả sai)

    // 💰 Mua & Đặt
    private bool isPlacedOnBoard = false; // Đã đặt trên bàn cờ chưa
    public bool isBuy = false;            // Đã thanh toán mua chưa
    public bool IsPlacedOnBoard => isPlacedOnBoard;

    // Tế bào đang được kéo (gợi ý ghép ẩn đi khi người chơi đang kéo)
    public static DragAndDrop Dragging { get; private set; }
    // Lần cuối người chơi mua / ghép / bán (gợi ý bấm Roll khi đứng yên lâu)
    public static float LastActionTime { get; private set; }

    private static readonly Color PlaceColor = new Color(0.3f, 1f, 0.3f, 0.8f);
    private static readonly Color InvalidColor = new Color(1f, 0.3f, 0.3f, 0.8f);
    private static readonly Color MergeColor = new Color(1f, 0.85f, 0.1f, 0.9f);

    // Các tế bào trên bàn ghép được với con đang kéo (đang sáng viền) + con đang được ngắm tới
    private readonly System.Collections.Generic.List<HeroBase> mergeCandidates = new System.Collections.Generic.List<HeroBase>();
    private HeroBase hoveredMergeTarget;

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

        // Auto-attach HongCauCoinSpawner nếu tướng sinh ATP (HeroData.producesATP)
        HeroBase heroBase = GetComponent<HeroBase>();
        if (heroBase != null && heroBase.HeroData != null && heroBase.HeroData.producesATP)
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

        // Ghi lại chỗ đứng hiện tại (khung shop hoặc ô đang đứng) để quay về nếu thả sai
        previousPosition = transform.position;

        mouseDownScreenPos = Input.mousePosition;
        hasMovedBeyondThreshold = false;
        isTouchDrag = Input.touchCount > 0;
        isDragging = true;
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;

        // Chưa kéo đủ xa -> vẫn coi là đang nhấp, tướng đứng yên
        if (!hasMovedBeyondThreshold)
        {
            if (Vector2.Distance(Input.mousePosition, mouseDownScreenPos) < DragThreshold) return;
            hasMovedBeyondThreshold = true;
            Dragging = this;
            ShowMergeCandidates();
        }

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 newPosition = isTouchDrag ? mouseWorld + Vector3.up * TouchLiftWorld : mouseWorld + offset;
        newPosition.z = 0;
        transform.position = newPosition;

        // 🌟 Hiệu ứng Hover Highlight làm sáng ô Tile đang ngắm tới
        if (GridManager.Instance != null)
        {
            GridManager.Instance.ClearAllHighlights();

            TileCell hoverTile = GridManager.Instance.GetTileAtWorldPosition(GetPlacementPoint(mouseWorld));
            SetHoveredMergeTarget(hoverTile != null ? GetMergeTarget(hoverTile) : null);
            if (hoverTile != null)
            {
                bool canPay = isBuy || GoldManager.Instance == null || GoldManager.Instance.HasEnoughGold(price);

                // Vàng = ghép được vào tế bào cùng loại, Xanh lá = đặt được vào ô trống, Đỏ = không hợp lệ
                Color highlightColor;
                if (GetMergeTarget(hoverTile) != null) highlightColor = canPay ? MergeColor : InvalidColor;
                else highlightColor = canPay && GridManager.Instance.IsTileFree(hoverTile) ? PlaceColor : InvalidColor;
                hoverTile.SetHighlight(highlightColor);
            }
        }
    }

    private void OnMouseUp()
    {
        // OnMouseDown bị bỏ qua (VD: nhấp nhặt Coin) -> không xử lý thả
        if (!isDragging) return;
        isDragging = false;
        if (Dragging == this) Dragging = null;
        ClearMergePreview();

        // Reset toàn bộ màu Highlight của các ô Tile
        if (GridManager.Instance != null)
        {
            GridManager.Instance.ClearAllHighlights();
        }

        // Chỉ nhấp (chưa kéo) -> giữ nguyên chỗ cũ, tướng trong shop CHƯA bị mua
        if (!hasMovedBeyondThreshold)
        {
            RevertToPreviousPosition();
            return;
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
                // Tế bào đã ghép hoàn lại tiền của mọi con đã gộp vào
                HeroBase soldHero = GetComponent<HeroBase>();
                int refund = price * (soldHero != null ? soldHero.UnitCount : 1);
                TrashSellAnimation.Play(transform.position, refund);
                LastActionTime = Time.unscaledTime;
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
            targetTile = GridManager.Instance.GetTileAtWorldPosition(GetPlacementPoint(mouseWorld));
        }

        // 🔀 2b. Thả lên tế bào cùng loại, cùng cấp -> ghép lên cấp
        if (targetTile != null)
        {
            HeroBase mergeTarget = GetMergeTarget(targetTile);
            if (mergeTarget != null)
            {
                if (!TryMergeInto(mergeTarget)) RevertToPreviousPosition();
                return;
            }
            ExplainMergeFailure(targetTile);
        }

        // 🚫 3. Kiểm tra tính hợp lệ của ô Tile
        bool isValidTile = targetTile != null && GridManager.Instance != null && GridManager.Instance.IsTileFree(targetTile);

        // 💰 4. Kiểm tra tiền nếu chưa mua
        if (isValidTile && !isBuy)
        {
            if (GoldManager.Instance != null && !GoldManager.Instance.HasEnoughGold(price))
            {
                Debug.Log($"❌ Không đủ tiền mua {gameObject.name}, cần {price}");
                BattleHUD.Toast($"Không đủ ATP! Cần {price} ATP. Nhặt xu từ Hồng Cầu hoặc tiêu diệt mầm bệnh để có thêm.", BattleHUD.WarningColor);
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

            HeroBase placedHero = GetComponent<HeroBase>();
            if (placedHero != null && placedHero.HeroData != null) NotebookProgress.UnlockHero(placedHero.HeroData.name);
            LastActionTime = Time.unscaledTime;

            // Hiệu ứng "đáp xuống": bè ra & lùn xuống rồi nảy về dáng gốc
            UnitJuice.GetOrAdd(gameObject).Punch(new Vector2(0.22f, -0.22f), 0.35f);

            Animator anim = GetComponent<Animator>();
            if (anim) anim.enabled = true;
        }
        else
        {
            // Thả vào ô Độc tố (ô bị chiếm nhưng không phải tế bào) -> giải thích hậu quả lối sống
            if (targetTile != null && targetTile.IsOccupied && targetTile.OccupiedUnit.GetComponent<HeroBase>() == null)
            {
                BattleHUD.Toast("Ô này bị Độc tố chiếm! Đây là hậu quả của chất kích thích, rượu bia, đồ chiên rán hôm nay.", BattleHUD.WarningColor);
            }

            // ❌ Đặt không hợp lệ (thả ra ngoài bàn cờ, ô bận, thiếu ATP) -> Trả về vị trí cũ (khung shop nếu chưa mua)
            RevertToPreviousPosition();
        }
    }

    /// <summary>
    /// Tế bào trên ô có ghép được với tế bào đang kéo không: cùng HeroData, cùng cấp, chưa tối đa.
    /// </summary>
    private HeroBase GetMergeTarget(TileCell tile)
    {
        if (tile == null || !tile.IsOccupied || tile.OccupiedUnit == gameObject) return null;
        HeroBase target = tile.OccupiedUnit.GetComponent<HeroBase>();
        return CanMergeWith(target) ? target : null;
    }

    private bool CanMergeWith(HeroBase target)
    {
        HeroBase self = GetComponent<HeroBase>();
        if (self == null || target == null || target == self || self.HeroData == null || target.HeroData != self.HeroData) return false;
        return target.Level == self.Level && target.Level < HeroBase.MaxLevel;
    }

    // ===== Hiệu ứng báo "ghép được" trong lúc kéo (chưa thả) =====

    /// <summary>Vừa bắt đầu kéo: làm sáng mọi tế bào trên bàn ghép được với con này.</summary>
    private void ShowMergeCandidates()
    {
        ClearMergePreview();
        HeroBase self = GetComponent<HeroBase>();
        if (self == null) return;

        foreach (HeroBase hero in HeroBase.Active)
        {
            if (!IsUnitActiveOnBoard(hero.gameObject) || !CanMergeWith(hero)) continue;
            mergeCandidates.Add(hero);
            MergePreview.GetOrAdd(hero.gameObject).SetMode(MergePreview.Mode.Candidate);
        }
    }

    /// <summary>Đang ngắm vào con ghép được -> con đó và con đang kéo sáng mạnh + "Thả để lên cấp N!".</summary>
    private void SetHoveredMergeTarget(HeroBase target)
    {
        if (target == hoveredMergeTarget) return;

        if (hoveredMergeTarget != null)
        {
            MergePreview.GetOrAdd(hoveredMergeTarget.gameObject).SetMode(
                mergeCandidates.Contains(hoveredMergeTarget) ? MergePreview.Mode.Candidate : MergePreview.Mode.None);
        }

        hoveredMergeTarget = target;
        MergePreview self = MergePreview.GetOrAdd(gameObject);
        if (target != null)
        {
            MergePreview.GetOrAdd(target.gameObject).SetMode(MergePreview.Mode.Hover, target.Level + 1);
            self.SetMode(MergePreview.Mode.Dragged);
        }
        else
        {
            self.SetMode(MergePreview.Mode.None);
        }
    }

    private void ClearMergePreview()
    {
        foreach (HeroBase hero in mergeCandidates)
        {
            if (hero != null) MergePreview.GetOrAdd(hero.gameObject).SetMode(MergePreview.Mode.None);
        }
        mergeCandidates.Clear();
        if (hoveredMergeTarget != null) MergePreview.GetOrAdd(hoveredMergeTarget.gameObject).SetMode(MergePreview.Mode.None);
        hoveredMergeTarget = null;
        MergePreview own = GetComponent<MergePreview>();
        if (own != null) own.SetMode(MergePreview.Mode.None);
    }

    /// <summary>Ghép tế bào đang kéo vào target. Tế bào từ shop thì trả tiền như khi mua.</summary>
    private bool TryMergeInto(HeroBase target)
    {
        if (!isBuy)
        {
            if (GoldManager.Instance != null && !GoldManager.Instance.HasEnoughGold(price))
            {
                BattleHUD.Toast($"Không đủ ATP! Cần {price} ATP để mua và ghép.", BattleHUD.WarningColor);
                return false;
            }
            isBuy = true;
            if (GoldManager.Instance != null) GoldManager.Instance.SpendGold(price);
        }

        int newLevel = target.Level + 1;
        target.SetLevel(newLevel);

        UnitJuice juice = UnitJuice.GetOrAdd(target.gameObject);
        juice.Punch(new Vector2(0.3f, 0.3f), 0.45f);
        juice.Flash(new Color(1f, 1f, 0.45f, 1f), 0.35f);
        GameSfx.Play(GameSfx.Kind.Merge);

        string name = target.HeroData != null && NotebookCatalog.Instance != null && NotebookCatalog.Instance.FindHero(target.HeroData.name) != null
            ? NotebookCatalog.Instance.FindHero(target.HeroData.name).displayName : "Tế bào";
        float mult = HeroBase.LevelMultiplier(newLevel);
        string effect = target.HeroData != null && target.HeroData.producesATP ? "rơi xu ATP nhanh hơn" : $"máu và sát thương x{mult:0.#}";
        BattleHUD.Toast(MergeHintUI.Learned
                ? $"{name} lên cấp {newLevel}!"
                : $"Ghép thành công! {name} lên cấp {newLevel}: {effect}. Ghép tiếp 2 con cấp {newLevel} để lên cấp {newLevel + 1}.",
            BattleHUD.TipColor);
        MergeHintUI.MarkLearned();

        LastActionTime = Time.unscaledTime;
        Destroy(gameObject);
        return true;
    }

    /// <summary>Thả lên tế bào cùng loại nhưng không ghép được -> giải thích lý do.</summary>
    private void ExplainMergeFailure(TileCell tile)
    {
        if (!tile.IsOccupied || tile.OccupiedUnit == gameObject) return;
        HeroBase self = GetComponent<HeroBase>();
        HeroBase other = tile.OccupiedUnit.GetComponent<HeroBase>();
        if (self == null || other == null || self.HeroData == null || other.HeroData != self.HeroData) return;

        if (other.Level >= HeroBase.MaxLevel)
            BattleHUD.Toast("Tế bào này đã đạt cấp tối đa (cấp 3).", BattleHUD.WarningColor);
        else if (other.Level != self.Level)
            BattleHUD.Toast($"Chỉ ghép được 2 tế bào CÙNG CẤP (đây là cấp {self.Level} và cấp {other.Level}).", BattleHUD.WarningColor);
    }

    private void OnDestroy()
    {
        if (Dragging == this) Dragging = null;
        ClearMergePreview();
    }

    /// <summary>
    /// Điểm dùng để chọn ô: chuột -> ngay con trỏ; cảm ứng -> chỗ tế bào đang được nhấc lên trên ngón tay,
    /// để người chơi canh hình tế bào vào ô nào thì tế bào rơi đúng ô đó.
    /// </summary>
    private Vector3 GetPlacementPoint(Vector3 pointerWorld)
    {
        Vector3 point = isTouchDrag ? pointerWorld + Vector3.up * TouchLiftWorld : pointerWorld;
        point.z = 0f;
        return point;
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
