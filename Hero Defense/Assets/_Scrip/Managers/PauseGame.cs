using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;       // Panel chứa menu pause
    [SerializeField] private Animator boardMenuAnimator;  // Animator của BoardMenu
    [SerializeField] private float closeAnimTime = 0.3f;  // Thời gian animation đóng

    private bool isPaused = false;

    private void Awake()
    {
        // Đăng ký callback khi scene được load
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Tìm ngay lập tức nếu có thể
        TryFindReferences();
    }

    private void OnDestroy()
    {
        // Hủy đăng ký callback
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Tìm lại references mỗi khi scene được load
        TryFindReferences();
    }

    private void TryFindReferences()
    {
        // Tìm lại references nếu bị mất do DontDestroyOnLoad
        if (pausePanel == null)
        {
            GameObject defeatObj = GameObject.Find("Defeat");
            if (defeatObj != null)
            {
                pausePanel = defeatObj;
                Debug.Log("✅ [PauseMenu] Đã tìm lại PausePanel reference từ GameObject 'Defeat'");
            }
        }

        if (boardMenuAnimator == null)
        {
            GameObject boardMenuObj = GameObject.Find("BoardMenu");
            if (boardMenuObj != null)
            {
                boardMenuAnimator = boardMenuObj.GetComponent<Animator>();
                if (boardMenuAnimator != null)
                {
                    Debug.Log("✅ [PauseMenu] Đã tìm lại BoardMenuAnimator reference từ GameObject 'BoardMenu'");
                }
            }
        }
    }

    private void Start()
    {
        // Tìm lại references nếu vẫn chưa tìm thấy (fallback)
        if (pausePanel == null)
        {
            GameObject defeatObj = GameObject.Find("Defeat");
            if (defeatObj != null)
            {
                pausePanel = defeatObj;
                Debug.Log("✅ Đã tìm lại PausePanel reference từ GameObject 'Defeat'");
            }
        }

        if (boardMenuAnimator == null)
        {
            GameObject boardMenuObj = GameObject.Find("BoardMenu");
            if (boardMenuObj != null)
            {
                boardMenuAnimator = boardMenuObj.GetComponent<Animator>();
                if (boardMenuAnimator != null)
                {
                    Debug.Log("✅ Đã tìm lại BoardMenuAnimator reference từ GameObject 'BoardMenu'");
                }
            }
        }
    }

    // 🔁 Gọi hàm này khi nhấn nút pause (ví dụ gán vào button)
    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    // 🧊 Dừng game + mở menu
    public void PauseGame()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        if (boardMenuAnimator != null)
        {
            boardMenuAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            boardMenuAnimator.Play("BoardMenu"); // animation mở menu
        }

        Time.timeScale = 0f;
        isPaused = true;
    }

    // ▶️ Đóng menu + tiếp tục game
    public void ResumeGame()
    {
        if (boardMenuAnimator != null)
        {
            boardMenuAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            boardMenuAnimator.Play("BoardMenuClouse");
        }

        StartCoroutine(ResumeAfterAnim(closeAnimTime));
    }

    private IEnumerator ResumeAfterAnim(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Time.timeScale = 1f;
        isPaused = false;
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    // 🔁 Restart lại màn chơi (chạy animation đóng menu trước)
    public void RestartLevel()
    {
        StartCoroutine(RestartAfterAnim(closeAnimTime));
    }

    private IEnumerator RestartAfterAnim(float delay)
    {
        // ✅ Chạy animation đóng menu
        if (boardMenuAnimator != null)
        {
            boardMenuAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            boardMenuAnimator.Play("BoardMenuClouse");
        }

        yield return new WaitForSecondsRealtime(delay); // dùng thời gian unscaled

        Time.timeScale = 1f;

        if (MultiEnemyPool.Instance != null)
            Destroy(MultiEnemyPool.Instance.gameObject);

        // Xóa listeners cũ trước khi reload scene để tránh null reference
        if (LifeManager.Instance != null)
            LifeManager.Instance.ClearListeners();

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    // 🏠 Thoát về Home
    public void ExitToHome()
    {
        StartCoroutine(ExitToHomeAfterAnim(closeAnimTime));
    }

    private IEnumerator ExitToHomeAfterAnim(float delay)
    {
        if (boardMenuAnimator != null)
        {
            boardMenuAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            boardMenuAnimator.Play("BoardMenuClouse");
        }

        yield return new WaitForSecondsRealtime(delay);

        Time.timeScale = 1f;

        if (MultiEnemyPool.Instance != null)
            Destroy(MultiEnemyPool.Instance.gameObject);

        // Xóa listeners cũ trước khi load scene mới để tránh null reference
        if (LifeManager.Instance != null)
            LifeManager.Instance.ClearListeners();

        SceneManager.LoadScene("AllMap");
    }
}
