using UnityEngine;

/// <summary>
/// Xử lý Game Over và hiển thị kết quả (Single Responsibility)
/// Tuân theo Dependency Inversion - phụ thuộc vào abstraction (IGameOverHandler)
/// </summary>
public class GameOverHandler : MonoBehaviour, IGameOverHandler, ILifeListener
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;

    [SerializeField] private GameObject gameWinPanel;
    [SerializeField] private Animator boardMenuAnimator;
    [SerializeField] private Animator boardWinAnimator;
    [SerializeField] private UnityEngine.UI.Image[] starImages; // 3 ảnh sao
    [SerializeField] private Sprite starFilledSprite;
    [SerializeField] private Sprite starEmptySprite;

    [Header("Dependencies")]
    [SerializeField] private LifeManager lifeManager;
    private IStarRatingCalculator starCalculator;

    //[SerializeField] PauseMenu pauseMenu;
    private void Start()
    {
        //pauseMenu = GetComponent<PauseMenu>();

        // Dependency Injection - có thể thay đổi cách tính sao
        starCalculator = new StarRatingCalculator();

        // Đảm bảo có LifeManager
        if (lifeManager == null)
        {
            lifeManager = LifeManager.Instance;
        }

        // Đăng ký lắng nghe thay đổi máu
        if (lifeManager != null)
        {
            lifeManager.Subscribe(this);
        }

        // Đăng ký lắng nghe event khi tất cả wave hoàn thành (thắng)
        RegisterWaveManagerEvent();

        // Ẩn panel ban đầu
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (gameWinPanel != null)
        {
            gameWinPanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        // Đăng ký lại event khi GameObject được enable (nếu WaveManager đã sẵn sàng)
        RegisterWaveManagerEvent();
    }

    private void OnDestroy()
    {
        // Hủy đăng ký khi bị destroy
        if (lifeManager != null)
        {
            lifeManager.Unsubscribe(this);
        }

        // Hủy đăng ký event WaveManager
        UnregisterWaveManagerEvent();
    }

    /// <summary>
    /// Đăng ký event từ WaveManager
    /// </summary>
    private void RegisterWaveManagerEvent()
    {
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnAllWavesCompleted -= OnAllWavesCompleted; // Unsubscribe trước để tránh duplicate
            WaveManager.Instance.OnAllWavesCompleted += OnAllWavesCompleted;
            Debug.Log("✅ GameOverHandler: Đã đăng ký event OnAllWavesCompleted từ WaveManager");
        }
        else
        {
            Debug.LogWarning("⚠️ GameOverHandler: WaveManager.Instance chưa sẵn sàng, sẽ thử lại sau...");
            // Thử lại sau một frame
            StartCoroutine(RetryRegisterWaveManagerEvent());
        }
    }

    /// <summary>
    /// Hủy đăng ký event từ WaveManager
    /// </summary>
    private void UnregisterWaveManagerEvent()
    {
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnAllWavesCompleted -= OnAllWavesCompleted;
            Debug.Log("✅ GameOverHandler: Đã hủy đăng ký event OnAllWavesCompleted từ WaveManager");
        }
    }

    /// <summary>
    /// Retry đăng ký event nếu WaveManager chưa sẵn sàng
    /// </summary>
    private System.Collections.IEnumerator RetryRegisterWaveManagerEvent()
    {
        yield return new WaitUntil(() => WaveManager.Instance != null);
        RegisterWaveManagerEvent();
    }

    /// <summary>
    /// Implement ILifeListener - được gọi khi máu thay đổi
    /// </summary>
    public void OnLifeChanged(int currentLife, int maxLife)
    {
        // Có thể cập nhật UI hiển thị máu ở đây nếu cần
    }

    /// <summary>
    /// Implement ILifeListener - được gọi khi hết máu
    /// </summary>
    public void OnLifeDepleted()
    {
        ShowGameOver();
    }

    /// <summary>
    /// Được gọi khi tất cả wave đã hoàn thành - kiểm tra thắng hay thua
    /// </summary>
    private void OnAllWavesCompleted()
    {
        Debug.Log("🎯 GameOverHandler: OnAllWavesCompleted được gọi!");
        
        // Kiểm tra nếu player còn máu thì thắng
        if (lifeManager == null)
        {
            Debug.LogError("❌ GameOverHandler: lifeManager is null!");
            lifeManager = LifeManager.Instance;
            if (lifeManager == null)
            {
                Debug.LogError("❌ GameOverHandler: LifeManager.Instance cũng là null!");
                return;
            }
        }
        
        if (lifeManager.CurrentLife > 0)
        {
            Debug.Log($"✅ Player thắng với {lifeManager.CurrentLife}/{lifeManager.MaxLife} máu còn lại!");
            ShowVictory();
        }
        else
        {
            Debug.LogWarning($"⚠️ Tất cả wave đã hoàn thành nhưng player đã hết máu! (CurrentLife={lifeManager.CurrentLife})");
        }
    }

    /// <summary>
    /// Hiển thị panel thắng khi hoàn thành tất cả wave mà còn máu
    /// </summary>
    public void ShowVictory()
    {
        Debug.Log("🎉 ShowVictory() được gọi!");
        
        if (gameWinPanel == null)
        {
            Debug.LogError("❌ GameWinPanel chưa được gán trong Inspector!");
            return;
        }

        // Tính số sao dựa trên máu còn lại
        int stars = 0;
        if (lifeManager != null && starCalculator != null)
        {
            stars = starCalculator.CalculateStars(lifeManager.CurrentLife, lifeManager.MaxLife);
            Debug.Log($"⭐ Tính được {stars} sao từ {lifeManager.CurrentLife}/{lifeManager.MaxLife} máu.");
        }
        else
        {
            Debug.LogWarning($"⚠️ lifeManager={lifeManager}, starCalculator={starCalculator}");
        }

        // Hiển thị sao
        UpdateStarDisplay(stars);

        // Hiển thị panel
        Debug.Log("🖼️ Đang hiển thị gameWinPanel...");
        gameWinPanel.SetActive(true);
        
        if (boardWinAnimator != null)
        {
            boardWinAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            boardWinAnimator.Play("BoardMenu");
            Debug.Log("▶️ Đã play animation BoardMenu.");
        }
        else
        {
            Debug.LogWarning("⚠️ boardWinAnimator chưa được gán!");
        }

        // Dừng game
        Time.timeScale = 0f;
        Debug.Log("⏸️ Time.timeScale đã được set về 0.");

        Debug.Log($"🎉 VICTORY! Đã hoàn thành tất cả wave! Số sao đạt được: {stars}");
    }

    /// <summary>
    /// Implement IGameOverHandler - Hiển thị panel game over
    /// </summary>
    public void ShowGameOver(int stars = -1)
    {
        //pauseMenu.PauseGame();
        if (gameOverPanel == null)
        {
            Debug.LogWarning("GameOverPanel chưa được gán!");
            return;
        }

        // Tính số sao nếu chưa có
        if (stars < 0 && lifeManager != null && starCalculator != null)
        {
            stars = starCalculator.CalculateStars(lifeManager.CurrentLife, lifeManager.MaxLife);
        }

        // Hiển thị sao
        UpdateStarDisplay(stars);

        // Hiển thị panel
        gameOverPanel.SetActive(true);
        if (boardMenuAnimator != null)
        {
            boardMenuAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            boardMenuAnimator.Play("BoardMenu");
        }

        // Dừng game
        Time.timeScale = 0f;

        Debug.Log($"🎮 Game Over! Số sao đạt được: {stars}");
    }

    /// <summary>
    /// Implement IGameOverHandler - Ẩn panel game over
    /// </summary>
    public void HideGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Cập nhật hiển thị sao
    /// </summary>
    private void UpdateStarDisplay(int stars)
    {

        if (starImages == null || starImages.Length != 3)
        {
            Debug.LogWarning("StarImages chưa được setup đúng! Cần 3 ảnh sao.");
            return;
        }

        if (starFilledSprite == null || starEmptySprite == null)
        {
            Debug.LogWarning("Star sprites chưa được gán!");
            return;
        }

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] != null)
            {
                // i + 1 vì sao bắt đầu từ 1
                starImages[i].sprite = (i + 1 <= stars) ? starFilledSprite : starEmptySprite;
            }
        }
    }

    /// <summary>
    /// Set Star Calculator (cho phép thay đổi cách tính sao - Strategy Pattern)
    /// </summary>
    public void SetStarCalculator(IStarRatingCalculator calculator)
    {
        this.starCalculator = calculator;
    }

    // 🔁 Nút Chơi Lại (Restart Level)
    public void RestartLevel()
    {
        Debug.Log("🔄 [GameOverHandler] RestartLevel called!");
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (!string.IsNullOrEmpty(currentScene.name))
        {
            SceneVideoTransition.LoadScene(currentScene.name);
        }
    }

    public void Replay()
    {
        RestartLevel();
    }

    // 🏠 Nút Trở về Home (MainMap)
    public void ExitToHome()
    {
        Debug.Log("🏠 [GameOverHandler] ExitToHome called!");
        Time.timeScale = 1f;
        SceneVideoTransition.LoadScene("MainMap");
    }
}

