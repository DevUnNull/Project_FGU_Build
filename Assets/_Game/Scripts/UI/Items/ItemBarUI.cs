using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Thanh vật phẩm lối sống trong trận (góc dưới trái, giữa thùng rác và shop). Do BattleHUD tạo.
/// Mỗi ô: biểu tượng + số lượng còn lại + thời gian hiệu ứng còn lại.
/// </summary>
public class ItemBarUI : MonoBehaviour
{
    private const float SlotSize = 104f;
    private const float SlotSpacing = 124f;
    private static readonly Color EmptyTint = new Color(1f, 1f, 1f, 0.35f);

    private class Slot
    {
        public LifestyleItem type;
        public RectTransform rect;
        public Button button;
        public Image icon;
        public TextMeshProUGUI countText;
        public TextMeshProUGUI timerText;
    }

    private readonly List<Slot> slots = new List<Slot>();
    private GameObject bar;

    /// <param name="area">Vị trí ô đầu tiên đặt sẵn trong Scene (có thể null -> dùng vị trí mặc định)</param>
    public void Build(Transform parent, RectTransform area)
    {
        BattleBuffs.StartBattle();

        RectTransform barRt = RuntimeUI.CreateRect(area != null ? area : parent, "ItemBar",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        Vector2 firstSlot = area != null ? Vector2.zero : BattleHUD.DefaultItemBarPosition;
        bar = barRt.gameObject;

        ItemCatalog catalog = ItemCatalog.Instance;
        IReadOnlyList<LifestyleItem> owned = BattleBuffs.OwnedTypes;
        for (int i = 0; i < owned.Count; i++)
        {
            ItemCatalog.Entry entry = catalog != null ? catalog.Get(owned[i]) : null;
            slots.Add(CreateSlot(barRt, owned[i], entry, firstSlot + new Vector2(i * SlotSpacing, 0f),
                catalog != null ? catalog.slotBackground : null));
        }

        if (slots.Count > 0)
        {
            BattleHUD.Toast("Lối sống tốt hôm nay cho bạn vật phẩm! Bấm biểu tượng ở góc dưới trái để dùng.", BattleHUD.TipColor, "item_bar_hint");
        }
    }

    private Slot CreateSlot(RectTransform parent, LifestyleItem type, ItemCatalog.Entry entry, Vector2 position, Sprite background)
    {
        RectTransform rt = RuntimeUI.CreateCentered(parent, "Item_" + type, new Vector2(SlotSize, SlotSize));
        rt.anchoredPosition = position;
        Image bg = RuntimeUI.AddImage(rt, background != null ? Color.white : new Color(1f, 0.97f, 0.88f, 0.95f));
        bg.sprite = background;
        bg.preserveAspect = true;

        Button button = rt.gameObject.AddComponent<Button>();
        button.targetGraphic = bg;
        button.onClick.AddListener(() => OnSlotClicked(type, rt));

        RectTransform iconRt = RuntimeUI.CreateCentered(rt, "Icon", new Vector2(SlotSize * 0.66f, SlotSize * 0.66f));
        Image icon = RuntimeUI.AddImage(iconRt, Color.white, false);
        if (entry != null) icon.sprite = entry.icon;
        icon.preserveAspect = true;

        RectTransform countRt = RuntimeUI.CreateRect(rt, "Count", new Vector2(0.5f, 0f), new Vector2(1.15f, 0.4f), Vector2.zero, Vector2.zero);
        TextMeshProUGUI countText = RuntimeUI.CreateText(countRt, "", 30f, RuntimeUI.TextDark, TextAlignmentOptions.BottomRight);
        countText.fontStyle = FontStyles.Bold;
        countText.outlineWidth = 0.25f;
        countText.outlineColor = Color.white;

        RectTransform timerRt = RuntimeUI.CreateRect(rt, "Timer", new Vector2(0f, 1f), new Vector2(1f, 1.45f), Vector2.zero, Vector2.zero);
        TextMeshProUGUI timerText = RuntimeUI.CreateText(timerRt, "", 28f, RuntimeUI.PositiveText);
        timerText.fontStyle = FontStyles.Bold;

        return new Slot { type = type, rect = rt, button = button, icon = icon, countText = countText, timerText = timerText };
    }

    private void OnSlotClicked(LifestyleItem type, RectTransform slotRect)
    {
        // Vật phẩm có thời hạn sẽ phí nếu dùng khi trận chưa chạy
        if (WaveManager.Instance != null && !WaveManager.Instance.IsWaveStarted)
        {
            BattleHUD.Toast("Bấm Bắt đầu trận rồi hãy dùng vật phẩm nhé!", BattleHUD.WarningColor);
            return;
        }

        if (BattleBuffs.Use(type)) StartCoroutine(Pulse(slotRect));
    }

    private static System.Collections.IEnumerator Pulse(RectTransform rt)
    {
        const float duration = 0.25f;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float k = Mathf.Sin(t / duration * Mathf.PI);
            rt.localScale = Vector3.one * (1f + 0.2f * k);
            yield return null;
        }
        rt.localScale = Vector3.one;
    }

    private void Update()
    {
        if (bar == null) return;

        // Ẩn khi game dừng (Pause, bảng Thắng/Thua) như các nút HUD khác
        bool running = Time.timeScale > 0f;
        if (bar.activeSelf != running) bar.SetActive(running);
        if (!running) return;

        foreach (Slot slot in slots)
        {
            int count = BattleBuffs.Count(slot.type);
            float timeLeft = BattleBuffs.ActiveTimeLeft(slot.type);
            slot.countText.text = "x" + count;
            slot.button.interactable = count > 0;
            slot.icon.color = count > 0 ? Color.white : EmptyTint;
            slot.timerText.text = timeLeft > 0f ? Mathf.CeilToInt(timeLeft) + "s" : "";
        }
    }
}
