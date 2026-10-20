using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Bảng tổng kết cuối ngày (Scene 2): so sánh chỉ số cơ thể với đầu ngày (100%)
/// và cho biết mỗi chỉ số sẽ ảnh hưởng thế nào trong trận phòng thủ.
/// </summary>
public static class DaySummaryUI
{
    public static IEnumerator ShowAndWait(StomachDayData data, string title, Color headerColor)
    {
        Canvas canvas = RuntimeUI.CreateOverlayCanvas("DaySummary", 400);
        CanvasGroup group = canvas.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;

        RectTransform root = RuntimeUI.Stretch(canvas.transform, "Backdrop");
        RuntimeUI.AddImage(root, RuntimeUI.Backdrop);

        RectTransform card = RuntimeUI.CreateCard(root, new Vector2(1150f, 700f), title, headerColor, out RectTransform body);

        string[] labels =
        {
            "Kháng thể  <size=70%>(Máu & Sát thương tế bào)</size>",
            "Thể lực  <size=70%>(ATP khởi điểm & tốc độ hồi ATP)</size>",
            "Nước  <size=70%>(Tốc độ đánh Tế bào B, xu Hồng Cầu)</size>",
            "Hồi phục  <size=70%>(Máu Trái Tim)</size>",
        };
        float[] values =
        {
            data.immunityMultiplier, data.energyMultiplier, data.hydrationMultiplier, data.recoveryMultiplier
        };

        const float rowHeight = 70f;
        float y = 0f;
        for (int i = 0; i < labels.Length; i++)
        {
            AddRow(body, y, labels[i], FormatMultiplier(values[i]));
            y += rowHeight;
        }

        int lockedTiles = Mathf.RoundToInt(data.toxicityLevel);
        string toxinValue = lockedTiles > 0
            ? $"<color=#{ColorUtility.ToHtmlStringRGB(RuntimeUI.NegativeText)}>{lockedTiles} ô bị khóa</color>"
            : $"<color=#{ColorUtility.ToHtmlStringRGB(RuntimeUI.PositiveText)}>An toàn</color>";
        AddRow(body, y, "Độc tố  <size=70%>(Ô bàn cờ bị khóa)</size>", toxinValue);
        y += rowHeight;

        bool acidWave = data.toxicityLevel >= StomachDayData.GastricAcidThreshold;
        string acidValue = acidWave
            ? $"<color=#{ColorUtility.ToHtmlStringRGB(RuntimeUI.NegativeText)}>Sẽ xảy ra ở wave 2!</color>"
            : $"<color=#{ColorUtility.ToHtmlStringRGB(RuntimeUI.PositiveText)}>Không</color>";
        AddRow(body, y, "Sóng Axit Dạ Dày", acidValue);

        bool closed = false;
        var button = RuntimeUI.CreateButton(body, "Vào trận!", headerColor, new Vector2(320f, 76f), () => closed = true);
        RectTransform btnRt = (RectTransform)button.transform;
        btnRt.anchorMin = btnRt.anchorMax = new Vector2(0.5f, 0f);
        btnRt.anchoredPosition = new Vector2(0f, 38f);

        yield return RuntimeUI.PopIn(group, card);
        while (!closed) yield return null;
        yield return RuntimeUI.FadeOut(group);

        Object.Destroy(canvas.gameObject);
    }

    private static void AddRow(RectTransform body, float topOffset, string label, string value)
    {
        RectTransform row = RuntimeUI.CreateRect(body, "Row", new Vector2(0f, 1f), Vector2.one,
            new Vector2(0f, -topOffset - 64f), new Vector2(0f, -topOffset));
        RuntimeUI.CreateText(row, label, 34f, RuntimeUI.TextDark, TextAlignmentOptions.MidlineLeft);
        RuntimeUI.CreateText(row, value, 36f, RuntimeUI.TextDark, TextAlignmentOptions.MidlineRight);
    }

    private static string FormatMultiplier(float multiplier)
    {
        int percent = Mathf.RoundToInt(multiplier * 100f);
        int diff = percent - 100;
        if (diff == 0) return $"{percent}%";

        Color c = diff > 0 ? RuntimeUI.PositiveText : RuntimeUI.NegativeText;
        string sign = diff > 0 ? "+" : "";
        return $"{percent}%  <color=#{ColorUtility.ToHtmlStringRGB(c)}>({sign}{diff}%)</color>";
    }
}
