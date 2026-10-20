using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RollButtonAnimation : MonoBehaviour
{
    [Header("Target Arrow Transform")]
    public RectTransform arrowTransform; // Mũi tên (muiten)
    
    [Header("Animation Settings")]
    public float spinDuration = 0.45f;    // Thời gian quay 1 vòng
    public float totalRotation = 360f;   // 360 độ
    public float wobbleAngle = 18f;      // Góc lắc lư khi dừng

    private Coroutine spinRoutine;
    private Vector3 originalScale = Vector3.one;

    private void Awake()
    {
        AutoFindArrow();
    }

    private void Start()
    {
        AutoFindArrow();

        // Tự động lắng nghe sự kiện Click nếu component này nằm trên nút Button
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(PlaySpinAnimation);
        }
    }

    private void AutoFindArrow()
    {
        if (arrowTransform == null)
        {
            Transform found = transform.Find("muiten");
            if (found != null)
            {
                arrowTransform = found.GetComponent<RectTransform>();
            }
        }

        if (arrowTransform != null && originalScale == Vector3.one)
        {
            originalScale = arrowTransform.localScale;
        }
    }

    public void PlaySpinAnimation()
    {
        AutoFindArrow();
        if (arrowTransform == null) return;

        if (spinRoutine != null) StopCoroutine(spinRoutine);
        spinRoutine = StartCoroutine(SpinRoutine());
    }

    private IEnumerator SpinRoutine()
    {
        float elapsed = 0f;

        // Phase 1: Quay mượt 360 độ kèm hiệu ứng nảy Pop Scale
        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / spinDuration);

            // Easing curve cho xoay: EaseOutCubic
            float easeRot = EaseOutCubic(t);
            float currentAngle = -easeRot * totalRotation; // Quay theo chiều kim đồng hồ

            arrowTransform.localRotation = Quaternion.Euler(0, 0, currentAngle);

            // Hiệu ứng nảy scale (Pop effect khi quay)
            float scalePop = 1f + Mathf.Sin(t * Mathf.PI) * 0.22f;
            arrowTransform.localScale = originalScale * scalePop;

            yield return null;
        }

        // Phase 2: Hiệu ứng lắc lư đàn hồi (Wobble & Elastic Settle)
        float wobbleTime = 0.3f;
        elapsed = 0f;
        while (elapsed < wobbleTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / wobbleTime;

            // Lắc lư qua lại tắt dần (Elastic Decay)
            float wobble = Mathf.Sin(t * Mathf.PI * 4f) * (1f - t) * wobbleAngle;
            arrowTransform.localRotation = Quaternion.Euler(0, 0, wobble);
            arrowTransform.localScale = Vector3.Lerp(arrowTransform.localScale, originalScale, t * 2f);

            yield return null;
        }

        // Đảm bảo về trạng thái chuẩn
        arrowTransform.localRotation = Quaternion.identity;
        arrowTransform.localScale = originalScale;
    }

    private float EaseOutCubic(float x)
    {
        return 1f - Mathf.Pow(1f - x, 3f);
    }
}
