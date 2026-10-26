using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bảng "CÁCH CHƠI" 4 bước có hình minh họa: đặt tế bào, ghép 2 con cùng loại, Roll, bán.
/// Tự hiện 1 lần ở trận đầu tiên (khi bấm Bắt đầu) và mở lại bằng nút "?" trên HUD. Tạm dừng trận khi đang mở.
/// </summary>
public class HowToPlayUI : MonoBehaviour
{
    private const string SeenKey = "CallOutMyCells_HowToPlaySeen";
    private const float RowHeight = 128f;
    private const float RowSpacing = 12f;
    private const float IllustrationWidth = 330f;
    private static readonly Color RowColor = new Color(1f, 0.97f, 0.88f, 0.9f);
    private static readonly Color Highlight = new Color(0.85f, 0.45f, 0.05f, 1f);

    private static HowToPlayUI open;
    private bool pausedByMe;

    public static bool HasSeen => PlayerPrefs.GetInt(SeenKey, 0) == 1;

    public static void ResetSeen()
    {
        PlayerPrefs.DeleteKey(SeenKey);
        PlayerPrefs.Save();
    }
    public static bool IsOpen => open != null;

    public static void Show()
    {
        if (open != null) return;
        PlayerPrefs.SetInt(SeenKey, 1);
        PlayerPrefs.Save();

        Canvas canvas = RuntimeUI.CreateOverlayCanvas("HowToPlay", 480);
        open = canvas.gameObject.AddComponent<HowToPlayUI>();
        open.Build(canvas);
    }

    private void Build(Canvas canvas)
    {
        if (Time.timeScale > 0f)
        {
            Time.timeScale = 0f;
            pausedByMe = true;
        }

        CanvasGroup group = canvas.gameObject.AddComponent<CanvasGroup>();
        RectTransform root = RuntimeUI.Stretch(canvas.transform, "Backdrop");
        RuntimeUI.AddImage(root, RuntimeUI.Backdrop);

        RectTransform card = RuntimeUI.CreateCard(root, new Vector2(1400f, 900f), "CÁCH CHƠI", RuntimeUI.HeaderGreen, out RectTransform body);
        RectTransform list = RuntimeUI.CreateRect(body, "Steps", Vector2.zero, Vector2.one, new Vector2(0f, 95f), Vector2.zero);

        NotebookCatalog catalog = NotebookCatalog.Instance;
        Sprite hongCau = catalog != null ? catalog.FindHero("Cell_HongCau")?.icon : null;
        Sprite cellB = catalog != null ? catalog.FindHero("Cell_B")?.icon : null;

        RectTransform row1 = CreateRow(list, 0, "1", $"Kéo tế bào từ <b>khay dưới</b> lên một <color=#2E8B3E><b>ô trống</b></color> trên bàn. Mỗi tế bào tốn ATP (giá ghi dưới khay).");
        AddIcon(row1, hongCau, new Vector2(165f, 0f), 100f);

        RectTransform row2 = CreateRow(list, 1, "2", $"Kéo 2 tế bào <b>CÙNG LOẠI, CÙNG CẤP</b> chồng lên nhau để <color=#D97300><b>nâng cấp</b></color>: máu & sát thương x1.8 (cấp 3: x3.2). Ô sáng <color=#D97300><b>vàng</b></color> là ghép được.");
        AddIcon(row2, cellB, new Vector2(88f, 0f), 72f);
        AddLabel(row2, "+", new Vector2(137f, 0f));
        AddIcon(row2, cellB, new Vector2(186f, 0f), 72f);
        AddLabel(row2, "=", new Vector2(235f, 0f));
        RectTransform big = AddIcon(row2, cellB, new Vector2(290f, 0f), 96f);
        AddBadge(big, "2");

        RectTransform row3 = CreateRow(list, 2, "3", "Mỗi <b>wave mới</b>, khay tự đổi tế bào <color=#2E8B3E><b>miễn phí</b></color>. Muốn đổi thêm để tìm con cùng loại? Bấm <b>Roll</b> (2 ATP). Tế bào trên bàn vẫn giữ nguyên.");
        RectTransform rollIcon = AddIcon(row3, FindSceneSprite("Roll"), new Vector2(165f, 0f), 100f);
        TextMeshProUGUI rollText = RuntimeUI.CreateText(rollIcon, "Roll", 26f, Color.white);
        rollText.fontStyle = FontStyles.Bold;

        RectTransform row4 = CreateRow(list, 3, "4", "Kéo tế bào vào <b>thùng rác</b> để bán và lấy lại ATP (tế bào đã ghép trả lại tiền của mọi con đã gộp).");
        AddIcon(row4, FindTrashSprite(), new Vector2(165f, 0f), 100f);

        bool hasHeroTutorial = HeroTutorialManager.Instance != null;
        if (hasHeroTutorial)
        {
            Button heroes = RuntimeUI.CreateButton(body, "Giới thiệu tế bào", RuntimeUI.HeaderOrange, new Vector2(330f, 76f), () =>
            {
                Close();
                HeroTutorialManager.Instance.OpenHelp();
            });
            Place(heroes, new Vector2(-190f, 40f));
        }

        Button play = RuntimeUI.CreateButton(body, "Chơi thôi!", RuntimeUI.HeaderGreen, new Vector2(260f, 76f), Close);
        Place(play, new Vector2(hasHeroTutorial ? 190f : 0f, 40f));

        StartCoroutine(RuntimeUI.PopIn(group, card));
    }

