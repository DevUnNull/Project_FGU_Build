using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public SceneUIController musicController;
    
    [Header("Settings")]
    public float delayBeforeLoad = 0.1f;

    // Hàm đổi scene theo tên
    public void ChangeScener(string sceneName)
    {
        Time.timeScale = 1f;
        if (musicController != null)
            musicController.FadeOutAndStop(delayBeforeLoad);

        SceneVideoTransition.LoadScene(sceneName);
    }

    // Hàm chơi lại màn hiện tại
    public void RestartCurrentScene()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        if (!string.IsNullOrEmpty(currentScene.name))
        {
            ChangeScener(currentScene.name);
        }
    }
}
