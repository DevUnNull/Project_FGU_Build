using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bảng "NHẬT KÝ CÁC NGÀY" (MainMenuScene → BT_XemNgay): tiến trình từng ngày
/// (đã thắng / đang mở / chưa mở), sao tốt nhất, số thẻ kiến thức đã mở, và nút chơi thẳng ngày đó.
/// </summary>
public class DayJournalUI : MonoBehaviour
{
    private const string FirstScene = "Scene1_LifeSimulation";
    private const float RowHeight = 150f;
    private const float StarSize = 44f;
    private static readonly Color RowColor = new Color(1f, 0.97f, 0.88f, 0.9f);
    private static readonly Color LockedRowColor = new Color(0.86f, 0.84f, 0.8f, 0.9f);

    private RectTransform content;

    public static void Open()
    {
        Canvas canvas = RuntimeUI.CreateOverlayCanvas("DayJournal", 450);
        canvas.gameObject.AddComponent<DayJournalUI>().Build(canvas);
    }

    private void Build(Canvas canvas)
    {
        CanvasGroup group = canvas.gameObject.AddComponent<CanvasGroup>();
        RectTransform root = RuntimeUI.Stretch(canvas.transform, "Backdrop");
        RuntimeUI.AddImage(root, RuntimeUI.Backdrop);

        RectTransform card = RuntimeUI.CreateCard(root, new Vector2(1300f, 880f), "NHẬT KÝ CÁC NGÀY", RuntimeUI.HeaderOrange, out RectTransform body);

        int dayCount = GameProgress.DayCount;
        int tipsOpened = 0, tipsTotal = 0;
        for (int day = 1; day <= dayCount; day++)
        {
            var (opened, total) = NotebookProgress.GetDayTipProgress(GameProgress.GetDayConfig(day));
            tipsOpened += opened;
            tipsTotal += total;
        }

        RectTransform header = RuntimeUI.CreateRect(body, "Summary", new Vector2(0f, 1f), Vector2.one, new Vector2(10f, -70f), new Vector2(-10f, 0f));
        RuntimeUI.CreateText(header, $"Tổng sao: {GameProgress.TotalStars}/{dayCount * 3}   ·   Thẻ kiến thức đã mở: {tipsOpened}/{tipsTotal}",
            30f, RuntimeUI.TextDark, TextAlignmentOptions.MidlineLeft);

        // Danh sách cuộn (thêm ngày mới không cần sửa giao diện)
        RectTransform viewport = RuntimeUI.CreateRect(body, "Viewport", Vector2.zero, Vector2.one, new Vector2(0f, 100f), new Vector2(0f, -80f));
        RuntimeUI.AddImage(viewport, new Color(0f, 0f, 0f, 0f));
        viewport.gameObject.AddComponent<RectMask2D>();
        content = RuntimeUI.CreateRect(viewport, "Content", new Vector2(0f, 1f), Vector2.one, Vector2.zero, Vector2.zero);
        content.pivot = new Vector2(0.5f, 1f);
        VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 12f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 40f;

        for (int day = 1; day <= dayCount; day++) AddDayRow(day, canvas);
        AddComingSoonRow(dayCount + 1);

        Button close = RuntimeUI.CreateButton(body, "Đóng", RuntimeUI.HeaderGreen, new Vector2(240f, 76f), () => Destroy(canvas.gameObject));
        RectTransform closeRt = (RectTransform)close.transform;
        closeRt.anchorMin = closeRt.anchorMax = new Vector2(0.5f, 0f);
        closeRt.anchoredPosition = new Vector2(0f, 40f);

        StartCoroutine(RuntimeUI.PopIn(group, card));
    }

