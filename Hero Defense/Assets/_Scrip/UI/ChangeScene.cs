using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    // Hàm đổi scene theo tên
    public void ChangeScener(string sceneName)
    {
        // Kiểm tra xem scene có tồn tại trong Build Settings không
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' không tồn tại trong Build Settings!");
        }
    }
}
