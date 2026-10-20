using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Thẻ "Bài học hôm nay" hiện sau trận (trước bảng Thắng/Thua):
/// nối kết quả trận đánh với các lựa chọn lối sống ở Scene 1.
/// </summary>
public static class BattleLessonUI
{
    public static IEnumerator ShowAndWait(StomachDayData data, bool victory)
    {
        Canvas canvas = RuntimeUI.CreateOverlayCanvas("BattleLesson", 450);
        CanvasGroup group = canvas.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;

        RectTransform root = RuntimeUI.Stretch(canvas.transform, "Backdrop");
        RuntimeUI.AddImage(root, RuntimeUI.Backdrop);

        string title = victory ? "BÀI HỌC HÔM NAY: CHIẾN THẮNG!" : "BÀI HỌC HÔM NAY: CƠ THỂ GỤC NGÃ";
        Color headerColor = victory ? RuntimeUI.HeaderGreen : RuntimeUI.HeaderRed;
        RectTransform card = RuntimeUI.CreateCard(root, new Vector2(1200f, 720f), title, headerColor, out RectTransform body);

        RectTransform textArea = RuntimeUI.CreateRect(body, "LessonText", Vector2.zero, Vector2.one,
            new Vector2(0f, 100f), Vector2.zero);
        TextMeshProUGUI text = RuntimeUI.CreateText(textArea, BuildLesson(data, victory), 30f, RuntimeUI.TextDark,
            TextAlignmentOptions.TopLeft);
        text.lineSpacing = 8f;

        bool closed = false;
        var button = RuntimeUI.CreateButton(body, "Tiếp tục", headerColor, new Vector2(280f, 72f), () => closed = true);
        RectTransform btnRt = (RectTransform)button.transform;
        btnRt.anchorMin = btnRt.anchorMax = new Vector2(0.5f, 0f);
        btnRt.anchoredPosition = new Vector2(0f, 36f);

        yield return RuntimeUI.PopIn(group, card);
        while (!closed) yield return null;
        yield return RuntimeUI.FadeOut(group);

        Object.Destroy(canvas.gameObject);
    }

    private static string BuildLesson(StomachDayData data, bool victory)
    {
        List<string> strengths = new List<string>();
        List<string> weaknesses = new List<string>();

        Classify(data.immunityMultiplier, strengths, weaknesses,
            "Kháng thể mạnh ({0}%): ăn uống đủ chất và vận động giúp tế bào miễn dịch khỏe hơn.",
            "Kháng thể yếu ({0}%): thiếu ngủ, bỏ bữa, rượu bia làm tế bào miễn dịch yếu đi.");
        Classify(data.energyMultiplier, strengths, weaknesses,
            "Thể lực tốt ({0}%): cơ thể đủ năng lượng nên ATP dồi dào.",
            "Thể lực thấp ({0}%): thiếu năng lượng nên ATP hồi chậm.");
        Classify(data.hydrationMultiplier, strengths, weaknesses,
            "Đủ nước ({0}%): tế bào phản ứng nhanh hơn.",
            "Thiếu nước ({0}%): tế bào phản ứng chậm hơn.");
        Classify(data.recoveryMultiplier, strengths, weaknesses,
            "Hồi phục tốt ({0}%): ngủ sớm, nghỉ ngơi giúp cơ thể bền bỉ hơn.",
            "Hồi phục kém ({0}%): thức khuya khiến cơ thể không kịp phục hồi.");

        int lockedTiles = Mathf.RoundToInt(data.toxicityLevel);
        if (lockedTiles > 0)
        {
            weaknesses.Add($"{lockedTiles} ô bị khóa vì độc tố từ chất kích thích, đồ chiên rán, rượu bia.");
        }

        string positive = ColorUtility.ToHtmlStringRGB(RuntimeUI.PositiveText);
        string negative = ColorUtility.ToHtmlStringRGB(RuntimeUI.NegativeText);
        string result = victory
            ? "Hệ miễn dịch đã bảo vệ được cơ thể!\n\n"
            : "Mầm bệnh đã vượt qua hàng phòng thủ của cơ thể.\n\n";

        if (strengths.Count > 0)
        {
            result += $"<color=#{positive}>Điểm mạnh nhờ lối sống:</color>\n";
            foreach (var s in strengths) result += "• " + s + "\n";
            result += "\n";
        }

        if (weaknesses.Count > 0)
        {
            result += $"<color=#{negative}>Điểm yếu do lối sống:</color>\n";
            foreach (var w in weaknesses) result += "• " + w + "\n";
            result += "\n";
        }

        if (weaknesses.Count > 0 || !victory)
        {
            result += "<b>Lần sau hãy thử:</b> ngủ đủ 7–9 tiếng, ăn sáng đầy đủ, uống đủ nước và tránh rượu bia, đồ chiên rán.";
        }
        else
        {
            result += "<b>Tuyệt vời!</b> Hãy giữ lối sống lành mạnh này mỗi ngày.";
        }

        return result;
    }

    private static void Classify(float multiplier, List<string> strengths, List<string> weaknesses,
        string strengthFormat, string weaknessFormat)
    {
        int percent = Mathf.RoundToInt(multiplier * 100f);
        if (percent > 100) strengths.Add(string.Format(strengthFormat, percent));
        else if (percent < 100) weaknesses.Add(string.Format(weaknessFormat, percent));
    }
}
