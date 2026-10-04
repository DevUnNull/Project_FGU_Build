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
        if (musicController != null)
            musicController.FadeOutAndStop(delayBeforeLoad);

        SceneVideoTransition.LoadScene(sceneName);
    }
}
