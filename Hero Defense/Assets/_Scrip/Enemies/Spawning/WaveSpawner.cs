using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Spawner Setup")]
    public Transform spawnPoint;          // Vị trí spawn enemy (thường là SpawnPoint_Path1)
    public WaypointPath assignedPath;     // Đường enemy sẽ đi theo (Path1, Path2,...)
    public PathID myPathID;               // Path ID tương ứng

    private Coroutine currentSpawnRoutine;

    private void OnEnable()
    {
        // Đăng ký lắng nghe sự kiện bắt đầu wave
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveStart += HandleWaveStart;
        }
        else
        {
            Debug.LogWarning($"{name}: WaveManager.Instance chưa khởi tạo khi OnEnable!");
        }
    }

    private void OnDisable()
    {
        // Hủy đăng ký khi tắt spawner
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveStart -= HandleWaveStart;

        StopSpawning();
    }

    private void HandleWaveStart(WaveConfig wave)
    {
        // Chỉ spawn nếu wave có PathID trùng với spawner này
        if (wave.pathID == myPathID)
        {
            StopSpawning(); // Dừng coroutine cũ (nếu có)
            currentSpawnRoutine = StartCoroutine(SpawnWaveRoutine(wave.groups));
        }
    }

    private IEnumerator SpawnWaveRoutine(List<WaveConfig.SpawnGroup> groups)
    {
        foreach (var group in groups)
        {
            for (int i = 0; i < group.count; i++)
            {
                SpawnSingleEnemy(group.type);
                yield return new WaitForSeconds(group.interval);
            }
        }
    }

    private void SpawnSingleEnemy(EnemyType type)
    {
        GameObject enemy = MultiEnemyPool.Instance.Get(type);
        if (enemy == null)
        {
            Debug.LogError($"❌ Không tìm thấy prefab cho {type} trong PoolConfig!");
            return;
        }

        // Đặt enemy tại điểm spawn
        enemy.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

        // Khởi tạo đường đi cho enemy
        var wayEnemy = enemy.GetComponent<WayEnemy>();
        if (wayEnemy != null)
        {
            wayEnemy.Init(assignedPath);
            Debug.Log($"✅ Spawned {type} on {myPathID}");
        }
        else
        {
            Debug.LogError($"❌ Enemy prefab {enemy.name} thiếu component WayEnemy!");
        }
    }

    // Dừng Coroutine spawn hiện tại
    public void StopSpawning()
    {
        if (currentSpawnRoutine != null)
        {
            StopCoroutine(currentSpawnRoutine);
            currentSpawnRoutine = null;
        }
    }
    private void Start()
    {
        // Nếu WaveManager chưa sẵn trong OnEnable, ta đăng ký lại ở đây
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveStart += HandleWaveStart;
            Debug.Log($"{name}: Đăng ký lại WaveManager thành công trong Start()");
        }
        else
        {
            StartCoroutine(WaitForWaveManager());
        }
    }

    private IEnumerator WaitForWaveManager()
    {
        yield return new WaitUntil(() => WaveManager.Instance != null);
        WaveManager.Instance.OnWaveStart += HandleWaveStart;
        Debug.Log($"{name}: WaveManager đã sẵn sàng, đăng ký thành công!");
    }

}
