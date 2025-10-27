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
        pausePanel.SetActive(true);

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
        pausePanel.SetActive(false);
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

        SceneManager.LoadScene("AllMap");
    }
}
