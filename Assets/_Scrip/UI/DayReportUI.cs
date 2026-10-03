using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class DayReportUI : MonoBehaviour
{
    [Header("UI References (Gán trong Inspector hoặc tự động tìm)")]
    public GameObject reportPanel;
    public CanvasGroup backgroundCanvasGroup;
    public RectTransform dialogBoxContainer;
    public Transform thongBaoHeader;
    public TextMeshProUGUI reportText;
    public Button closeButton;

    [Header("Animation Settings")]
    public bool useTypewriterEffect = true;
    public float typewriterSpeed = 0.015f; // Tốc độ hiện từng chữ (giây/ký tự)
    public bool enableButtonBreathing = true; // Hiệu ứng nhịp thở cho nút Đóng

    private string fullReportText = "";
    private bool isClosing = false;
    private Coroutine buttonPulseCoroutine;

    private void Start()
    {
        // 1. Tự động liên kết các thành phần UI nếu chưa gán thủ công
        AutoFindOrBuildUI();

        // Nếu không có dữ liệu StomachDayData, ẩn panel báo cáo
        if (StomachDayData.Instance == null)
        {
            if (reportPanel != null) reportPanel.SetActive(false);
            return;
        }

        if (reportPanel != null)
        {
            reportPanel.SetActive(true);
        }

        // 2. Soạn nội dung báo cáo 5 chỉ số cơ thể
        fullReportText = "<b><size=120%>BÁO CÁO TÌNH TRẠNG CƠ THỂ</size></b>\n\n";

        float immunity = StomachDayData.Instance.immunityMultiplier;
        fullReportText += FormatStat("- Kháng thể (Máu & Dmg Tế bào)", immunity);

        float energy = StomachDayData.Instance.energyMultiplier;
        fullReportText += FormatStat("- Thể lực (ATP Khởi điểm)", energy);

        float hydration = StomachDayData.Instance.hydrationMultiplier;
        fullReportText += FormatStat("- Nước (Cooldown Tế Bào B & Hồng Cầu)", hydration);

        float recovery = StomachDayData.Instance.recoveryMultiplier;
        fullReportText += FormatStat("- Hồi phục (Máu Cơ Thể Trái Tim)", recovery);

        fullReportText += "\n<b><size=120%>ẢNH HƯỞNG TIÊU CỰC</size></b>\n\n";
        float toxicity = StomachDayData.Instance.toxicityLevel;
        if (toxicity > 0)
        {
            int lockedCount = Mathf.RoundToInt(toxicity);
            fullReportText += $"- <color=#ff4444>Độc tố cao (+{toxicity:0.#}) -> Xuất hiện {lockedCount} Ô Axit bị khóa!</color>\n";
        }
        else
        {
            fullReportText += "- <color=#44ff44>Độc tố an toàn (Không có ô bị khóa).</color>\n";
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseReport);
        }

        // 3. Chạy animation mở bảng & gõ chữ
        StartCoroutine(AnimateInRoutine());
    }

    private IEnumerator AnimateInRoutine()
    {
        // Ẩn ban đầu để làm animation pop-in
        if (dialogBoxContainer != null) dialogBoxContainer.localScale = Vector3.zero;
        if (backgroundCanvasGroup != null) backgroundCanvasGroup.alpha = 0f;
        if (closeButton != null) closeButton.transform.localScale = Vector3.zero;
        if (thongBaoHeader != null) thongBaoHeader.localScale = Vector3.zero;

        // A. Fade nền đen & nảy bảng DialogBox (EaseOutBack)
        float duration = 0.35f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (backgroundCanvasGroup != null) backgroundCanvasGroup.alpha = t;

            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            float easeBack = 1f + c3 * Mathf.Pow(t - 1f, 3) + c1 * Mathf.Pow(t - 1f, 2);

            if (dialogBoxContainer != null)
            {
                dialogBoxContainer.localScale = Vector3.one * easeBack;
            }

            yield return null;
        }

        if (dialogBoxContainer != null) dialogBoxContainer.localScale = Vector3.one;
        if (backgroundCanvasGroup != null) backgroundCanvasGroup.alpha = 1f;

        // B. Bounce nảy Header "Tình Trạng Cơ Thể" (ThongBao)
        if (thongBaoHeader != null)
        {
            elapsed = 0f;
            float headerDuration = 0.25f;
            while (elapsed < headerDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / headerDuration);
                float c1 = 1.70158f;
                float c3 = c1 + 1f;
                float easeBack = 1f + c3 * Mathf.Pow(t - 1f, 3) + c1 * Mathf.Pow(t - 1f, 2);
                thongBaoHeader.localScale = Vector3.one * easeBack;
                yield return null;
            }
            thongBaoHeader.localScale = Vector3.one;
        }

        // C. Hiệu ứng gõ chữ Typewriter cho ReportText
        if (reportText != null)
        {
            if (useTypewriterEffect)
            {
                yield return StartCoroutine(TypewriterTextRoutine(fullReportText));
            }
            else
            {
                reportText.text = fullReportText;
            }
        }

        // D. Nút Đóng xuất hiện & bắt đầu nhịp thở (Pulse)
        if (closeButton != null)
        {
            elapsed = 0f;
            float btnDuration = 0.25f;
            while (elapsed < btnDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / btnDuration);
                float c1 = 1.70158f;
                float c3 = c1 + 1f;
                float easeBack = 1f + c3 * Mathf.Pow(t - 1f, 3) + c1 * Mathf.Pow(t - 1f, 2);
                closeButton.transform.localScale = Vector3.one * easeBack;
                yield return null;
            }
            closeButton.transform.localScale = Vector3.one;

            if (enableButtonBreathing)
            {
                buttonPulseCoroutine = StartCoroutine(ButtonPulseRoutine());
            }
        }
    }

    private IEnumerator TypewriterTextRoutine(string fullText)
    {
        reportText.text = fullText;
        reportText.ForceMeshUpdate();

        int totalChars = reportText.textInfo.characterCount;
        reportText.maxVisibleCharacters = 0;

        for (int i = 0; i <= totalChars; i++)
        {
            reportText.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(typewriterSpeed);
        }

        reportText.maxVisibleCharacters = 99999;
    }

    private IEnumerator ButtonPulseRoutine()
    {
        if (closeButton == null) yield break;
        Vector3 baseScale = Vector3.one;

        while (!isClosing)
        {
            float wave = Mathf.Sin(Time.unscaledTime * 4.5f) * 0.04f;
            closeButton.transform.localScale = baseScale * (1.0f + wave);
            yield return null;
        }
    }

    private void CloseReport()
    {
        if (isClosing) return;
        StartCoroutine(AnimateOutRoutine());
    }

    private IEnumerator AnimateOutRoutine()
    {
        isClosing = true;

        if (buttonPulseCoroutine != null)
        {
            StopCoroutine(buttonPulseCoroutine);
        }

        // Bấm nút: Nút thu nhỏ lại 0.9x tạo cảm giác bấm sướng tay
        if (closeButton != null)
        {
            closeButton.transform.localScale = Vector3.one * 0.9f;
        }

        yield return new WaitForSecondsRealtime(0.05f);

        // Bảng DialogBox thu nhỏ & mờ dần mượt mà
        float duration = 0.22f;
        float elapsed = 0f;

        Vector3 startScale = dialogBoxContainer != null ? dialogBoxContainer.localScale : Vector3.one;
        float startAlpha = backgroundCanvasGroup != null ? backgroundCanvasGroup.alpha : 1f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            float easeIn = c3 * t * t * t - c1 * t * t;
            float scaleFactor = Mathf.Clamp01(1f - t);

            if (dialogBoxContainer != null)
            {
                dialogBoxContainer.localScale = startScale * scaleFactor;
            }

            if (backgroundCanvasGroup != null)
            {
                backgroundCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            }

            yield return null;
        }

        if (reportPanel != null)
        {
            reportPanel.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void AutoFindOrBuildUI()
    {
        if (reportPanel == null)
        {
            Transform panelTr = transform.Find("ReportPanel");
            if (panelTr != null) reportPanel = panelTr.gameObject;
            else reportPanel = gameObject;
        }

        if (backgroundCanvasGroup == null && reportPanel != null)
        {
            backgroundCanvasGroup = reportPanel.GetComponent<CanvasGroup>();
            if (backgroundCanvasGroup == null)
            {
                backgroundCanvasGroup = reportPanel.AddComponent<CanvasGroup>();
            }
        }

        if (dialogBoxContainer == null && reportPanel != null)
        {
            Transform dialogTr = reportPanel.transform.Find("DialogBox");
            if (dialogTr != null) dialogBoxContainer = dialogTr.GetComponent<RectTransform>();
        }

        if (thongBaoHeader == null && dialogBoxContainer != null)
        {
            thongBaoHeader = dialogBoxContainer.Find("ThongBao");
        }

        if (reportText == null)
        {
            reportText = GetComponentInChildren<TextMeshProUGUI>();
        }

        if (closeButton == null)
        {
            closeButton = GetComponentInChildren<Button>();
        }
    }

    private string FormatStat(string statName, float multiplier)
    {
        float percent = Mathf.Round(multiplier * 100f);
        if (multiplier > 1.001f)
        {
            return $"{statName}: <color=#44ff44>{percent}% (Tăng cường)</color>\n";
        }
        else if (multiplier < 0.999f)
        {
            return $"{statName}: <color=#ff4444>{percent}% (Suy giảm)</color>\n";
        }
        else
        {
            return $"{statName}: <color=white>{percent}% (Bình thường)</color>\n";
        }
    }
}
