using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bảng "Sổ tay sức khỏe" (mở từ MainMap): 3 trang Kiến thức / Tế bào / Mầm bệnh.
/// Mục chưa mở hiện "???" kèm gợi ý cách mở, để người chơi có lý do chơi lại với lựa chọn khác.
/// </summary>
public class NotebookUI : MonoBehaviour
{
    private enum Tab { Tips, Heroes, Enemies }

    private static readonly Color RowColor = new Color(1f, 0.97f, 0.88f, 0.9f);
    private static readonly Color LockedRowColor = new Color(0.86f, 0.84f, 0.8f, 0.9f);
    private static readonly Color TabIdleTint = new Color(0.7f, 0.7f, 0.7f, 1f);
    private const float RowSpacing = 14f;
    private const float IconSize = 120f;

    private RectTransform viewport;
    private RectTransform content;
    private TextMeshProUGUI progressText;
    private readonly List<Button> tabButtons = new List<Button>();

    public static void Open()
    {
        Canvas canvas = RuntimeUI.CreateOverlayCanvas("Notebook", 450);
        canvas.gameObject.AddComponent<NotebookUI>().Build(canvas);
    }

    private void Build(Canvas canvas)
    {
        CanvasGroup group = canvas.gameObject.AddComponent<CanvasGroup>();
        RectTransform root = RuntimeUI.Stretch(canvas.transform, "Backdrop");
        RuntimeUI.AddImage(root, RuntimeUI.Backdrop);

        RectTransform card = RuntimeUI.CreateCard(root, new Vector2(1500f, 900f), "SỔ TAY SỨC KHỎE", RuntimeUI.HeaderGreen, out RectTransform body);

        // Hàng trên: 3 nút trang + tiến độ
        string[] labels = { "Kiến thức", "Tế bào", "Mầm bệnh" };
        for (int i = 0; i < labels.Length; i++)
        {
            Tab tab = (Tab)i;
            Button btn = RuntimeUI.CreatePillButton(body, labels[i], new Vector2(250f, 76f), () => ShowTab(tab));
            RectTransform rt = (RectTransform)btn.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(135f + i * 265f, -40f);
            tabButtons.Add(btn);
        }

        RectTransform progressArea = RuntimeUI.CreateRect(body, "Progress", new Vector2(0.55f, 1f), Vector2.one, new Vector2(0f, -80f), Vector2.zero);
        progressText = RuntimeUI.CreateText(progressArea, "", 30f, RuntimeUI.TextDark, TextAlignmentOptions.MidlineRight);

        // Danh sách cuộn
        viewport = RuntimeUI.CreateRect(body, "Viewport", Vector2.zero, Vector2.one, new Vector2(0f, 100f), new Vector2(0f, -95f));
        RuntimeUI.AddImage(viewport, new Color(0f, 0f, 0f, 0f)); // nhận thao tác kéo
        viewport.gameObject.AddComponent<RectMask2D>();

        content = RuntimeUI.CreateRect(viewport, "Content", new Vector2(0f, 1f), Vector2.one, Vector2.zero, Vector2.zero);
        content.pivot = new Vector2(0.5f, 1f);
        VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = RowSpacing;
        layout.padding = new RectOffset(10, 10, 6, 6);
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 40f;

        Button close = RuntimeUI.CreateButton(body, "Đóng", RuntimeUI.HeaderGreen, new Vector2(240f, 76f), () => Destroy(canvas.gameObject));
        RectTransform closeRt = (RectTransform)close.transform;
        closeRt.anchorMin = closeRt.anchorMax = new Vector2(0.5f, 0f);
        closeRt.anchoredPosition = new Vector2(0f, 40f);

        Canvas.ForceUpdateCanvases(); // cần bề rộng viewport để tính chiều cao từng dòng
        ShowTab(Tab.Tips);
        StartCoroutine(RuntimeUI.PopIn(group, card));
    }

