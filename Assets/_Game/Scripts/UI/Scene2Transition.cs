using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Scene 2 "Đêm xuống": cầu nối giữa một ngày sinh hoạt (Scene 1) và trận phòng thủ.
/// Phòng ngủ hiện ngay (không đen màn), đèn tối dần + máy quay đẩy chậm, tiêu đề và nhận xét sức khỏe hiện lần lượt,
/// sau đó là bảng tổng kết chỉ số (DaySummaryUI). Bấm "Vào trận!" thì chuyển scene luôn (hiệu ứng trượt lo phần chuyển).
/// Chạm màn hình trong lúc giới thiệu để bỏ qua.
/// </summary>
public class Scene2Transition : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup canvasGroup;
    public Image backgroundImage;
    [Tooltip("Dòng nhận xét sức khỏe (chữ được đổi font/màu lúc chạy)")]
    public TextMeshProUGUI statusTextUI;

    [Header("Transition Settings")]
    [Tooltip("Thời gian chờ khi không có dữ liệu ngày (chạy thẳng scene này để thử)")]
    public float waitDuration = 3.0f;
    public string nextSceneName = "Ingame_1";

    [Header("Hiệu ứng đêm xuống")]
    [Tooltip("Độ tối của lớp màn đêm phủ lên phòng ngủ")]
    [Range(0f, 1f)] public float nightDarkness = 0.45f;
    public Color nightColor = new Color(0.04f, 0.05f, 0.16f, 1f);
    [Tooltip("Phóng to dần ảnh nền (1 = đứng yên)")]
    public float zoomTo = 1.08f;
    [Tooltip("Thời gian giới thiệu trước khi hiện bảng tổng kết")]
    public float introDuration = 2.6f;

    [Header("Ngưỡng điểm sức khỏe (StomachDayData.GetHealthScore)")]
    [Tooltip("Điểm >= ngưỡng này: Trạng thái Tốt")]
    public float goodScoreThreshold = 0.3f;
    [Tooltip("Điểm < ngưỡng này: Trạng thái Xấu. Ở giữa: Trung bình")]
    public float badScoreThreshold = -0.3f;

    private const float ZoomSeconds = 12f;

    private Image night;
    private CanvasGroup titleGroup;
    private RectTransform titleRect;
    private CanvasGroup verdictGroup;
    private RectTransform verdictRect;
    private bool skipIntro;

    private void Start()
    {
        StartCoroutine(TransitionRoutine());
    }

    private void Update()
    {
        if (Time.unscaledTime > 0.5f && UnityEngine.InputSystem.Pointer.current != null &&
            UnityEngine.InputSystem.Pointer.current.press.wasPressedThisFrame)
            skipIntro = true;
    }

    private IEnumerator TransitionRoutine()
    {
        // Phòng ngủ hiện ngay, không tô màu: màu chỉ nằm ở nhãn nhận xét
        if (canvasGroup != null) canvasGroup.alpha = 1f;
        if (backgroundImage != null) backgroundImage.color = Color.white;

        float score = StomachDayData.Instance != null ? StomachDayData.Instance.GetHealthScore() : goodScoreThreshold;
        string summaryTitle;
        string statusLine;
        Color summaryColor;
        if (score >= goodScoreThreshold)
        {
            summaryTitle = "CƠ THỂ SUNG SỨC";
            summaryColor = RuntimeUI.HeaderGreen;
            statusLine = "Hệ miễn dịch sẵn sàng chiến đấu!";
        }
        else if (score >= badScoreThreshold)
        {
            summaryTitle = "CƠ THỂ TẠM ỔN";
            summaryColor = RuntimeUI.HeaderOrange;
            statusLine = "Hệ miễn dịch đang có vài điểm yếu...";
        }
        else
        {
            summaryTitle = "CƠ THỂ KIỆT QUỆ";
            summaryColor = RuntimeUI.HeaderRed;
            statusLine = "Vi khuẩn đang thừa cơ tấn công!";
        }

        BuildIntro(summaryTitle, statusLine, summaryColor);
        StartCoroutine(ZoomBackground());

        // 1. Đèn tối dần
        yield return Animate(0.9f, k => night.color = WithAlpha(nightColor, nightDarkness * Smooth(k)));

        // 2. Tiêu đề rồi nhãn nhận xét trồi lên
        yield return Animate(0.5f, k => Reveal(titleGroup, titleRect, k));
        yield return Animate(0.45f, k => Reveal(verdictGroup, verdictRect, k));

        for (float t = 0f; t < introDuration && !skipIntro; t += Time.unscaledDeltaTime) yield return null;

        if (StomachDayData.Instance != null)
        {
            // 3. Bảng tổng kết chỉ số; tiêu đề lùi xuống nền
            StartCoroutine(Animate(0.3f, k => titleGroup.alpha = verdictGroup.alpha = 1f - k));
            yield return DaySummaryUI.ShowAndWait(StomachDayData.Instance, summaryTitle, summaryColor);
        }
        else
        {
            for (float t = 0f; t < waitDuration; t += Time.unscaledDeltaTime) yield return null;
        }

        SceneChuyenCanhManager.LoadScene(nextSceneName);
    }

    /// <summary>
    /// Lớp màn đêm + tiêu đề + nhãn nhận xét, đặt trong khung phủ toàn màn hình
    /// (hiệu ứng trượt chuyển scene bỏ qua khối toàn màn hình nên không đè lên hoạt ảnh riêng).
    /// </summary>
    private void BuildIntro(string verdict, string statusLine, Color color)
    {
        Transform canvas = backgroundImage != null ? backgroundImage.canvas.transform : transform;
        RectTransform intro = RuntimeUI.Stretch(canvas, "NightIntro");
        intro.SetSiblingIndex(backgroundImage != null ? backgroundImage.transform.GetSiblingIndex() + 1 : 0);

        night = RuntimeUI.AddImage(RuntimeUI.Stretch(intro, "Night"), WithAlpha(nightColor, 0f), false);

        // Font Baloo2-Bold đã đậm sẵn: không bật FontStyles.Bold (TMP giãn chữ theo cỡ gốc khi đổi cỡ trong dòng)
        titleRect = RuntimeUI.CreateRect(intro, "Title", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        titleRect.sizeDelta = new Vector2(1400f, 260f);
        titleRect.anchoredPosition = new Vector2(0f, 170f);
        titleGroup = titleRect.gameObject.AddComponent<CanvasGroup>();
        titleGroup.alpha = 0f;
        AddLine(titleRect, "ĐÊM XUỐNG...", 120f, Color.white, 40f);
        AddLine(titleRect, "Trong lúc bạn ngủ, cơ thể tổng kết lại một ngày", 42f, new Color(0.85f, 0.88f, 1f, 1f), -80f);

        verdictRect = RuntimeUI.CreateRect(intro, "Verdict", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        verdictRect.sizeDelta = new Vector2(1000f, 110f);
        verdictRect.anchoredPosition = new Vector2(0f, -110f);
        verdictGroup = verdictRect.gameObject.AddComponent<CanvasGroup>();
        verdictGroup.alpha = 0f;
        // Chữ lớn tô màu trạng thái (sáng lên để nổi trên nền đêm), không khung: ảnh thanh có sẵn đều màu đỏ
        AddLine(verdictRect, verdict, 72f, Color.Lerp(color, Color.white, 0.3f), 0f);
        AddLine(verdictRect, statusLine, 40f, Color.white, -85f);

        // Dòng chữ cũ trong scene được thay bằng nhãn trên
        if (statusTextUI != null) statusTextUI.gameObject.SetActive(false);
    }

    private IEnumerator ZoomBackground()
    {
        if (backgroundImage == null) yield break;
        Transform bg = backgroundImage.transform;
        for (float t = 0f; t < ZoomSeconds && bg != null; t += Time.unscaledDeltaTime)
        {
            bg.localScale = Vector3.one * Mathf.Lerp(1f, zoomTo, Smooth(t / ZoomSeconds));
            yield return null;
        }
    }

    private static void Reveal(CanvasGroup group, RectTransform rect, float k)
    {
        group.alpha = k;
        float ease = 1f - Mathf.Pow(1f - k, 3f);
        rect.localScale = Vector3.one * Mathf.Lerp(0.92f, 1f, ease);
    }

    private static IEnumerator Animate(float seconds, System.Action<float> step)
    {
        for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
        {
            step(t / seconds);
            yield return null;
        }
        step(1f);
    }

    // Một dòng chữ có viền tối, tâm lệch y so với khung cha
    private static TextMeshProUGUI AddLine(RectTransform parent, string text, float size, Color color, float y)
    {
        TextMeshProUGUI tmp = RuntimeUI.CreateText(parent, text, size, color);
        RectTransform rt = tmp.rectTransform;
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.sizeDelta = new Vector2(400f, size * 1.4f);
        rt.anchoredPosition = new Vector2(0f, y);
        tmp.enableWordWrapping = false;
        AddShadow(tmp);
        return tmp;
    }

    private static void AddShadow(TextMeshProUGUI text)
    {
        text.outlineWidth = 0.18f;
        text.outlineColor = new Color(0.08f, 0.06f, 0.15f, 0.9f);
    }

    private static float Smooth(float k) => k * k * (3f - 2f * k);

    private static Color WithAlpha(Color c, float a) => new Color(c.r, c.g, c.b, a);
}
