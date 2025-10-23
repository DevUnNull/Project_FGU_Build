using UnityEngine;

public class LogicSpawner : MonoBehaviour
{
    // Kéo thả GameObject chứa script ContinuousSpawner vào đây
    public ContinuousSpawner spawner;

    public void Start()
    {
        // Kiểm tra xem biến spawner đã được gán chưa
        if (spawner != null)
        {
            spawner.StartSpawning();
        }
        else
        {
            Debug.LogError("Chưa gán ContinuousSpawner trong Inspector!");
        }
    }
}
