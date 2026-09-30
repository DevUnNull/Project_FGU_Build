using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachineReelController : MonoBehaviour
{
    [Header("Jackpot Reel Timing")]
    public float baseSpinDuration = 0.35f; // Thời gian quay của ô 1 (giây)
    public float staggerDelay = 0.20f;      // Độ trễ nối tiếp giữa 4 ô (Trái -> Phải)
    
    [Header("Reel Motion & Motion Blur")]
    public float reelSpinSpeed = 45f;      // Tốc độ cuộn cuồn cuộn Jackpot
    public float verticalStretch = 1.35f;   // Motion Blur co giãn Y
    public float overshootDistance = 0.40f; // Độ nảy quá đà (Overshoot) khi khựng lại

    [Header("Audio & FX")]
    public AudioClip lockSoundClip;          // Tiếng 'CLACK' chốt hạ
    [Range(0f, 1f)] public float soundVolume = 0.90f;

    private AudioSource audioSource;
    private bool isRolling = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        // Tự động load âm thanh 'CLACK' từ dự án nếu chưa gán
        if (lockSoundClip == null)
        {
            lockSoundClip = Resources.Load<AudioClip>("Sounds/Button - High");
        }
    }

    public bool IsRolling => isRolling;

    /// <summary>
    /// Kích hoạt hiệu ứng quay Reel Slot Machine Jackpot cho 4 ô card
    /// </summary>
    public void StartReelRoll(Transform[] slots, System.Action<int> onSpawnSlotUnit)
    {
        if (slots == null || slots.Length == 0) return;
        StartCoroutine(ReelRollRoutine(slots, onSpawnSlotUnit));
    }

    private IEnumerator ReelRollRoutine(Transform[] slots, System.Action<int> onSpawnSlotUnit)
    {
        isRolling = true;

        for (int i = 0; i < slots.Length; i++)
        {
            float stopDelay = baseSpinDuration + (i * staggerDelay);
            int slotIndex = i;
            StartCoroutine(SpinSingleSlotRoutine(slots[slotIndex], stopDelay, () =>
            {
                // Gọi callback sinh tướng khi ô này khựng dừng lại
                onSpawnSlotUnit?.Invoke(slotIndex);
            }));
        }

        // Chờ ô cuối cùng dừng hoàn toàn
        float totalTime = baseSpinDuration + (slots.Length * staggerDelay) + 0.45f;
        yield return new WaitForSeconds(totalTime);

        isRolling = false;
    }

    private IEnumerator SpinSingleSlotRoutine(Transform slotTransform, float spinDuration, System.Action onSlotLock)
    {
        if (slotTransform == null) yield break;

        Vector3 originalPos = slotTransform.localPosition;
        Vector3 originalScale = slotTransform.localScale;

        // --- PHA 1: WIND-UP / ANTICIPATION (Nảy giật nhích nhẹ lên trên trước khi cuộn xuống) ---
        float windUpDuration = 0.08f;
        float elapsed = 0f;
        while (elapsed < windUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / windUpDuration;
            slotTransform.localPosition = originalPos + new Vector3(0, t * 0.18f, 0);
            yield return null;
        }

        // --- PHA 2: QUAY JACKPOT SIÊU TỐC + MOTION BLUR CỦA SLOT MACHINE ---
        elapsed = 0f;
        float mainSpinTime = spinDuration - windUpDuration;

        while (elapsed < mainSpinTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / mainSpinTime;

            // Tính toán chuyển động cuộn cuồn cuộn theo nhịp Slot Machine
            float speedMult = (t > 0.75f) ? (1f - (t - 0.75f) * 3f) : 1f; // Chậm dần về cuối (Deceleration)
            float yOffset = (Mathf.Repeat(Time.time * reelSpinSpeed * speedMult, 1.2f) - 0.6f);
            
            slotTransform.localPosition = originalPos + new Vector3(0, -yOffset, 0);

            // Motion Blur: Co giãn chiều dọc Y và hẹp lại chiều ngang X
            float stretchFactor = Mathf.Lerp(verticalStretch, 1f, t * t);
            slotTransform.localScale = new Vector3(originalScale.x * (2f - stretchFactor), originalScale.y * stretchFactor, originalScale.z);

            yield return null;
        }

        // --- PHA 3: KHỰNG LẠI CHỐT KẾT QUẢ & SINH TƯỚNG ---
        slotTransform.localPosition = originalPos;
        slotTransform.localScale = originalScale;

        // Sinh tướng và chốt giá cho ô này
        onSlotLock?.Invoke();

        // Lấy tướng vừa được sinh ra trong ô này để làm hiệu ứng Overshoot Bounce
        Transform spawnedUnit = null;
        if (slotTransform.childCount > 0)
        {
            spawnedUnit = slotTransform.GetChild(slotTransform.childCount - 1);
        }

        // --- PHA 4: HIỆU ỨNG OVERSHOOT / ELASTIC BOUNCE (Trôi tụt quá đà rồi nảy sật về) ---
        Transform targetTransform = spawnedUnit != null ? spawnedUnit : slotTransform;
        Vector3 initialTargetPos = targetTransform.localPosition;
        Vector3 initialTargetScale = targetTransform.localScale;

        // Phát âm thanh 'CLACK' chốt kết quả
        PlayLockSound();

        // Lóe sáng viền vàng (Golden Border Flash FX)
        CreateGoldFlashEffect(slotTransform);

        float bounceDuration = 0.28f;
        elapsed = 0f;

        while (elapsed < bounceDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / bounceDuration;

            // Nảy giật đàn hồi Jackpot (Rơi tụt xuống quá đà rồi bật nảy lại vị trí chuẩn)
            float bounceY = -Mathf.Sin(t * Mathf.PI) * (1f - t) * overshootDistance;
            targetTransform.localPosition = initialTargetPos + new Vector3(0, bounceY, 0);
            
            // Pop scale nhẹ theo chuẩn scale của tướng
            float scalePop = 1f + Mathf.Sin(t * Mathf.PI) * 0.15f;
            targetTransform.localScale = initialTargetScale * scalePop;

            yield return null;
        }

        targetTransform.localPosition = initialTargetPos;
        targetTransform.localScale = initialTargetScale;
    }

    private void PlayLockSound()
    {
        if (lockSoundClip == null)
        {
            lockSoundClip = Resources.Load<AudioClip>("Sounds/Button - High");
        }

        if (audioSource != null && lockSoundClip != null)
        {
            audioSource.PlayOneShot(lockSoundClip, soundVolume);
        }
    }

    private void CreateGoldFlashEffect(Transform slotTransform)
    {
        if (slotTransform == null) return;

        GameObject flashObj = new GameObject("GoldFlashFX");
        flashObj.transform.SetParent(slotTransform, false);
        
        RectTransform rt = flashObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;

        Image img = flashObj.AddComponent<Image>();
        img.color = new Color(1f, 0.88f, 0.25f, 0.75f); // Lóe ánh kim vàng rực rỡ
        img.raycastTarget = false;

        StartCoroutine(FadeAndDestroyFlash(img, flashObj));
    }

    private IEnumerator FadeAndDestroyFlash(Image img, GameObject flashObj)
    {
        float fadeDuration = 0.3f;
        float elapsed = 0f;
        Color startColor = img.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            img.color = Color.Lerp(startColor, new Color(1f, 0.88f, 0.25f, 0f), t);
            yield return null;
        }

        Destroy(flashObj);
    }
}
