using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [SerializeField] private List<TuneConfig> tunes;  // Danh sách các Tune
    private Dictionary<int, int> tuneWaveProgress = new();  // Ghi nhớ wave hiện tại của từng tune

    public System.Action<WaveConfig> OnWaveStart;  // Sự kiện gửi tới các spawner

    [Header("Delay giữa các wave (giây)")]
    [SerializeField] private float delayBetweenWaves = 3f; // tuỳ chỉnh trong Inspector

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    // Gọi từ LogicSpawner
    public void StartTune(int tuneIdx)
    {
        if (tuneIdx < 0 || tuneIdx >= tunes.Count)
        {
            Debug.LogWarning($"⚠️ Tune index {tuneIdx} không tồn tại! Số lượng tune: {tunes.Count}");
            return;
        }

        if (!tuneWaveProgress.ContainsKey(tuneIdx))
            tuneWaveProgress[tuneIdx] = 0;

        StartCoroutine(RunTuneRoutine(tuneIdx));
    }

    private IEnumerator RunTuneRoutine(int tuneIdx)
    {
        var tune = tunes[tuneIdx];

        for (int waveIdx = 0; waveIdx < tune.waves.Count; waveIdx++)
        {
            var wave = tune.waves[waveIdx];

            Debug.Log($"🎵 Bắt đầu Wave {waveIdx + 1} trong Tune {tuneIdx}");

            // Phát sự kiện cho các WaveSpawner
            OnWaveStart?.Invoke(wave);

            // Chờ hết thời gian delay trước khi chạy wave kế tiếp
            yield return new WaitForSeconds(delayBetweenWaves);
        }

        Debug.Log($"✅ Tune {tuneIdx} đã hoàn thành!");
    }
}
