using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bảng "Nâng cấp cơ thể" (mở từ MainMap): dùng sao tốt nhất của các ngày để mua nâng cấp lâu dài.
/// </summary>
public class UpgradeUI : MonoBehaviour
{
    private static readonly Color RowColor = new Color(1f, 0.97f, 0.88f, 0.9f);
    private const float RowHeight = 108f;
    private const float RowSpacing = 10f;

    private RectTransform listArea;
    private TextMeshProUGUI starsText;

    public static void Open()
    {
        Canvas canvas = RuntimeUI.CreateOverlayCanvas("Upgrades", 450);
        canvas.gameObject.AddComponent<UpgradeUI>().Build(canvas);

        // Lần đầu mở bảng mà chưa xem hướng dẫn -> giải thích sao & nâng cấp ngay trên bảng
        if (!UpgradeTutorialUI.HasSeen) UpgradeTutorialUI.Show(false);
    }

    private void Build(Canvas canvas)
    {
        CanvasGroup group = canvas.gameObject.AddComponent<CanvasGroup>();
        RectTransform root = RuntimeUI.Stretch(canvas.transform, "Backdrop");
        RuntimeUI.AddImage(root, RuntimeUI.Backdrop);

        RectTransform card = RuntimeUI.CreateCard(root, new Vector2(1250f, 900f), "NÂNG CẤP CƠ THỂ", RuntimeUI.HeaderOrange, out RectTransform body);

        // Dòng sao hiện có
        RectTransform header = RuntimeUI.CreateRect(body, "Stars", new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -88f), Vector2.zero);
        Sprite star = UISkin.Instance != null ? UISkin.Instance.starFilled : null;
        float textLeft = 10f;
        if (star != null)
        {
            RectTransform starRt = RuntimeUI.CreateRect(header, "StarIcon", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
            starRt.sizeDelta = new Vector2(64f, 64f);
            starRt.anchoredPosition = new Vector2(42f, 0f);
            Image img = RuntimeUI.AddImage(starRt, Color.white, false);
            img.sprite = star;
            img.preserveAspect = true;
            textLeft = 90f;
        }
        RectTransform starsArea = RuntimeUI.CreateRect(header, "StarsText", Vector2.zero, Vector2.one, new Vector2(textLeft, 0f), Vector2.zero);
        starsText = RuntimeUI.CreateText(starsArea, "", 30f, RuntimeUI.TextDark, TextAlignmentOptions.MidlineLeft);

        listArea = RuntimeUI.CreateRect(body, "List", Vector2.zero, Vector2.one, new Vector2(0f, 95f), new Vector2(0f, -95f));

        Button reset = RuntimeUI.CreateButton(body, "Đặt lại", RuntimeUI.HeaderRed, new Vector2(220f, 72f), () =>
        {
            UpgradeProgress.ResetAll();
            Refresh();
        });
        RectTransform resetRt = (RectTransform)reset.transform;
        resetRt.anchorMin = resetRt.anchorMax = new Vector2(0.5f, 0f);
        resetRt.anchoredPosition = new Vector2(-140f, 30f);

        Button close = RuntimeUI.CreateButton(body, "Đóng", RuntimeUI.HeaderGreen, new Vector2(220f, 72f), () => Destroy(canvas.gameObject));
        RectTransform closeRt = (RectTransform)close.transform;
        closeRt.anchorMin = closeRt.anchorMax = new Vector2(0.5f, 0f);
        closeRt.anchoredPosition = new Vector2(140f, 30f);

        Refresh();
        StartCoroutine(RuntimeUI.PopIn(group, card));
    }

    private void Refresh()
    {
        starsText.text = $"Sao còn lại: {UpgradeProgress.AvailableStars} (tổng {GameProgress.TotalStars}). " +
                         "Thắng mỗi ngày mà không mất máu tim để được tới 3 sao.";

        for (int i = listArea.childCount - 1; i >= 0; i--)
        {
            GameObject old = listArea.GetChild(i).gameObject;
            old.SetActive(false);
            Destroy(old);
        }

        UpgradeType[] types = UpgradeProgress.All;
        for (int i = 0; i < types.Length; i++)
        {
            CreateRow(types[i], i);
        }
    }

