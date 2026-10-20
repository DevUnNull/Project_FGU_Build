using UnityEngine;

public class HeartPulseAnimation : MonoBehaviour
{
    [Header("Pulse Settings")]
    [Tooltip("Tốc độ nhịp đập")]
    public float pulseSpeed = 2.5f;

    [Tooltip("Độ phập phồng scale (0.12 = phập phồng 12%)")]
    public float pulseAmount = 0.12f;

    [Tooltip("Sử dụng nhịp đập tim thực tế (nhịp kép: thình thịch)")]
    public bool useHeartbeatPattern = true;

    private Vector3 originalScale;
    private RectTransform rectTransform;

    // 🎯 Tự động tìm và gắn vào icon 'Icone' trong Scene ngay khi Game chạy
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoHookToHeartIcon()
    {
        GameObject iconObj = GameObject.Find("Icone");
        if (iconObj == null)
        {
            GameObject healObj = GameObject.Find("Heal");
            if (healObj != null)
            {
                Transform iconT = healObj.transform.Find("Icone");
                if (iconT != null) iconObj = iconT.gameObject;
            }
        }

        if (iconObj != null && iconObj.GetComponent<HeartPulseAnimation>() == null)
        {
            iconObj.AddComponent<HeartPulseAnimation>();
            Debug.Log("💖 Tự động kích hoạt Animation nhịp tim phập phồng cho " + iconObj.name);
        }
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            originalScale = rectTransform.localScale;
        }
        else
        {
            originalScale = transform.localScale;
        }

        if (originalScale == Vector3.zero)
        {
            originalScale = Vector3.one;
        }
    }

    private void Update()
    {
        if (originalScale == Vector3.zero) originalScale = Vector3.one;

        float scaleFactor = 1f;

        if (useHeartbeatPattern)
        {
            // Nhịp đập tim kép tự nhiên (Lắp nhịp 'thình... thịch...')
            float time = Time.time * pulseSpeed;
            float cycle = time % (2f * Mathf.PI);

            if (cycle < Mathf.PI * 0.4f)
            {
                // Nhịp 1 (Thình)
                scaleFactor += Mathf.Sin(cycle / 0.4f * Mathf.PI) * pulseAmount;
            }
            else if (cycle >= Mathf.PI * 0.5f && cycle < Mathf.PI * 0.9f)
            {
                // Nhịp 2 (Thịch - nhẹ hơn)
                float t2 = (cycle - Mathf.PI * 0.5f) / 0.4f;
                scaleFactor += Mathf.Sin(t2 * Mathf.PI) * (pulseAmount * 0.65f);
            }
        }
        else
        {
            // Nhấp nhô hình sin mượt mà liên tục
            float sineWave = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f; // 0..1
            scaleFactor += sineWave * pulseAmount;
        }

        Vector3 targetScale = originalScale * scaleFactor;

        if (rectTransform != null)
        {
            rectTransform.localScale = targetScale;
        }
        else
        {
            transform.localScale = targetScale;
        }
    }
}
