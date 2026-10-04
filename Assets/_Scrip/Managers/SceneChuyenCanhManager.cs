using UnityEngine;

/// <summary>
/// Forwarding wrapper sang SceneVideoTransition để đảm bảo tương thích 100%
/// </summary>
public class SceneChuyenCanhManager : MonoBehaviour
{
    public static void LoadScene(string sceneName, float customDelay = -1f)
    {
        SceneVideoTransition.LoadScene(sceneName);
    }
}
