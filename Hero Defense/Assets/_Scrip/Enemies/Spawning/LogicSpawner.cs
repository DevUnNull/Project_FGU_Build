using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LogicSpawner : MonoBehaviour
{
    [Header("UI Button Start Game")]
    [SerializeField] private Button startGameButton; // Gán trong Inspector

    private void Start()
    {
        if (startGameButton != null)
        {
            startGameButton.gameObject.SetActive(true); // hiện nút lúc đầu
            startGameButton.onClick.AddListener(OnStartGameClicked); // gán sự kiện click
        }
        else
        {
            Debug.LogWarning("⚠️ Chưa gán StartGame Button trong Inspector!");
        }
    }

    private void OnStartGameClicked()
    {
        Debug.Log("🎬 Bắt đầu game!");

        // Ẩn nút sau khi ấn
        if (startGameButton != null)
            startGameButton.gameObject.SetActive(false);

        // Bắt đầu Tune đầu tiên (Path1)
        WaveManager.Instance.StartTune(0);

        // Sau 15s thì tự động bắt đầu Tune thứ hai (Path2)
        StartCoroutine(StartSecondTuneAfterDelay(15f));
    }

    private IEnumerator StartSecondTuneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("▶️ 15s đã trôi qua — bắt đầu Tune 1 (Path2)");
        WaveManager.Instance.StartTune(1);
    }
}
