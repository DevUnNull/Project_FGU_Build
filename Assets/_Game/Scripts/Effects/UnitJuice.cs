using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Hiệu ứng "juice" dùng chung cho Tế bào & Quái (tạo bằng code, không cần Animator):
/// - Flash: nháy màu khi trúng đòn
/// - Punch: co giãn nảy (squash & stretch) khi đặt xuống / bắn / trúng đòn
/// - FadeOut: mờ dần + thu nhỏ khi chết
/// Giữ nguyên dấu localScale.x (hướng lật trái/phải do script khác điều khiển).
/// </summary>
public class UnitJuice : MonoBehaviour
{
    private SpriteRenderer[] renderers;
    private Color[] originalColors;
    private bool isFlashing;
    private Coroutine flashRoutine;

    // Punch: hệ số co giãn được áp vào localScale trong LateUpdate
    private Vector3 punchBaseScale;
    private Vector2 punchAmplitude;
    private float punchDuration;
    private float punchTimer = -1f;

    private bool isFading;
    private Vector3 fadeStartScale;

    public static UnitJuice GetOrAdd(GameObject go)
    {
        UnitJuice juice = go.GetComponent<UnitJuice>();
        return juice != null ? juice : go.AddComponent<UnitJuice>();
    }

    private void CacheRenderers()
    {
        if (renderers != null) return;
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalColors = new Color[renderers.Length];
    }

    // ======================= FLASH =======================

    public void Flash(Color tint, float duration = 0.12f)
    {
        if (!isActiveAndEnabled || isFading) return;
        CacheRenderers();

        // Chỉ lưu màu gốc khi chưa đang nháy, tránh lưu nhầm màu đang tint
        if (!isFlashing)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null) originalColors[i] = renderers[i].color;
            }
        }

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine(tint, duration));
    }

    private IEnumerator FlashRoutine(Color tint, float duration)
    {
        isFlashing = true;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float k = 1f - t / duration;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null) renderers[i].color = Color.Lerp(originalColors[i], originalColors[i] * tint, k);
            }
            yield return null;
        }
        RestoreColors();
        flashRoutine = null;
    }

    private void RestoreColors()
    {
        if (renderers == null) return;
        if (isFlashing || isFading)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null) renderers[i].color = originalColors[i];
            }
        }
        isFlashing = false;
    }

    // ======================= PUNCH =======================

    /// <summary>
    /// Co giãn nảy dần tắt. amplitude.x / amplitude.y là độ lệch ban đầu (VD: (0.2, -0.2) = bè ra & lùn xuống).
    /// </summary>
    public void Punch(Vector2 amplitude, float duration = 0.25f)
    {
        if (!isActiveAndEnabled || isFading) return;

        // Đang punch dở -> giữ scale gốc cũ, tránh tích lũy sai lệch
        if (punchTimer < 0f) punchBaseScale = AbsScale(transform.localScale);
        punchAmplitude = amplitude;
        punchDuration = Mathf.Max(0.01f, duration);
        punchTimer = 0f;
    }

    /// <summary>
    /// Đổi kích thước gốc (VD: tế bào lên cấp to hơn). Đang nảy thì cập nhật gốc để khi nảy xong
    /// không trả về kích thước cũ.
    /// </summary>
    public void SetBaseScale(Vector3 absScale)
    {
        if (punchTimer >= 0f) punchBaseScale = absScale;
        else ApplyScale(absScale);
    }

    private void LateUpdate()
    {
        if (punchTimer < 0f) return;

        punchTimer += Time.deltaTime;
        float t = Mathf.Clamp01(punchTimer / punchDuration);

        // Dao động tắt dần: lệch -> nảy ngược -> về 1
        float wave = (1f - t) * Mathf.Cos(t * Mathf.PI * 2.5f);
        Vector2 multiplier = Vector2.one + punchAmplitude * wave;
        ApplyScale(new Vector3(punchBaseScale.x * multiplier.x, punchBaseScale.y * multiplier.y, punchBaseScale.z));

        if (t >= 1f)
        {
            ApplyScale(punchBaseScale);
            punchTimer = -1f;
        }
    }

    private void ApplyScale(Vector3 absScale)
    {
        float signX = transform.localScale.x < 0f ? -1f : 1f;
        transform.localScale = new Vector3(absScale.x * signX, absScale.y, absScale.z);
    }

    private static Vector3 AbsScale(Vector3 s) => new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), s.z);

    // ======================= FADE OUT =======================

    /// <summary>
    /// Mờ dần + thu nhỏ rồi gọi onComplete. Màu & scale được khôi phục trước khi gọi onComplete
    /// (để object trả về Pool có thể tái sử dụng bình thường).
    /// </summary>
    public void FadeOut(float duration, Action onComplete)
    {
        if (isFading) return;
        CacheRenderers();

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = null;
        if (!isFlashing)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null) originalColors[i] = renderers[i].color;
            }
        }
        isFlashing = false;

        if (punchTimer >= 0f) ApplyScale(punchBaseScale);
        punchTimer = -1f;

        StartCoroutine(FadeRoutine(duration, onComplete));
    }

    private IEnumerator FadeRoutine(float duration, Action onComplete)
    {
        isFading = true;
        fadeStartScale = AbsScale(transform.localScale);
        Vector3 startScale = fadeStartScale;

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float k = t / duration;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;
                Color c = originalColors[i];
                c.a *= 1f - k;
                renderers[i].color = c;
            }
            ApplyScale(startScale * Mathf.Lerp(1f, 0.7f, k));
            yield return null;
        }

        RestoreColors();
        ApplyScale(startScale);
        isFading = false;
        onComplete?.Invoke();
    }

    private void OnDisable()
    {
        // Bị tắt giữa chừng (trả Pool, đổi scene...) -> khôi phục trạng thái gốc
        if (flashRoutine != null) flashRoutine = null;
        RestoreColors();
        if (punchTimer >= 0f) ApplyScale(punchBaseScale);
        if (isFading) ApplyScale(fadeStartScale);
        punchTimer = -1f;
        isFading = false;
    }
}