    private const float UnitIconSize = 50f;
    private const float BigIconSize = 84f;

    /// <summary>
    /// Máu / sát thương: 4 tế bào (áp dụng cho mọi tế bào). ATP: Hồng cầu + đồng ATP. Máu tim: trái tim.
    /// Trả về mép phải của vùng hình để đặt chữ.
    /// </summary>
    private static float AddIllustration(RectTransform row, UpgradeType type)
    {
        UISkin skin = UISkin.Instance;
        NotebookCatalog catalog = NotebookCatalog.Instance;
        float x = 20f;

        switch (type)
        {
            case UpgradeType.CellHealth:
            case UpgradeType.CellDamage:
                if (catalog != null)
                {
                    foreach (NotebookCatalog.UnitEntry hero in catalog.heroes)
                    {
                        AddIcon(row, hero.icon, x, UnitIconSize);
                        x += UnitIconSize + 4f;
                    }
                }
                break;
            case UpgradeType.StartAtp:
                AddIcon(row, catalog != null ? catalog.FindHero("Cell_HongCau")?.icon : null, x, BigIconSize);
                x += BigIconSize + 6f;
                AddIcon(row, skin != null ? skin.atpIcon : null, x, UnitIconSize);
                x += UnitIconSize;
                break;
            case UpgradeType.HeartLife:
                AddIcon(row, skin != null ? skin.heartIcon : null, x, BigIconSize);
                x += BigIconSize;
                break;
        }
        return Mathf.Max(x, 24f);
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

    private void CreateRow(UpgradeType type, int index)
    {
        float top = -index * (RowHeight + RowSpacing);
        RectTransform row = RuntimeUI.CreateRect(listArea, "Row_" + type, new Vector2(0f, 1f), Vector2.one,
            new Vector2(0f, top - RowHeight), new Vector2(0f, top));
        RuntimeUI.AddImage(row, RowColor, false);

        int level = UpgradeProgress.GetLevel(type);

        // Hình minh họa bên trái: nâng cấp áp dụng cho tế bào nào / cho cái gì
        float textLeft = AddIllustration(row, type) + 20f;

        RectTransform titleArea = RuntimeUI.CreateRect(row, "Title", new Vector2(0f, 0.5f), new Vector2(1f, 1f), new Vector2(textLeft, 0f), new Vector2(-330f, -8f));
        TextMeshProUGUI title = RuntimeUI.CreateText(titleArea, $"{UpgradeProgress.GetTitle(type)}  –  Cấp {level}/{UpgradeProgress.MaxLevel}",
            32f, RuntimeUI.HeaderOrange, TextAlignmentOptions.MidlineLeft);
        title.fontStyle = FontStyles.Bold;

        RectTransform effectArea = RuntimeUI.CreateRect(row, "Effect", Vector2.zero, new Vector2(1f, 0.5f), new Vector2(textLeft, 8f), new Vector2(-330f, 0f));
        TextMeshProUGUI effect = RuntimeUI.CreateText(effectArea, UpgradeProgress.GetEffectText(type), 26f, RuntimeUI.TextDark, TextAlignmentOptions.MidlineLeft);
        effect.enableAutoSizing = true; // mô tả dài vẫn nằm gọn 1 dòng, không đè lên tiêu đề
        effect.fontSizeMin = 18f;
        effect.fontSizeMax = 26f;
        effect.enableWordWrapping = false;

        bool maxed = level >= UpgradeProgress.MaxLevel;
        string label = maxed ? "Tối đa" : $"Nâng ({UpgradeProgress.CostPerLevel} sao)";
        Button buy = RuntimeUI.CreateButton(row, label, RuntimeUI.HeaderGreen, new Vector2(280f, 76f), () =>
        {
            if (UpgradeProgress.TryBuy(type))
            {
                GameSfx.Play(GameSfx.Kind.Reward);
                Refresh();
            }
        });
        RectTransform buyRt = (RectTransform)buy.transform;
        buyRt.anchorMin = buyRt.anchorMax = new Vector2(1f, 0.5f);
        buyRt.anchoredPosition = new Vector2(-165f, 0f);
        buy.interactable = UpgradeProgress.CanBuy(type);
    }
}