    private void ShowTab(Tab tab)
    {
        for (int i = 0; i < tabButtons.Count; i++)
        {
            tabButtons[i].targetGraphic.color = i == (int)tab ? Color.white : TabIdleTint;
        }

        // Ẩn ngay (Destroy chỉ xóa cuối frame) để layout không tính các dòng cũ
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            GameObject old = content.GetChild(i).gameObject;
            old.SetActive(false);
            Destroy(old);
        }

        int total = 0, opened = 0;
        if (tab == Tab.Tips)
        {
            List<string> tips = NotebookProgress.GetAllTips();
            foreach (string tip in tips)
            {
                total++;
                bool isOpen = NotebookProgress.IsUnlocked(NotebookProgress.TipId(tip));
                if (isOpen) opened++;
                AddRow(null, isOpen ? $"Thẻ {total}" : $"Thẻ {total}: ???",
                    isOpen ? tip : "Chưa mở. Hãy thử những lựa chọn khác ở phần sinh hoạt trong ngày.", isOpen);
            }
        }
        else
        {
            NotebookCatalog catalog = NotebookCatalog.Instance;
            List<NotebookCatalog.UnitEntry> entries = catalog == null ? new List<NotebookCatalog.UnitEntry>()
                : tab == Tab.Heroes ? catalog.heroes : catalog.enemies;
            foreach (var entry in entries)
            {
                total++;
                string id = tab == Tab.Heroes ? NotebookProgress.HeroId(entry.assetName) : NotebookProgress.EnemyId(entry.assetName);
                bool isOpen = NotebookProgress.IsUnlocked(id);
                if (isOpen) opened++;
                string hint = tab == Tab.Heroes ? "Chưa mở. Đặt tế bào này lên bàn cờ để tìm hiểu." : "Chưa mở. Gặp mầm bệnh này trong trận để tìm hiểu.";
                AddRow(entry.icon, isOpen ? entry.displayName : "???", isOpen ? entry.description : hint, isOpen);
            }
        }

        progressText.text = $"Đã mở {opened}/{total}";
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        content.anchoredPosition = Vector2.zero;
    }

    private void AddRow(Sprite icon, string title, string text, bool isOpen)
    {
        float width = Mathf.Max(400f, viewport.rect.width - 20f);
        bool hasIcon = icon != null;
        float textLeft = hasIcon ? IconSize + 40f : 24f;
        float textWidth = width - textLeft - 24f;

        RectTransform row = RuntimeUI.CreateRect(content, "Row", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        RuntimeUI.AddImage(row, isOpen ? RowColor : LockedRowColor, false);

        RectTransform titleArea = RuntimeUI.CreateRect(row, "Title", new Vector2(0f, 1f), Vector2.one, new Vector2(textLeft, -52f), new Vector2(-24f, -10f));
        TextMeshProUGUI titleText = RuntimeUI.CreateText(titleArea, title, 32f, isOpen ? RuntimeUI.HeaderGreen : RuntimeUI.TextDark, TextAlignmentOptions.MidlineLeft);
        titleText.fontStyle = FontStyles.Bold;

        RectTransform bodyArea = RuntimeUI.CreateRect(row, "Body", Vector2.zero, Vector2.one, new Vector2(textLeft, 14f), new Vector2(-24f, -56f));
        TextMeshProUGUI bodyText = RuntimeUI.CreateText(bodyArea, text, 26f, RuntimeUI.TextDark, TextAlignmentOptions.TopLeft);

        float textHeight = bodyText.GetPreferredValues(text, textWidth, 0f).y;
        float height = Mathf.Max(hasIcon ? IconSize + 30f : 0f, 56f + textHeight + 20f);
        row.gameObject.AddComponent<LayoutElement>().preferredHeight = height;

        if (hasIcon)
        {
            RectTransform iconRt = RuntimeUI.CreateRect(row, "Icon", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
            iconRt.sizeDelta = new Vector2(IconSize, IconSize);
            iconRt.anchoredPosition = new Vector2(20f + IconSize * 0.5f, 0f);
            Image img = RuntimeUI.AddImage(iconRt, isOpen ? Color.white : new Color(0f, 0f, 0f, 0.55f), false);
            img.sprite = icon;
            img.preserveAspect = true;
        }
    }
}
