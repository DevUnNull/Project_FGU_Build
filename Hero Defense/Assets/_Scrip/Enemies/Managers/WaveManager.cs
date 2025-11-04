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
    private Dictionary<int, float> tuneRemainingCountdown = new();
    private Dictionary<int, bool> tuneCompleted = new(); // Track tune nào đã hoàn thành
    private Dictionary<int, int> tuneCurrentWave = new(); // Track wave hiện tại của mỗi tune

    // Tracking cho việc phát hiện enemy cuối cùng của wave cuối cùng
    public int currentTuneIndex { get; private set; } = -1;
    public int currentWaveIndex { get; private set; } = -1;
    public bool IsLastWaveOfLastTune { get; private set; } = false;
    private int activeEnemiesInCurrentWave = 0;
    private bool isLastWaveSpawningCompleted = false; // Đánh dấu wave cuối đã spawn xong chưa
    private bool victoryInvoked = false; // Flag để tránh trigger nhiều lần
    private int expectedEnemiesInLastWave = 0; // Số enemy dự kiến trong wave cuối

    [Header("Early Spawn Reward")]
    [Tooltip("Hệ số thưởng vàng khi người chơi ấn ra quái sớm (vàng = ceil(giây còn lại * hệ số))")]
    [SerializeField] private float earlySpawnRewardMultiplier = 1f;


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

            // Cập nhật tracking wave hiện tại của tune này
            tuneCurrentWave[tuneIdx] = waveIdx;
            tuneCompleted[tuneIdx] = false; // Reset completed flag
            
            // Kiểm tra nếu đây là wave cuối của tune hiện tại
            bool isLastWave = (waveIdx == tune.waves.Count - 1);
            
            // Kiểm tra xem TẤT CẢ tune đã ở wave cuối chưa
            // Đây là điều kiện để biết có phải "wave cuối của tune cuối" không
            bool allTunesAtLastWave = true;
            for (int i = 0; i < tunes.Count; i++)
            {
                // Nếu tune này đã hoàn thành → bỏ qua
                if (tuneCompleted.ContainsKey(i) && tuneCompleted[i])
                {
                    continue;
                }
                
                // Nếu tune này đang chạy
                if (tuneCoroutines.ContainsKey(i) && tuneCoroutines[i] != null)
                {
                    if (tuneCurrentWave.ContainsKey(i))
                    {
                        int currentWave = tuneCurrentWave[i];
                        int totalWaves = tunes[i].waves.Count;
                        bool isTuneAtLastWave = (currentWave == totalWaves - 1);
                        
                        if (!isTuneAtLastWave)
                        {
                            allTunesAtLastWave = false;
                            Debug.Log($"🎯 Tune {i} chưa ở wave cuối (wave {currentWave + 1}/{totalWaves})");
                            break;
                        }
                    }
                    else
                    {
                        // Tune đang chạy nhưng chưa có tracking → chưa ở wave cuối
                        allTunesAtLastWave = false;
                        Debug.Log($"🎯 Tune {i} đang chạy nhưng chưa có tracking wave");
                        break;
                    }
                }
                // Nếu tune này chưa được start → không tính vào
                // (có thể tune chưa được start hoặc đã xong)
            }
            
            // Chỉ coi là "wave cuối của tune cuối" khi:
            // 1. Đây là wave cuối của tune hiện tại
            // 2. TẤT CẢ tune đều đang ở wave cuối (hoặc đã xong)
            IsLastWaveOfLastTune = isLastWave && allTunesAtLastWave;
            
            // Debug chi tiết để kiểm tra logic
            Debug.Log($"🎯 [Wave Check] Tune {tuneIdx} (index {tuneIdx + 1}/{tunes.Count}), Wave {waveIdx} (index {waveIdx + 1}/{tune.waves.Count})");
            Debug.Log($"🎯 isLastWave: {isLastWave} (waveIdx={waveIdx}, waves.Count-1={tune.waves.Count - 1})");
            Debug.Log($"🎯 allTunesAtLastWave: {allTunesAtLastWave}");
            Debug.Log($"🎯 IsLastWaveOfLastTune: {IsLastWaveOfLastTune}");
            
            // Cảnh báo nếu logic có vẻ sai
            if (IsLastWaveOfLastTune)
            {
                Debug.Log($"⚠️ ⭐ ĐÂY LÀ WAVE CUỐI CỦA TẤT CẢ TUNE! ⭐ Tune {tuneIdx + 1}/{tunes.Count}, Wave {waveIdx + 1}/{tune.waves.Count}");
            }

            // Reset counter và flag khi bắt đầu wave mới
            if (!IsLastWaveOfLastTune)
            {
                activeEnemiesInCurrentWave = 0;
                isLastWaveSpawningCompleted = false;
                victoryInvoked = false;
            }
            else
            {
                // Wave cuối: KHÔNG reset counter để giữ lại enemy từ wave trước
                isLastWaveSpawningCompleted = false;
                victoryInvoked = false;
                // Tính số enemy dự kiến trong wave cuối
                expectedEnemiesInLastWave = CalculateTotalEnemyCount(wave);
                Debug.Log($"⚠️ ⭐ WAVE CUỐI BẮT ĐẦU! ⭐ Enemy hiện tại: {activeEnemiesInCurrentWave}, Dự kiến spawn: {expectedEnemiesInLastWave}");
            }

            Debug.Log($"🎵 [Tune {tuneIdx}] Bắt đầu Wave {waveIdx + 1}");
            OnWaveStart?.Invoke(wave);

            skipDelays[tuneIdx] = false;

            //bool isLastWave = (waveIdx == tune.waves.Count - 1);
            // QUAN TRỌNG: Chỉ xử lý logic đặc biệt cho wave cuối của TUNE CUỐI CÙNG
            if (IsLastWaveOfLastTune) // Chỉ check flag này, không cần check isLastWave lại
            {
                Debug.Log($"🕹 [Tune {tuneIdx}] ⭐ WAVE CUỐI CỦA TUNE CUỐI ⭐ — không hiện countdown hoặc nút skip.");

                if (countdownText != null)
                    countdownText.gameObject.SetActive(false);
                    
                if (skipButton != null)
                    skipButton.gameObject.SetActive(false);

                // Đợi wave spawn xong (tính toán thời gian spawn)
                float totalSpawnTime = CalculateTotalSpawnTime(wave);
                yield return new WaitForSeconds(totalSpawnTime);

                // Đợi thêm một chút để đảm bảo tất cả enemy đã được spawn và counter đã cập nhật
                yield return new WaitForSeconds(0.5f);

                // Đánh dấu wave đã spawn xong (CHỈ khi là wave cuối của tune cuối)
                isLastWaveSpawningCompleted = true;
                Debug.Log($"✅ Wave cuối của TUNE CUỐI đã spawn xong! Enemy hiện tại: {activeEnemiesInCurrentWave}, Đang đợi enemy bị tiêu diệt hết...");

                // Kiểm tra lại xem có còn enemy nào không (sau khi đã đợi đủ)
                CheckForVictory();

                // Bắt đầu coroutine check liên tục để đảm bảo không bỏ sót
                StartCoroutine(ContinuousVictoryCheck());

                continue; // Không cần delay giữa wave nữa vì đã là wave cuối
            }
            
            // Nếu KHÔNG phải wave cuối của tune cuối, xử lý bình thường (có skip button)
            Debug.Log($"🔄 [Tune {tuneIdx}] Wave {waveIdx + 1} - KHÔNG phải wave cuối của tune cuối. Sẽ hiện skip button sau {showButtonDelay}s.");
            
            if (skipButton != null)
            {
                skipButton.gameObject.SetActive(false);
                Debug.Log($"🔄 Skip button đã ẩn (sẽ hiện sau {showButtonDelay}s)");
            }
            else
            {
                Debug.LogWarning($"⚠️ Skip button không tồn tại cho Tune {tuneIdx}!");
            }

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

                // Cập nhật remaining hiện tại để Skip có thể dùng tính thưởng
                tuneRemainingCountdown[tuneIdx] = remaining;

                // Sau showButtonDelay giây thì hiện nút skip
                if (timer >= showButtonDelay && skipButton != null && !skipButton.gameObject.activeSelf)
                {
                    skipButton.gameObject.SetActive(true);
                    Debug.Log($"✅ Skip button đã được hiện cho Tune {tuneIdx} sau {showButtonDelay}s");
                }

                yield return null;
            }

            // Ẩn text và nút khi xong
            if (countdownText != null)
                countdownText.gameObject.SetActive(false);

            if (skipButton != null)
            {
                skipButton.gameObject.SetActive(false);
                Debug.Log($"🔄 Skip button đã ẩn sau khi delay xong");
            }
        }

        // Đánh dấu tune này đã hoàn thành
        tuneCompleted[tuneIdx] = true;
        tuneCurrentWave[tuneIdx] = tune.waves.Count - 1; // Đảm bảo tracking đúng
        
        Debug.Log($"✅ [Tune {tuneIdx}] Hoàn thành toàn bộ! Tất cả {tune.waves.Count} wave đã xong.");
        
        // Kiểm tra lại xem có phải wave cuối của tất cả tune không
        bool allTunesCompleted = true;
        for (int i = 0; i < tunes.Count; i++)
        {
            if (!tuneCompleted.ContainsKey(i) || !tuneCompleted[i])
            {
                allTunesCompleted = false;
                break;
            }
        }
        
        if (allTunesCompleted)
        {
            Debug.Log($"🎉 TẤT CẢ TUNE ĐÃ HOÀN THÀNH! Tất cả {tunes.Count} tune đã xong.");
        }
    }

    // Gọi từ UI button riêng của mỗi Tune
    public void SkipToNextWave(int tuneIdx)
    {
        if (!skipDelays.ContainsKey(tuneIdx))
            skipDelays[tuneIdx] = false;

        // Tính thưởng dựa trên thời gian còn lại (làm tròn lên)
        if (tuneRemainingCountdown.TryGetValue(tuneIdx, out float remaining))
        {
            int reward = Mathf.CeilToInt(remaining * Mathf.Max(0f, earlySpawnRewardMultiplier));
            if (reward > 0 && GoldManager.Instance != null)
            {
                GoldManager.Instance.AddGold(reward);
                Debug.Log($"💰 Early spawn reward: +{reward} gold (remaining={remaining:F1}s, multiplier={earlySpawnRewardMultiplier})");
            }
        }

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
        Debug.Log($"➕ Enemy spawned. Counter tăng lên: {activeEnemiesInCurrentWave}");
    }
    
    /// <summary>
    /// Method test để kiểm tra thủ công (có thể gọi từ Inspector hoặc code khác)
    /// </summary>
    [ContextMenu("Test Check Victory")]
    public void TestCheckVictory()
    {
        Debug.Log("🧪 TEST: Kiểm tra thủ công điều kiện thắng...");
        Debug.Log($"🧪 IsLastWaveOfLastTune: {IsLastWaveOfLastTune}");
        Debug.Log($"🧪 isLastWaveSpawningCompleted: {isLastWaveSpawningCompleted}");
        Debug.Log($"🧪 victoryInvoked: {victoryInvoked}");
        Debug.Log($"🧪 Counter: {activeEnemiesInCurrentWave}");
        
        int actual = GetActualActiveEnemyCount();
        Debug.Log($"🧪 Actual Enemies: {actual}");
        
        CheckForVictory();
    }

    // Kiểm tra khi enemy bị ẩn/return về pool
    public void OnEnemyReturned()
    {
        if (activeEnemiesInCurrentWave > 0)
        {
            activeEnemiesInCurrentWave--;
        }
        else
        {
            Debug.LogWarning($"⚠️ OnEnemyReturned được gọi nhưng activeEnemiesInCurrentWave <= 0! Giá trị: {activeEnemiesInCurrentWave}. Có thể do tracking sai hoặc enemy đã bị giết trước đó.");
        }
        
        // QUAN TRỌNG: CHỈ check victory nếu là wave cuối của TUNE CUỐI và đã spawn xong
        // Phải kiểm tra cả 2 điều kiện để tránh win sớm
        if (IsLastWaveOfLastTune && isLastWaveSpawningCompleted)
        {
            Debug.Log($"📊 Enemy returned. Counter={activeEnemiesInCurrentWave}. Đang kiểm tra thắng (wave cuối của TUNE CUỐI đã spawn xong)...");
            CheckForVictory();
        }
        else
        {
            // Log chi tiết để debug
            if (IsLastWaveOfLastTune && !isLastWaveSpawningCompleted)
            {
                Debug.Log($"📊 Enemy returned. Counter={activeEnemiesInCurrentWave}. Là wave cuối của TUNE CUỐI nhưng CHƯA spawn xong. Không check victory.");
            }
            else if (!IsLastWaveOfLastTune)
            {
                Debug.Log($"📊 Enemy returned. Counter={activeEnemiesInCurrentWave}. KHÔNG phải wave cuối của TUNE CUỐI. Không check victory.");
            }
        }
    }

    /// <summary>
    /// Kiểm tra điều kiện thắng và phát event nếu đủ điều kiện
    /// </summary>
    private void CheckForVictory()
    {
        Debug.Log($"🔍 CheckForVictory được gọi: IsLastWaveOfLastTune={IsLastWaveOfLastTune}, isLastWaveSpawningCompleted={isLastWaveSpawningCompleted}, victoryInvoked={victoryInvoked}");
        
        // Chỉ check khi: là wave cuối, đã spawn xong, và chưa trigger victory
        if (!IsLastWaveOfLastTune)
        {
            Debug.Log("❌ Không phải wave cuối, bỏ qua.");
            return;
        }
        
        if (!isLastWaveSpawningCompleted)
        {
            Debug.Log("❌ Wave cuối chưa spawn xong, bỏ qua.");
            return;
        }
        
        if (victoryInvoked)
        {
            Debug.Log("❌ Victory đã được trigger rồi, bỏ qua.");
            return;
        }

        // Kiểm tra số enemy thực tế trên map (QUAN TRỌNG: Dựa vào actual, không phải counter vì counter có thể sai)
        int actualActiveEnemies = GetActualActiveEnemyCount();
        
        Debug.Log($"🔍 CheckForVictory: Counter={activeEnemiesInCurrentWave}, Actual={actualActiveEnemies}, SpawnCompleted={isLastWaveSpawningCompleted}, VictoryInvoked={victoryInvoked}");

        // Đồng bộ counter với actual nếu khác nhau (để debug)
        if (activeEnemiesInCurrentWave != actualActiveEnemies)
        {
            Debug.LogWarning($"⚠️ Counter không khớp! Counter={activeEnemiesInCurrentWave}, Actual={actualActiveEnemies}. Đang đồng bộ counter với actual...");
            activeEnemiesInCurrentWave = actualActiveEnemies;
        }

        // QUAN TRỌNG: Chỉ dựa vào actual count, không phụ thuộc counter vì counter có thể bị sai
        // Chỉ thắng khi: KHÔNG còn enemy nào trên map (actual = 0)
        if (actualActiveEnemies == 0)
        {
            victoryInvoked = true; // Đánh dấu đã trigger để tránh trigger nhiều lần
            Debug.Log("🎉🎉🎉 THÔNG BÁO: Enemy cuối cùng của wave cuối cùng đã được ẩn đi! Đang trigger victory...");
            Debug.Log($"✅ Điều kiện thắng: IsLastWave={IsLastWaveOfLastTune}, SpawnCompleted={isLastWaveSpawningCompleted}, ActualEnemies={actualActiveEnemies}");

            // Delay một chút để đảm bảo tất cả logic đã hoàn thành
            StartCoroutine(DelayedInvokeVictory());
        }
        else
        {
            Debug.Log($"⏳ Vẫn còn {actualActiveEnemies} enemy trên map, chưa thể thắng. Đợi thêm...");
        }
    }

    /// <summary>
    /// Đếm số enemy thực tế đang active trên map (chỉ đếm enemy đang thực sự hoạt động)
    /// </summary>
    private int GetActualActiveEnemyCount()
    {
        int finalCount = 0;
        HashSet<GameObject> countedEnemies = new HashSet<GameObject>(); // Tránh đếm trùng
        List<string> enemyNames = new List<string>(); // Debug: lưu tên enemy để log

        // Cách 1: Tìm bằng tag "Enemy" (nhanh nhất và đáng tin cậy nhất)
        GameObject[] enemiesByTag = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemiesByTag)
        {
            if (enemy != null && enemy.activeInHierarchy && !countedEnemies.Contains(enemy))
            {
                countedEnemies.Add(enemy);
                enemyNames.Add(enemy.name);
                finalCount++;
            }
        }

        // Cách 2: Tìm bằng FindObjectsOfType (backup)
        if (finalCount == 0)
        {
            _Enemy[] allEnemies = FindObjectsOfType<_Enemy>(false); // false = chỉ tìm active
            
            foreach (_Enemy enemy in allEnemies)
            {
                if (enemy == null || enemy.gameObject == null) continue;
                
                if (enemy.gameObject.activeInHierarchy && !countedEnemies.Contains(enemy.gameObject))
                {
                    countedEnemies.Add(enemy.gameObject);
                    enemyNames.Add(enemy.gameObject.name);
                    finalCount++;
                }
            }
        }

        // Cách 3: Tìm trong pool (nếu có)
        if (MultiEnemyPool.Instance != null)
        {
            foreach (Transform child in MultiEnemyPool.Instance.transform)
            {
                if (child != null && child.gameObject != null && 
                    child.gameObject.activeInHierarchy && 
                    !countedEnemies.Contains(child.gameObject))
                {
                    // Kiểm tra xem có phải enemy không
                    if (child.GetComponent<_Enemy>() != null || 
                        child.GetComponent<EnemyBase>() != null ||
                        child.gameObject.CompareTag("Enemy"))
                    {
                        countedEnemies.Add(child.gameObject);
                        if (!enemyNames.Contains(child.gameObject.name))
                        {
                            enemyNames.Add(child.gameObject.name);
                            finalCount++;
                        }
                    }
                }
            }
        }
        
        // Debug log chi tiết
        if (finalCount > 0)
        {
            Debug.Log($"🔍 GetActualActiveEnemyCount: Tìm thấy {finalCount} enemy active: {string.Join(", ", enemyNames)}");
        }
        else
        {
            Debug.Log($"🔍 GetActualActiveEnemyCount: KHÔNG tìm thấy enemy nào trên map! (Final=0)");
        }
        
        return finalCount;
    }

    /// <summary>
    /// Tính tổng số enemy trong một wave
    /// </summary>
    private int CalculateTotalEnemyCount(WaveConfig wave)
    {
        int total = 0;
        foreach (var group in wave.groups)
        {
            total += group.count;
        }
        return total;
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
    /// Coroutine kiểm tra liên tục sau khi wave cuối spawn xong
    /// </summary>
    private System.Collections.IEnumerator ContinuousVictoryCheck()
    {
        Debug.Log("🔄 ContinuousVictoryCheck bắt đầu! Sẽ check mỗi 0.5 giây...");
        int checkCount = 0;
        
        while (IsLastWaveOfLastTune && isLastWaveSpawningCompleted && !victoryInvoked)
        {
            yield return new WaitForSeconds(0.5f); // Check mỗi 0.5 giây
            checkCount++;
            Debug.Log($"🔄 ContinuousVictoryCheck lần {checkCount}: Đang kiểm tra...");
            CheckForVictory();
        }
        
        Debug.Log($"✅ ContinuousVictoryCheck đã dừng. Đã check {checkCount} lần. IsLastWave={IsLastWaveOfLastTune}, SpawnCompleted={isLastWaveSpawningCompleted}, VictoryInvoked={victoryInvoked}");
    }

    /// <summary>
    /// Delay một frame trước khi phát event thắng
    /// </summary>
    private System.Collections.IEnumerator DelayedInvokeVictory()
    {
        yield return null; // Chờ 1 frame
        
        // Kiểm tra lại một lần nữa để chắc chắn (chỉ dựa vào actual)
        int finalCheck = GetActualActiveEnemyCount();
        Debug.Log($"🔍 Final check trước khi invoke: Counter={activeEnemiesInCurrentWave}, Actual={finalCheck}");
        
        // CHỈ dựa vào actual count, không cần counter
        if (finalCheck == 0)
        {
            Debug.Log("🚀 Phát event OnAllWavesCompleted!");
            if (OnAllWavesCompleted != null)
            {
                Debug.Log($"✅ OnAllWavesCompleted có {OnAllWavesCompleted.GetInvocationList().Length} listener(s)");
                OnAllWavesCompleted.Invoke();
            }
            else
            {
                Debug.LogError("❌ OnAllWavesCompleted event không có listener nào!");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ Final check thất bại! Vẫn còn {finalCheck} enemy trên map. Không invoke event.");
            victoryInvoked = false; // Reset flag để có thể thử lại
        }
    }
}
