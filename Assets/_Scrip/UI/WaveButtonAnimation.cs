using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Script tạo hiệu ứng Animation UI mượt mà cho các nút (StartGame, NextWave1..5).
/// Bao gồm:
/// 1. Hiệu ứng thở / dập dềnh / lắc nhẹ liên tục (Idle Breathing & Float).
/// 2. Hiệu ứng khi ấn nút (Press Down co nhỏ lại -> Click Pop Punch nảy lên cực kỳ sướng tay).
/// </summary>
public class WaveButtonAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("Idle Animation Settings (Hiệu ứng khi chờ)")]
    public bool enableIdleAnimation = true;
    [Tooltip("Tốc độ thở co giãn")]
    public float pulseSpeed = 2.5f;
    [Tooltip("Biên độ thở co giãn")]
    public float pulseAmount = 0.04f;

    [Tooltip("Tốc độ nổi nhấp nhô")]
    public float floatSpeed = 2.0f;
    [Tooltip("Biên độ nổi nhấp nhô")]
    public float floatAmount = 4.0f;

    [Tooltip("Góc lắc nhẹ ngẫu nhiên")]
    public float wobbleAngle = 2.5f;
    public float wobbleSpeed = 1.8f;

    [Header("Click Animation Settings (Hiệu ứng khi ấn)")]
    [Tooltip("Tỉ lệ thu nhỏ khi đè chuột/tay xuống (Press Down)")]
    public float pressScaleMultiplier = 0.88f;

    [Tooltip("Tỉ lệ nảy bung ra khi thả chuột/click (Punch Pop)")]
    public float punchScaleMultiplier = 1.20f;

    [Tooltip("Thời gian nảy đàn hồi")]
    public float punchDuration = 0.28f;

    private Vector3 originalScale = Vector3.one;
    private Vector2 originalAnchoredPosition;
    private Quaternion originalRotation = Quaternion.identity;

    private RectTransform rectTransform;
    private Button button;

    private Coroutine punchRoutine;
    private bool isPressed = false;
    private bool isPunching = false;
    private float seedOffset;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        seedOffset = Random.Range(0f, 100f);

        CacheOriginalTransforms();
    }

    private void OnEnable()
    {
        CacheOriginalTransforms();
        isPressed = false;
        isPunching = false;

        if (rectTransform != null)
        {
            rectTransform.localScale = originalScale;
            rectTransform.localRotation = originalRotation;
        }
    }

    private void CacheOriginalTransforms()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            if (rectTransform.localScale.sqrMagnitude > 0.1f)
            {
                originalScale = rectTransform.localScale;
            }
            else if (originalScale.sqrMagnitude < 0.1f)
            {
                originalScale = Vector3.one;
            }

            if (originalAnchoredPosition == Vector2.zero)
                originalAnchoredPosition = rectTransform.anchoredPosition;

            originalRotation = rectTransform.localRotation;
        }
    }

    private void Update()
    {
        if (!enableIdleAnimation || isPressed || isPunching || rectTransform == null) return;

        float time = Time.unscaledTime + seedOffset;

        // 1. Thở co giãn nhẹ (Pulse scale)
        float scaleDamp = Mathf.Sin(time * pulseSpeed) * pulseAmount;
        rectTransform.localScale = originalScale * (1f + scaleDamp);

        // 2. Nhấp nhô nổi nhẹ lên xuống (Float position)
        float floatOffsetY = Mathf.Sin(time * floatSpeed) * floatAmount;
        rectTransform.anchoredPosition = originalAnchoredPosition + new Vector2(0f, floatOffsetY);

        // 3. Lắc nhẹ góc ngẫu nhiên (Wobble rotation)
        float rotZ = Mathf.Sin(time * wobbleSpeed) * wobbleAngle;
        rectTransform.localRotation = originalRotation * Quaternion.Euler(0f, 0f, rotZ);
    }

    /// <summary>
    /// Xử lý khi bắt đầu nhấn xuống (Press Down)
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;

        isPressed = true;
        if (punchRoutine != null) StopCoroutine(punchRoutine);

        // Co nhỏ lại khi đè xuống
        StartCoroutine(AnimateScaleRoutine(originalScale * pressScaleMultiplier, 0.08f));
    }

    /// <summary>
    /// Xử lý khi nhả tay ra (Pointer Up)
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }

    /// <summary>
    /// Xử lý khi Click hoàn tất -> Phát hiệu ứng nảy Pop Elastic
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;

        PlayClickAnimation();
    }

    /// <summary>
    /// Gọi kích hoạt hiệu ứng nảy nút khi Click (Có thể gọi từ script bên ngoài)
    /// </summary>
    public void PlayClickAnimation()
    {
        CacheOriginalTransforms();
        if (punchRoutine != null) StopCoroutine(punchRoutine);
        punchRoutine = StartCoroutine(PunchClickRoutine());
    }

    private IEnumerator PunchClickRoutine()
    {
        isPunching = true;

        if (rectTransform == null) yield break;

        float elapsed = 0f;
        float halfDuration = punchDuration * 0.4f;

        // Phase 1: Nảy bung to ra (Punch Pop Up)
        Vector3 targetPopScale = originalScale * punchScaleMultiplier;
        float popRotZ = Random.Range(-8f, 8f);

        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / halfDuration;
            float easeT = EaseOutBack(t);

            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetPopScale, easeT);
            rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, popRotZ, t));
            yield return null;
        }

        // Phase 2: Lắc nảy đàn hồi trở về kích thước chuẩn (Elastic Settle)
        float remainDuration = punchDuration - halfDuration;
        elapsed = 0f;

        while (elapsed < remainDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / remainDuration;

            // Xoay nảy qua lại tắt dần
            float wobble = Mathf.Sin(t * Mathf.PI * 4f) * (1f - t) * popRotZ;
            rectTransform.localRotation = originalRotation * Quaternion.Euler(0, 0, wobble);

            // Rescale về gốc
            rectTransform.localScale = Vector3.Lerp(targetPopScale, originalScale, EaseOutCubic(t));
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, originalAnchoredPosition, t);

            yield return null;
        }

        // Đảm bảo trả về đúng chuẩn
        rectTransform.localScale = originalScale;
        rectTransform.anchoredPosition = originalAnchoredPosition;
        rectTransform.localRotation = originalRotation;

        isPunching = false;
    }

    private IEnumerator AnimateScaleRoutine(Vector3 targetScale, float duration)
    {
        float elapsed = 0f;
        Vector3 startScale = rectTransform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        rectTransform.localScale = targetScale;
    }

    private float EaseOutBack(float x)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }

    private float EaseOutCubic(float x)
    {
        return 1f - Mathf.Pow(1f - x, 3f);
    }
}
