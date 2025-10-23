using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousSpawner : AbtractSpawn
{
    public float spawnInterval = 3f;

    public override void StartSpawning()
    {
        // Bắt đầu Coroutine để sinh enemy liên tục
        StartCoroutine(SpawnRoutine());
    }

    public override void StopSpawning()
    {
        // Dừng Coroutine khi không cần sinh nữa
        StopAllCoroutines();
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            Debug.Log("spawn enemy");
            SpawnSingleEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
