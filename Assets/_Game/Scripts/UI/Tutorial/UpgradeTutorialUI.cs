using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bảng hướng dẫn "SAO & NÂNG CẤP": sao kiếm được thế nào, dùng sao ở đâu, nâng cấp có tác dụng gì.
/// - Ở MainMap: tự hiện 1 lần khi người chơi vừa có sao chưa dùng (sau trận thắng đầu tiên),
///   nút "Đi nâng cấp" về Menu chính và mở thẳng bảng Nâng cấp.
/// - Ở Menu chính: hiện 1 lần khi mở bảng Nâng cấp lần đầu (nếu chưa xem).
/// </summary>
public class UpgradeTutorialUI : MonoBehaviour
{
    private const string SeenKey = "CallOutMyCells_UpgradeTutorialSeen";
    private const string MenuScene = "MainMenuScene";
    private const float RowHeight = 130f;
    private const float RowSpacing = 12f;
    private const float IllustrationWidth = 300f;
    private static readonly Color RowColor = new Color(1f, 0.97f, 0.88f, 0.9f);

    private static UpgradeTutorialUI open;

    public static bool HasSeen => PlayerPrefs.GetInt(SeenKey, 0) == 1;
    public static bool IsOpen => open != null;

    public static void ResetSeen()
    {
        PlayerPrefs.DeleteKey(SeenKey);
        PlayerPrefs.Save();
    }

    /// <summary>MainMap: hiện nếu chưa xem và đang có sao chưa dùng.</summary>
    public static void TryShowOnMap()
    {
        if (HasSeen || open != null || UpgradeProgress.AvailableStars <= 0) return;
        Show(true);
    }

    /// <param name="fromMap">true = có nút "Đi nâng cấp" (về Menu chính); false = chỉ "Đã hiểu"</param>
    public static void Show(bool fromMap)
    {
        if (open != null) return;
        PlayerPrefs.SetInt(SeenKey, 1);
        PlayerPrefs.Save();

        Canvas canvas = RuntimeUI.CreateOverlayCanvas("UpgradeTutorial", 470);
        open = canvas.gameObject.AddComponent<UpgradeTutorialUI>();
        open.Build(canvas, fromMap);
    }

