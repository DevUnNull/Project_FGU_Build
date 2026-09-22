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
    public Color badColor = new Color(0.8f, 0.2f, 0.2f, 1f); // Đỏ mờ

    private void Start()
    {
        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        // Set initial state
        canvasGroup.alpha = 0f;

        // Check data and set color/text
        bool isGoodState = true;
        if (StomachDayData.Instance != null)
        {
            // Simple logic: if bad events happened or stats went down, it's a bad state
            if (StomachDayData.Instance.gastricAcidLevel > 0 || 
                StomachDayData.Instance.toxinObstaclesCount > 0 ||
                StomachDayData.Instance.atpRecoveryMultiplier < 1.0f ||
                StomachDayData.Instance.mucosaHpMultiplier < 1.0f)
            {
                isGoodState = false;
            }
        }

        if (isGoodState)
        {
            backgroundImage.color = goodColor;
            statusTextUI.text = "Cơ thể sung sức! Hệ miễn dịch sẵn sàng chiến đấu.";
        }
        else
        {
            backgroundImage.color = badColor;
            statusTextUI.text = "Cơ thể kiệt huệ! Vi khuẩn đang thừa cơ tấn công.";
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

        // Wait
        yield return new WaitForSeconds(waitDuration);

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
        SceneManager.LoadScene(nextSceneName);
    }
}
