using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbtractSpawn : MonoBehaviour, ISpawn
{
    public Transform spawnPoint;

    public void SpawnSingleEnemy()
    {
        // Thay thế Instantiate bằng việc lấy enemy từ Object Pool
        if (spawnPoint != null)
        {
            // Lấy enemy từ bể
            // GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation); // Bỏ dòng này
            GameObject newEnemy = EnemyPool.Instance.GetEnemy(); // Lấy enemy từ pool
            newEnemy.transform.position = spawnPoint.position;
            newEnemy.transform.rotation = spawnPoint.rotation;
            Debug.Log("Enemy đã được sinh ra từ pool.");
        }
        else
        {
            // Debug.LogError("Thiếu prefab enemy hoặc spawnPoint!"); // Bỏ dòng này
            Debug.LogError("Thiếu spawnPoint!");
        }
    }

    public abstract void StartSpawning();
    public abstract void StopSpawning();
}