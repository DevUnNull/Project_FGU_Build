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

            // Tự động gắn hiệu ứng animation thở & nảy khi click cho nút StartGame
            if (startGameButton.GetComponent<WaveButtonAnimation>() == null)
            {
                startGameButton.gameObject.AddComponent<WaveButtonAnimation>();
            }
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

        // Kích hoạt cờ đã bấm StartWave
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.SetWaveStarted(true);
        }

        // Bắt đầu tất cả các Tune (Path1 đến Path5)
        for (int i = 0; i < 5; i++)
        {
            WaveManager.Instance.StartTune(i);
        }
    }
}
