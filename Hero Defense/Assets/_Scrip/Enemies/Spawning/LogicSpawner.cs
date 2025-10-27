using UnityEngine;

public class LogicSpawner : MonoBehaviour
{
    [SerializeField] private int startTune = 0;  // UI set

    void Start()
    {
        // Bắt đầu cả hai Tune cùng lúc
        WaveManager.Instance.StartTune(0); // Path1
        WaveManager.Instance.StartTune(1); // Path2
    }

}