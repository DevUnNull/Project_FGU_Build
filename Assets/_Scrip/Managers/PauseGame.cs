using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;       // Panel chứa menu pause
    [SerializeField] private Animator boardMenuAnimator;  // Animator của BoardMenu
    [SerializeField] private float closeAnimTime = 0.25f; // Thời gian animation đóng

    private bool isPaused = false;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryFindReferences();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryFindReferences();
    }

    private void TryFindReferences()
    {
        if (pausePanel == null)
        {
            pausePanel = gameObject;
        }

        if (boardMenuAnimator == null)
        {
            Transform boardTF = transform.Find("BoardMenu");
            if (boardTF != null)
            {
                boardMenuAnimator = boardTF.GetComponent<Animator>();
            }
        }
    }

    private void Start()
    {
        TryFindReferences();
    }

    // 🔁 Gọi hàm này khi nhấn nút pause
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
            try
            {
                boardMenuAnimator.Play("BoardMenu");
            }
            catch { }
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
            try
            {
                boardMenuAnimator.Play("BoardMenuClouse");
            }
            catch { }
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

    // 🔁 Restart lại màn chơi (Sử dụng SceneChuyenCanhManager để chạy Out_ChuyenCanh 3.7s)
    public void RestartLevel()
    {
        Debug.Log("🔄 [PauseMenu] RestartLevel called!");
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        if (!string.IsNullOrEmpty(currentScene.name))
        {
            SceneChuyenCanhManager.LoadScene(currentScene.name);
        }
    }

    // 🏠 Thoát về Home (Sử dụng SceneChuyenCanhManager để chạy Out_ChuyenCanh 3.7s)
    public void ExitToHome()
    {
        Debug.Log("🏠 [PauseMenu] ExitToHome called!");
        Time.timeScale = 1f;
        SceneChuyenCanhManager.LoadScene("MainMap");
    }
}
