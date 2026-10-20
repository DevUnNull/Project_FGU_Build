using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class Scene2Transition : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup canvasGroup;
    public Image backgroundImage;
    public TextMeshProUGUI statusTextUI;

    [Header("Transition Settings")]
    public float fadeDuration = 1.0f;
    public float waitDuration = 3.0f;
    public string nextSceneName = "Ingame_1"; // Thay tên scene nếu cần

    [Header("Visual Configs")]
    public Color goodColor = new Color(0.2f, 0.8f, 0.2f, 1f); // Xanh lá
    public Color mediumColor = new Color(0.9f, 0.7f, 0.2f, 1f); // Vàng cam
    public Color badColor = new Color(0.8f, 0.2f, 0.2f, 1f); // Đỏ mờ

    [Header("Ngưỡng điểm sức khỏe (StomachDayData.GetHealthScore)")]
    [Tooltip("Điểm >= ngưỡng này: Trạng thái Tốt")]
    public float goodScoreThreshold = 0.3f;
    [Tooltip("Điểm < ngưỡng này: Trạng thái Xấu. Ở giữa: Trung bình")]
    public float badScoreThreshold = -0.3f;

    private void Start()
    {
        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        // Set initial state
        canvasGroup.alpha = 0f;

        // Chấm điểm sức khỏe tổng hợp -> 3 mức Tốt / Trung bình / Xấu
        float score = StomachDayData.Instance != null ? StomachDayData.Instance.GetHealthScore() : goodScoreThreshold;
        string summaryTitle;
        Color summaryColor;

        if (score >= goodScoreThreshold)
        {
            summaryTitle = "CƠ THỂ SUNG SỨC";
            summaryColor = RuntimeUI.HeaderGreen;
            backgroundImage.color = goodColor;
            statusTextUI.text = "Cơ thể sung sức! Hệ miễn dịch sẵn sàng chiến đấu.";
        }
        else if (score >= badScoreThreshold)
        {
            summaryTitle = "CƠ THỂ TẠM ỔN";
            summaryColor = RuntimeUI.HeaderOrange;
            backgroundImage.color = mediumColor;
            statusTextUI.text = "Cơ thể tạm ổn... nhưng hệ miễn dịch đang có điểm yếu.";
        }
        else
        {
            summaryTitle = "CƠ THỂ KIỆT QUỆ";
            summaryColor = RuntimeUI.HeaderRed;
            backgroundImage.color = badColor;
            statusTextUI.text = "Cơ thể kiệt quệ! Vi khuẩn đang thừa cơ tấn công.";
        }

        // Fade in
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Wait: có dữ liệu ngày -> đọc thông báo 1 chút rồi hiện bảng tổng kết chỉ số, chờ người chơi bấm "Vào trận"
        if (StomachDayData.Instance != null)
        {
            yield return new WaitForSeconds(1.2f);
            yield return DaySummaryUI.ShowAndWait(StomachDayData.Instance, summaryTitle, summaryColor);
        }
        else
        {
            yield return new WaitForSeconds(waitDuration);
        }

        // Fade out
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(timer / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        // Load next scene
        SceneChuyenCanhManager.LoadScene(nextSceneName);
    }
}
