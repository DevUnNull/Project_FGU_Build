using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // nếu bạn dùng TextMeshPro

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [SerializeField] private List<TuneConfig> tunes;
    private Dictionary<int, int> tuneWaveProgress = new();

    public System.Action<WaveConfig> OnWaveStart;

    [Header("Delay giữa các wave (giây)")]
    [SerializeField] private float delayBetweenWaves = 10f;

    [Header("UI Buttons Skip (mỗi Tune 1 nút)")]
    [SerializeField] private List<Button> skipButtons; // gán theo thứ tự Tune
    [SerializeField] private float showButtonDelay = 5f;

    [Header("UI Countdown (mỗi Tune 1 text)")]
    [SerializeField] private List<TextMeshProUGUI> countdownTexts; // gán trong Inspector theo Tune

    private Dictionary<int, bool> skipDelays = new();
    private Dictionary<int, Coroutine> tuneCoroutines = new();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Ẩn tất cả button + text ban đầu
        foreach (var btn in skipButtons)
            if (btn != null) btn.gameObject.SetActive(false);

        foreach (var txt in countdownTexts)
            if (txt != null) txt.gameObject.SetActive(false);
    }

    public void StartTune(int tuneIdx)
    {
        if (tuneIdx < 0 || tuneIdx >= tunes.Count)
        {
            Debug.LogWarning($"⚠️ Tune index {tuneIdx} không tồn tại! Số lượng tune: {tunes.Count}");
            return;
        }

        if (tuneCoroutines.ContainsKey(tuneIdx))
            StopCoroutine(tuneCoroutines[tuneIdx]);

        skipDelays[tuneIdx] = false;

        tuneCoroutines[tuneIdx] = StartCoroutine(RunTuneRoutine(tuneIdx));
    }

    private IEnumerator RunTuneRoutine(int tuneIdx)
    {
        var tune = tunes[tuneIdx];
        var skipButton = (tuneIdx < skipButtons.Count) ? skipButtons[tuneIdx] : null;
        var countdownText = (tuneIdx < countdownTexts.Count) ? countdownTexts[tuneIdx] : null;

        for (int waveIdx = 0; waveIdx < tune.waves.Count; waveIdx++)
        {
            var wave = tune.waves[waveIdx];
            Debug.Log($"🎵 [Tune {tuneIdx}] Bắt đầu Wave {waveIdx + 1}");
            OnWaveStart?.Invoke(wave);

            skipDelays[tuneIdx] = false;

            bool isLastWave = (waveIdx == tune.waves.Count - 1);
            if (isLastWave)
            {
                if (countdownText != null)
                    countdownText.gameObject.SetActive(false);

                Debug.Log($"🕹 [Tune {tuneIdx}] Wave cuối — không hiện countdown hoặc nút skip.");
                yield return new WaitForSeconds(delayBetweenWaves);
                continue;
            }

            if (skipButton != null)
                skipButton.gameObject.SetActive(false);

            if (countdownText != null)
                countdownText.gameObject.SetActive(true);

            float timer = 0f;
            float totalDelay = delayBetweenWaves;
            float remaining = totalDelay;

            // Đếm ngược
            while (timer < totalDelay && !skipDelays[tuneIdx])
            {
                timer += Time.deltaTime;
                remaining = Mathf.Max(0, totalDelay - timer);

                // Cập nhật text
                if (countdownText != null)
                    countdownText.text = $"{remaining:F1}s";

                // Sau 5s đầu thì hiện nút skip
                if (timer >= showButtonDelay && skipButton != null && !skipButton.gameObject.activeSelf)
                    skipButton.gameObject.SetActive(true);

                yield return null;
            }

            // Ẩn text và nút khi xong
            if (countdownText != null)
                countdownText.gameObject.SetActive(false);

            if (skipButton != null)
                skipButton.gameObject.SetActive(false);
        }

        Debug.Log($"✅ [Tune {tuneIdx}] Hoàn thành toàn bộ!");
    }

    // Gọi từ UI button riêng của mỗi Tune
    public void SkipToNextWave(int tuneIdx)
    {
        if (!skipDelays.ContainsKey(tuneIdx))
            skipDelays[tuneIdx] = false;

        skipDelays[tuneIdx] = true;

        if (tuneIdx < skipButtons.Count && skipButtons[tuneIdx] != null)
            skipButtons[tuneIdx].gameObject.SetActive(false);

        if (tuneIdx < countdownTexts.Count && countdownTexts[tuneIdx] != null)
            countdownTexts[tuneIdx].gameObject.SetActive(false);

        Debug.Log($"⏩ [Tune {tuneIdx}] Skip: Sang wave tiếp theo ngay!");
    }
}
