using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Script tạo hiệu ứng mượt mà cho Banner "Chọn ngày" (Icon) và Nút Quay lại (Back) trong Scene MainMap.
/// An toàn 100% không bị co scale về 0 khi chuyển scene.
/// </summary>
public class MainMapUIAnimation : MonoBehaviour
{
    [Header("UI Element References")]
    public RectTransform iconTitle;   // Banner "Chọn ngày"
    public RectTransform backButton;  // Nút Back mũi tên

    [Header("Icon Idle Animation Settings")]
    public bool enableIconAnim = true;
    public float floatSpeed = 2.0f;
    public float floatAmount = 5.0f;
    public float pulseSpeed = 2.2f;
    public float pulseAmount = 0.03f;

    [Header("Back Button Navigation")]
    public string targetSceneName = "MainMenuScene";
    public float loadDelay = 0.15f;

    private Vector2 iconOrigPos;
    private Vector3 iconOrigScale = Vector3.one;
    private Vector3 backOrigScale = Vector3.one;
    private bool hasCachedOriginals = false;

    private void Awake()
    {
        AutoFindElements();
        CacheOriginals();
    }

    private void AutoFindElements()
    {
        if (iconTitle == null)
        {
            Transform t = transform.Find("Icon");
            if (t != null) iconTitle = t.GetComponent<RectTransform>();
        }

        if (backButton == null)
        {
            Transform t = transform.Find("Back");
            if (t != null) backButton = t.GetComponent<RectTransform>();
        }

        if (backButton != null)
        {
            WaveButtonAnimation anim = backButton.GetComponent<WaveButtonAnimation>();
            if (anim == null)
            {
                backButton.gameObject.AddComponent<WaveButtonAnimation>();
            }

            Button btn = backButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveListener(OnBackButtonClicked);
                btn.onClick.AddListener(OnBackButtonClicked);
            }
        }
    }

    public void CacheOriginals(bool forceRecache = false)
    {
        if (hasCachedOriginals && !forceRecache) return;

        if (iconTitle != null)
        {
            iconOrigPos = iconTitle.anchoredPosition;
            if (iconTitle.localScale.sqrMagnitude > 0.1f)
                iconOrigScale = iconTitle.localScale;
            else
                iconOrigScale = Vector3.one;
        }

        if (backButton != null)
        {
            if (backButton.localScale.sqrMagnitude > 0.1f)
                backOrigScale = backButton.localScale;
            else
                backOrigScale = Vector3.one;
        }

        hasCachedOriginals = true;
    }

    private void OnEnable()
    {
        AutoFindElements();
        CacheOriginals();

        ResetToOriginals();

        if (Application.isPlaying)
        {
            StartCoroutine(PlayEntranceRoutine());
        }
    }

    private void OnDisable()
    {
        ResetToOriginals();
    }

    public void ResetToOriginals()
    {
        if (!hasCachedOriginals) return;

        if (iconTitle != null)
        {
            iconTitle.anchoredPosition = iconOrigPos;
            iconTitle.localScale = iconOrigScale;
        }

        if (backButton != null)
        {
            if (backOrigScale.sqrMagnitude < 0.1f) backOrigScale = Vector3.one;
            backButton.localScale = backOrigScale;
        }
    }

    private void Update()
    {
        if (!Application.isPlaying) return;

        if (enableIconAnim && iconTitle != null)
        {
            float time = Time.unscaledTime;
            float offsetY = Mathf.Sin(time * floatSpeed) * floatAmount;
            iconTitle.anchoredPosition = iconOrigPos + new Vector2(0f, offsetY);

            float scaleDamp = Mathf.Sin(time * pulseSpeed) * pulseAmount;
            iconTitle.localScale = iconOrigScale * (1f + scaleDamp);
        }
    }

    private IEnumerator PlayEntranceRoutine()
    {
        if (iconTitle != null) iconTitle.localScale = Vector3.zero;
        if (backButton != null) backButton.localScale = Vector3.zero;

        yield return StartCoroutine(PopScaleRoutine(iconTitle, iconOrigScale, 0.35f));
        yield return StartCoroutine(PopScaleRoutine(backButton, backOrigScale, 0.3f));

        ResetToOriginals();
    }

    private IEnumerator PopScaleRoutine(RectTransform rect, Vector3 targetScale, float duration)
    {
        if (rect == null) yield break;
        if (targetScale.sqrMagnitude < 0.1f) targetScale = Vector3.one;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float ease = EaseOutBack(t);
            rect.localScale = Vector3.Lerp(Vector3.zero, targetScale, ease);
            yield return null;
        }
        rect.localScale = targetScale;
    }

    public void OnBackButtonClicked()
    {
        Debug.Log($"🔙 [MainMapUIAnimation] Back clicked! Navigating to: {targetSceneName}");
        Time.timeScale = 1f;
        SceneChuyenCanhManager.LoadScene(targetSceneName);
    }

    private IEnumerator LoadSceneRoutine()
    {
        yield return new WaitForSecondsRealtime(loadDelay);

        if (Application.CanStreamedLevelBeLoaded(targetSceneName))
        {
            SceneChuyenCanhManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError($"❌ Scene '{targetSceneName}' chưa có trong Build Settings!");
        }
    }

    private float EaseOutBack(float x)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}
