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
        // Kiểm tra xem scene có tồn tại trong Build Settings không
        // Do đổi scene liên quan đến set âm thanh nên em sẽ bổ sung thêm nha (LINH-01/11)
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            StartCoroutine(ChangeSceneWithFade(sceneName));
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' không tồn tại trong Build Settings!");
        }
    }
     private IEnumerator ChangeSceneWithFade(string sceneName )
    {
        if (musicController != null)
            musicController.FadeOutAndStop(delayBeforeLoad); // fade-out

        yield return new WaitForSeconds(delayBeforeLoad);
        SceneManager.LoadScene(sceneName);
    }
}