    private void Build(Canvas canvas, bool fromMap)
    {
        CanvasGroup group = canvas.gameObject.AddComponent<CanvasGroup>();
        RectTransform root = RuntimeUI.Stretch(canvas.transform, "Backdrop");
        RuntimeUI.AddImage(root, RuntimeUI.Backdrop);

        RectTransform card = RuntimeUI.CreateCard(root, new Vector2(1350f, 860f), "SAO & NÂNG CẤP", RuntimeUI.HeaderOrange, out RectTransform body);
        RectTransform list = RuntimeUI.CreateRect(body, "Steps", Vector2.zero, Vector2.one, new Vector2(0f, 95f), Vector2.zero);

        UISkin skin = UISkin.Instance;
        NotebookCatalog catalog = NotebookCatalog.Instance;
        int available = UpgradeProgress.AvailableStars;

        RectTransform row1 = CreateRow(list, 0,
            "Thắng mỗi ngày để nhận <b>sao</b>: <color=#D97300><b>3 sao</b></color> khi Trái Tim không mất máu, 2 sao khi còn từ 50% máu, 1 sao khi còn ít hơn. Mỗi ngày tính số sao <b>cao nhất</b>.");
        for (int i = 0; i < 3; i++) AddIcon(row1, skin != null ? skin.starFilled : null, 40f + i * 78f, 72f);

        RectTransform row2 = CreateRow(list, 1,
            "Mỗi sao mua <b>1 cấp nâng cấp</b>: tế bào bền hơn, mạnh hơn, thêm ATP khởi đầu hoặc thêm máu Trái Tim. Nâng cấp có hiệu lực ở <b>mọi trận sau</b>.");
        if (catalog != null)
        {
            for (int i = 0; i < catalog.heroes.Count && i < 4; i++) AddIcon(row2, catalog.heroes[i].icon, 22f + i * 66f, 62f);
        }

        RectTransform row3 = CreateRow(list, 2,
            "Vào <b>Menu chính → Nâng cấp</b>. Số trong ngoặc trên nút là sao chưa dùng. Có nút <b>Đặt lại</b> để lấy lại sao và chọn nâng cấp khác.");
        AddPillLabel(row3, "Nâng cấp" + (available > 0 ? $" ({available})" : ""));

        RectTransform row4 = CreateRow(list, 3,
            "Mẹo: chơi lại ngày cũ với lối sống tốt hơn (ít mất máu hơn) để lấy đủ 3 sao!");
        AddIcon(row4, skin != null ? skin.heartIcon : null, 110f, 84f);

        if (fromMap)
        {
            Button go = RuntimeUI.CreateButton(body, available > 0 ? $"Đi nâng cấp ({available} sao)" : "Đi nâng cấp", RuntimeUI.HeaderOrange, new Vector2(380f, 76f), () =>
            {
                MainMenuUI.OpenUpgradesOnLoad = true;
                Destroy(canvas.gameObject);
                SceneVideoTransition.LoadScene(MenuScene);
            });
            Place(go, new Vector2(-210f, 40f));

            Button later = RuntimeUI.CreateButton(body, "Để sau", RuntimeUI.HeaderGreen, new Vector2(240f, 76f), () => Destroy(canvas.gameObject));
            Place(later, new Vector2(210f, 40f));
        }
        else
        {
            Button ok = RuntimeUI.CreateButton(body, "Đã hiểu", RuntimeUI.HeaderGreen, new Vector2(260f, 76f), () => Destroy(canvas.gameObject));
            Place(ok, new Vector2(0f, 40f));
        }

        StartCoroutine(RuntimeUI.PopIn(group, card));
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

    private static RectTransform CreateRow(RectTransform list, int index, string text)
    {
        float top = -index * (RowHeight + RowSpacing);
        RectTransform row = RuntimeUI.CreateRect(list, "Step" + index, new Vector2(0f, 1f), Vector2.one,
            new Vector2(0f, top - RowHeight), new Vector2(0f, top));
        RuntimeUI.AddImage(row, RowColor, false);

        RectTransform textArea = RuntimeUI.CreateRect(row, "Text", Vector2.zero, Vector2.one, new Vector2(IllustrationWidth + 20f, 8f), new Vector2(-20f, -8f));
        TextMeshProUGUI tmp = RuntimeUI.CreateText(textArea, text, 28f, RuntimeUI.TextDark, TextAlignmentOptions.MidlineLeft);
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 20f;
        tmp.fontSizeMax = 28f;
        return row;
    }

    private static void AddIcon(RectTransform row, Sprite sprite, float left, float size)
    {
        if (sprite == null) return;
        RectTransform rt = RuntimeUI.CreateRect(row, "Icon", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
        rt.sizeDelta = new Vector2(size, size);
        rt.anchoredPosition = new Vector2(left + size * 0.5f, 0f);
        Image img = RuntimeUI.AddImage(rt, Color.white, false);
        img.sprite = sprite;
        img.preserveAspect = true;
    }

    // Hình nút "Nâng cấp" thu nhỏ để người chơi nhận ra nút cần bấm ở Menu chính
    private static void AddPillLabel(RectTransform row, string label)
    {
        Button pill = RuntimeUI.CreatePillButton(row, label, new Vector2(250f, 76f), null);
        pill.interactable = false;
        ColorBlock colors = pill.colors;
        colors.disabledColor = Color.white; // chỉ là hình minh họa, giữ màu như nút thật
        pill.colors = colors;
        RectTransform rt = (RectTransform)pill.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
        rt.anchoredPosition = new Vector2(150f, 0f);
    }
}
