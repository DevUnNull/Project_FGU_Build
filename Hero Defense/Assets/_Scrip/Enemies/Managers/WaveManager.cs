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
    public System.Action OnAllWavesCompleted; // Event khi tất cả wave đã hoàn thành (thắng)

    [Header("Delay giữa các wave (giây)")]
    [SerializeField] private float delayBetweenWaves = 10f;

    [Header("UI Buttons Skip (mỗi Tune 1 nút)")]
    [SerializeField] private List<Button> skipButtons; // gán theo thứ tự Tune
    [SerializeField] private float showButtonDelay = 5f;

    [Header("UI Countdown (mỗi Tune 1 text)")]
    [SerializeField] private List<TextMeshProUGUI> countdownTexts; // gán trong Inspector theo Tune

    private Dictionary<int, bool> skipDelays = new();
    private Dictionary<int, Coroutine> tuneCoroutines = new();

    // Tracking cho việc phát hiện enemy cuối cùng của wave cuối cùng
    public int currentTuneIndex { get; private set; } = -1;
    public int currentWaveIndex { get; private set; } = -1;
    public bool IsLastWaveOfLastTune { get; private set; } = false;
    private int activeEnemiesInCurrentWave = 0;
    private bool isLastWaveSpawningCompleted = false; // Đánh dấu wave cuối đã spawn xong chưa


    // default game 
    [SerializeField] public PauseMenu pauseMenu;

    void Awake()
    {
        pauseMenu = GetComponent<PauseMenu>();
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

            // Cập nhật tracking
            currentTuneIndex = tuneIdx;
            currentWaveIndex = waveIdx;

            // Kiểm tra nếu đây là wave cuối cùng của tune cuối cùng
            bool isLastTune = (tuneIdx == tunes.Count - 1);
            bool isLastWave = (waveIdx == tune.waves.Count - 1);
            IsLastWaveOfLastTune = isLastTune && isLastWave;

            // Reset counter khi bắt đầu wave mới
            // NHƯNG nếu là wave cuối, KHÔNG reset để giữ lại enemy từ wave trước
            if (!IsLastWaveOfLastTune)
            {
                activeEnemiesInCurrentWave = 0;
            }
            else
            {
                Debug.Log($"⚠️ Wave cuối bắt đầu - KHÔNG reset counter. Enemy hiện tại: {activeEnemiesInCurrentWave}");
            }

            isLastWaveSpawningCompleted = false; // Reset flag

            Debug.Log($"🎵 [Tune {tuneIdx}] Bắt đầu Wave {waveIdx + 1}");
            OnWaveStart?.Invoke(wave);

            skipDelays[tuneIdx] = false;

            //bool isLastWave = (waveIdx == tune.waves.Count - 1);
            if (isLastWave)
            {
                if (countdownText != null)
                    countdownText.gameObject.SetActive(false);

                Debug.Log($"🕹 [Tune {tuneIdx}] Wave cuối — không hiện countdown hoặc nút skip.");

                // Đợi wave spawn xong (tính toán thời gian spawn)
                float totalSpawnTime = CalculateTotalSpawnTime(wave);
                yield return new WaitForSeconds(totalSpawnTime);

                // Đánh dấu wave đã spawn xong
                isLastWaveSpawningCompleted = true;
                Debug.Log($"✅ Wave cuối đã spawn xong! Đang đợi enemy bị tiêu diệt hết...");

                // Kiểm tra lại xem có còn enemy nào không
                CheckForVictory();

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

    // Đếm enemy được spawn
    public void OnEnemySpawned()
    {
        activeEnemiesInCurrentWave++;
    }

    // Kiểm tra khi enemy bị ẩn/return về pool
    public void OnEnemyReturned()
    {
        if (activeEnemiesInCurrentWave > 0)
        {
            activeEnemiesInCurrentWave--;
            Debug.Log($"📊 Enemy returned. Còn lại: {activeEnemiesInCurrentWave} enemies. IsLastWaveOfLastTune: {IsLastWaveOfLastTune}, SpawnCompleted: {isLastWaveSpawningCompleted}");

            // Kiểm tra thắng chỉ khi: wave cuối đã spawn xong VÀ không còn enemy nào
            if (IsLastWaveOfLastTune && isLastWaveSpawningCompleted)
            {
                CheckForVictory();
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ OnEnemyReturned được gọi nhưng activeEnemiesInCurrentWave <= 0! Giá trị: {activeEnemiesInCurrentWave}");
        }
    }

    /// <summary>
    /// Kiểm tra điều kiện thắng và phát event nếu đủ điều kiện
    /// </summary>
    private void CheckForVictory()
    {
        if (IsLastWaveOfLastTune && isLastWaveSpawningCompleted && activeEnemiesInCurrentWave == 0)
        {
            Debug.Log("🎉 THÔNG BÁO: Enemy cuối cùng của wave cuối cùng đã được ẩn đi!");

            // Delay một chút để đảm bảo tất cả logic đã hoàn thành
            StartCoroutine(DelayedInvokeVictory());
        }
    }

    /// <summary>
    /// Tính tổng thời gian spawn của một wave
    /// </summary>
    private float CalculateTotalSpawnTime(WaveConfig wave)
    {
        float totalTime = 0f;
        foreach (var group in wave.groups)
        {
            // Thời gian spawn = (số lượng - 1) * interval (vì enemy đầu spawn ngay)
            float groupTime = (group.count > 0) ? (group.count - 1) * group.interval : 0f;
            totalTime += groupTime;
        }
        return totalTime;
    }

    /// <summary>
    /// Delay một frame trước khi phát event thắng
    /// </summary>
    private System.Collections.IEnumerator DelayedInvokeVictory()
    {
        yield return null; // Chờ 1 frame
        Debug.Log("🚀 Phát event OnAllWavesCompleted!");
        OnAllWavesCompleted?.Invoke();
    }
}
