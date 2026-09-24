using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UIButtonEffect))]
public class LevelButton : MonoBehaviour, IPointerClickHandler
{
    [Header("Scene to Load")]
    public string sceneName;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Scene name is empty on " + gameObject.name);
        }
    }
}