    private void AddDayRow(int day, Canvas canvas)
    {
        DayConfig config = GameProgress.GetDayConfig(day);
        bool unlocked = GameProgress.IsUnlocked(day);
        int stars = GameProgress.GetBestStars(day);
        var (opened, total) = NotebookProgress.GetDayTipProgress(config);

        RectTransform row = CreateRow(unlocked);
        string title = config != null && !string.IsNullOrEmpty(config.displayName) ? config.displayName : $"Ngày {day}";
        AddText(row, "Title", title, 34f, unlocked ? RuntimeUI.HeaderOrange : RuntimeUI.TextDark, new Vector2(24f, -60f), new Vector2(-330f, -12f), true);

        string status = !unlocked ? $"Chưa mở khóa – hãy thắng Ngày {day - 1} trước."
            : stars > 0 ? "Đã chiến thắng" + (stars < 3 ? " – chơi lại để lấy đủ 3 sao!" : " – xuất sắc!")
            : "Đang chờ bạn khám phá.";
        AddText(row, "Status", status, 26f, RuntimeUI.TextDark, new Vector2(24f, -100f), new Vector2(-330f, -60f), false);
        AddText(row, "Tips", $"Thẻ kiến thức: {opened}/{total}", 24f, RuntimeUI.HeaderGreen, new Vector2(24f, -140f), new Vector2(-330f, -100f), false);

        AddStars(row, stars);

        if (unlocked)
        {
            Button play = RuntimeUI.CreateButton(row, stars > 0 ? "Chơi lại" : "Chơi", RuntimeUI.HeaderGreen, new Vector2(220f, 76f), () =>
            {
                GameProgress.CurrentDay = day;
                Destroy(canvas.gameObject);
                SceneVideoTransition.LoadScene(FirstScene);
            });
            RectTransform playRt = (RectTransform)play.transform;
            playRt.anchorMin = playRt.anchorMax = new Vector2(1f, 0.5f);
            playRt.anchoredPosition = new Vector2(-135f, -18f);
        }
    }

    private void AddComingSoonRow(int day)
    {
        RectTransform row = CreateRow(false);
        AddText(row, "Title", $"Ngày {day}", 34f, RuntimeUI.TextDark, new Vector2(24f, -60f), new Vector2(-24f, -12f), true);
        AddText(row, "Status", "Đang được phát triển, hẹn bạn sớm nhé!", 26f, RuntimeUI.TextDark, new Vector2(24f, -100f), new Vector2(-24f, -60f), false);
    }

    private RectTransform CreateRow(bool unlocked)
    {
        RectTransform row = RuntimeUI.CreateRect(content, "Day", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        RuntimeUI.AddImage(row, unlocked ? RowColor : LockedRowColor, false);
        row.gameObject.AddComponent<LayoutElement>().preferredHeight = RowHeight;
        return row;
    }

    private static void AddText(RectTransform row, string name, string text, float size, Color color, Vector2 offsetMin, Vector2 offsetMax, bool bold)
    {
        RectTransform area = RuntimeUI.CreateRect(row, name, new Vector2(0f, 1f), Vector2.one, offsetMin, offsetMax);
        TextMeshProUGUI tmp = RuntimeUI.CreateText(area, text, size, color, TextAlignmentOptions.MidlineLeft);
        if (bold) tmp.fontStyle = FontStyles.Bold;
    }

    // 3 ngôi sao ở góc trên phải dòng, phía trên nút Chơi
    private static void AddStars(RectTransform row, int stars)
    {
        UISkin skin = UISkin.Instance;
        if (skin == null || skin.starFilled == null || skin.starEmpty == null) return;
        for (int i = 0; i < 3; i++)
        {
            RectTransform star = RuntimeUI.CreateRect(row, "Star" + i, new Vector2(1f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            star.sizeDelta = new Vector2(StarSize, StarSize);
            star.anchoredPosition = new Vector2(-135f + (i - 1) * (StarSize + 6f), -32f);
            Image img = RuntimeUI.AddImage(star, Color.white, false);
            img.sprite = i < stars ? skin.starFilled : skin.starEmpty;
            img.preserveAspect = true;
        }
    }
}