    private void Close()
    {
        if (pausedByMe) GameSpeed.Resume();
        pausedByMe = false;
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (open == this) open = null;
    }

    private static void Place(Button button, Vector2 position)
    {
        RectTransform rt = (RectTransform)button.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.anchoredPosition = position;
    }

    private static RectTransform CreateRow(RectTransform list, int index, string number, string text)
    {
        float top = -index * (RowHeight + RowSpacing);
        RectTransform row = RuntimeUI.CreateRect(list, "Step" + number, new Vector2(0f, 1f), Vector2.one,
            new Vector2(0f, top - RowHeight), new Vector2(0f, top));
        RuntimeUI.AddImage(row, RowColor, false);

        RectTransform numberRt = RuntimeUI.CreateRect(row, "Number", new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero, Vector2.zero);
        numberRt.sizeDelta = new Vector2(44f, 44f);
        numberRt.anchoredPosition = new Vector2(26f, -26f);
        TextMeshProUGUI num = RuntimeUI.CreateText(numberRt, number, 30f, Highlight);
        num.fontStyle = FontStyles.Bold;

        RectTransform textArea = RuntimeUI.CreateRect(row, "Text", Vector2.zero, Vector2.one, new Vector2(IllustrationWidth + 20f, 8f), new Vector2(-20f, -8f));
        TextMeshProUGUI tmp = RuntimeUI.CreateText(textArea, text, 28f, RuntimeUI.TextDark, TextAlignmentOptions.MidlineLeft);
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 20f;
        tmp.fontSizeMax = 28f;
        return row;
    }

    private static RectTransform AddIcon(RectTransform row, Sprite sprite, Vector2 position, float size)
    {
        RectTransform rt = RuntimeUI.CreateRect(row, "Icon", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
        rt.sizeDelta = new Vector2(size, size);
        rt.anchoredPosition = position;
        if (sprite == null) return rt;
        Image img = RuntimeUI.AddImage(rt, Color.white, false);
        img.sprite = sprite;
        img.preserveAspect = true;
        return rt;
    }

    private static void AddLabel(RectTransform row, string label, Vector2 position)
    {
        RectTransform rt = RuntimeUI.CreateRect(row, "Label", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
        rt.sizeDelta = new Vector2(40f, 60f);
        rt.anchoredPosition = position;
        TextMeshProUGUI t = RuntimeUI.CreateText(rt, label, 44f, Highlight);
        t.fontStyle = FontStyles.Bold;
    }

    private static void AddBadge(RectTransform icon, string level)
    {
        RectTransform rt = RuntimeUI.CreateRect(icon, "Badge", new Vector2(1f, 0f), new Vector2(1f, 0f), Vector2.zero, Vector2.zero);
        rt.sizeDelta = new Vector2(46f, 46f);
        rt.anchoredPosition = new Vector2(-6f, 10f);
        Sprite badge = UISkin.Instance != null ? UISkin.Instance.levelBadge : null;
        if (badge != null)
        {
            Image img = RuntimeUI.AddImage(rt, Color.white, false);
            img.sprite = badge;
            img.preserveAspect = true;
        }
        TextMeshProUGUI t = RuntimeUI.CreateText(rt, level, 30f, new Color(0.45f, 0.22f, 0.02f, 1f));
        t.fontStyle = FontStyles.Bold;
    }

    // Lấy hình nút Roll / thùng rác ngay trong scene để minh họa đúng thứ người chơi thấy
    private static Sprite FindSceneSprite(string objectName)
    {
        GameObject go = GameObject.Find(objectName);
        if (go == null) return null;
        Image img = go.GetComponent<Image>() ?? go.GetComponentInChildren<Image>();
        if (img != null) return img.sprite;
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>() ?? go.GetComponentInChildren<SpriteRenderer>();
        return sr != null ? sr.sprite : null;
    }

    private static Sprite FindTrashSprite()
    {
        DestroyUnitTrigger trash = UnityEngine.Object.FindFirstObjectByType<DestroyUnitTrigger>();
        if (trash == null) return null;
        SpriteRenderer sr = trash.GetComponent<SpriteRenderer>() ?? trash.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) return sr.sprite;
        Image img = trash.GetComponent<Image>() ?? trash.GetComponentInChildren<Image>();
        return img != null ? img.sprite : null;
    }
}
