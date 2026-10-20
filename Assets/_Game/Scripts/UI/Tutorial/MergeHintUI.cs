using TMPro;
using UnityEngine;

/// <summary>
/// Gợi ý ngay trên bàn cờ (do BattleHUD tạo):
/// - Ghép: khi có 2 tế bào cùng loại, cùng cấp (1 con trên bàn + 1 con trong khay hoặc trên bàn), hiện bong bóng
///   "Kéo con cùng loại vào đây để nâng cấp!" trên con trên bàn và nháy vàng cả 2 con. Tắt hẳn sau lần ghép đầu tiên.
/// - Roll: đứng yên lâu mà không có cặp nào ghép được -> nút Roll nhún nhảy + 1 lời nhắc mỗi trận.
/// </summary>
public class MergeHintUI : MonoBehaviour
{
    private const string LearnedKey = "CallOutMyCells_MergeLearned";
    private const float ScanInterval = 0.4f;
    private const float FlashInterval = 1.3f;
    private const float RollIdleSeconds = 12f;
    private const int RollCost = 2;
    private static readonly Color HintFlash = new Color(1f, 1f, 0.45f, 1f);

    public static bool Learned => PlayerPrefs.GetInt(LearnedKey, 0) == 1;

    public static void ResetLearned()
    {
        PlayerPrefs.DeleteKey(LearnedKey);
        PlayerPrefs.Save();
    }

    public static void MarkLearned()
    {
        if (Learned) return;
        PlayerPrefs.SetInt(LearnedKey, 1);
        PlayerPrefs.Save();
    }

    private RectTransform canvasRect;
    private RectTransform bubble;
    private CanvasGroup bubbleGroup;

    private HeroBase hintTarget;
    private HeroBase hintSource;
    private float scanTimer;
    private float flashTimer;

    private RectTransform rollRect;
    private Vector3 rollBaseScale;
    private bool rollPulsing;
    private bool rollToastShown;
    private float battleStartTime;

    public void Build(Canvas canvas)
    {
        canvasRect = (RectTransform)canvas.transform;
        bubble = RuntimeUI.CreateWarningBanner(canvas.transform, "MergeHint", new Vector2(560f, 96f), out RectTransform content);
        TextMeshProUGUI text = RuntimeUI.CreateText(content, "Kéo con cùng loại vào đây để nâng cấp!", 24f, BattleHUD.TipColor);
        text.enableAutoSizing = true;
        text.fontSizeMin = 14f;
        text.fontSizeMax = 24f;
        bubbleGroup = bubble.gameObject.AddComponent<CanvasGroup>();
        bubbleGroup.alpha = 0f;
        bubbleGroup.blocksRaycasts = false;
        bubbleGroup.interactable = false;

        battleStartTime = Time.unscaledTime;
    }

    private void Update()
    {
        bool running = Time.timeScale > 0f;

        scanTimer -= Time.unscaledDeltaTime;
        if (scanTimer <= 0f)
        {
            scanTimer = ScanInterval;
            FindPair(out hintSource, out hintTarget);
        }

        UpdateMergeBubble(running);
        UpdateRollHint(running);
    }

    // ===================== Gợi ý ghép =====================

    private void UpdateMergeBubble(bool running)
    {
        bool show = running && !Learned && hintTarget != null && DragAndDrop.Dragging == null;
        bubbleGroup.alpha = Mathf.MoveTowards(bubbleGroup.alpha, show ? 1f : 0f, Time.unscaledDeltaTime * 5f);
        if (!show) return;

        // Bong bóng nhún nhẹ ngay trên đầu tế bào đích
        Vector3 top = hintTarget.transform.position + Vector3.up * 1.0f;
        Camera cam = Camera.main;
        if (cam != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, cam.WorldToScreenPoint(top), null, out Vector2 local))
        {
            local.y += 60f + Mathf.Sin(Time.unscaledTime * 4f) * 8f;
            local.x = Mathf.Clamp(local.x, -canvasRect.rect.width * 0.5f + 300f, canvasRect.rect.width * 0.5f - 300f);
            bubble.anchoredPosition = local;
        }

        flashTimer -= Time.unscaledDeltaTime;
        if (flashTimer <= 0f)
        {
            flashTimer = FlashInterval;
            UnitJuice.GetOrAdd(hintTarget.gameObject).Flash(HintFlash, 0.5f);
            if (hintSource != null) UnitJuice.GetOrAdd(hintSource.gameObject).Flash(HintFlash, 0.5f);
        }
    }

    /// <summary>
    /// Tìm cặp ghép được: target đang đứng trên bàn, source là con cùng loại cùng cấp (ưu tiên con mua được trong khay).
    /// </summary>
    private static void FindPair(out HeroBase source, out HeroBase target)
    {
        source = null;
        target = null;
        int gold = GoldManager.Instance != null ? GoldManager.Instance.gold : int.MaxValue;

        foreach (HeroBase board in HeroBase.Active)
        {
            if (board.HeroData == null || board.Level >= HeroBase.MaxLevel || !DragAndDrop.IsUnitActiveOnBoard(board.gameObject)) continue;

            foreach (HeroBase other in HeroBase.Active)
            {
                if (other == board || other.HeroData != board.HeroData || other.Level != board.Level) continue;

                DragAndDrop drag = other.GetComponent<DragAndDrop>();
                if (drag == null) continue;
                bool onBoard = drag.isBuy && drag.IsPlacedOnBoard;
                bool affordableInShop = !drag.isBuy && board.HeroData.price <= gold;
                if (!onBoard && !affordableInShop) continue;

                source = other;
                target = board;
                if (affordableInShop) return; // cặp "khay -> bàn" dễ hiểu nhất cho người mới
            }
        }
    }

    // ===================== Gợi ý Roll =====================

    private void UpdateRollHint(bool running)
    {
        if (rollRect == null)
        {
            GameObject roll = GameObject.Find("Roll");
            if (roll == null) return;
            rollRect = roll.transform as RectTransform;
            if (rollRect == null) return;
            rollBaseScale = rollRect.localScale;
        }

        bool shouldPulse = running && ShouldSuggestRoll();
        if (shouldPulse)
        {
            float k = 1f + 0.1f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 4f));
            rollRect.localScale = rollBaseScale * k;
            if (!rollToastShown)
            {
                rollToastShown = true;
                BattleHUD.Toast($"Không có tế bào cùng loại để ghép? Bấm Roll ({RollCost} ATP) để đổi tế bào mới trong khay!", BattleHUD.TipColor);
            }
        }
        else if (rollPulsing)
        {
            rollRect.localScale = rollBaseScale;
        }
        rollPulsing = shouldPulse;
    }

    private bool ShouldSuggestRoll()
    {
        if (WaveManager.Instance == null || !WaveManager.Instance.IsWaveStarted) return false;
        if (hintTarget != null || DragAndDrop.Dragging != null) return false;
        if (GoldManager.Instance == null || GoldManager.Instance.gold < RollCost) return false;

        bool hasBoardUnit = false;
        foreach (HeroBase hero in HeroBase.Active)
        {
            if (DragAndDrop.IsUnitActiveOnBoard(hero.gameObject)) { hasBoardUnit = true; break; }
        }
        if (!hasBoardUnit) return false;

        float lastAction = Mathf.Max(battleStartTime, Mathf.Max(DragAndDrop.LastActionTime, ShopSpawner.LastRollTime));
        return Time.unscaledTime - lastAction > RollIdleSeconds;
    }

    private void OnDestroy()
    {
        if (rollRect != null && rollPulsing) rollRect.localScale = rollBaseScale;
    }
}
