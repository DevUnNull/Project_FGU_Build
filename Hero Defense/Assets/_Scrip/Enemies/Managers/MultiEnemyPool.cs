using System.Collections.Generic;
using UnityEngine;

public class MultiEnemyPool : MonoBehaviour
{
    public static MultiEnemyPool Instance { get; private set; }

    [SerializeField] private PoolConfig config;
    private Dictionary<EnemyType, Queue<GameObject>> pools = new();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    void Start() => InitializePools();

    private void InitializePools()
    {
        foreach (var entry in config.pools)
        {
            pools[entry.type] = new Queue<GameObject>();
            for (int i = 0; i < entry.initialSize; i++)
            {
                GameObject obj = Instantiate(entry.prefab, transform);
                obj.SetActive(false);
                pools[entry.type].Enqueue(obj);
            }
        }
    }

    public GameObject Get(EnemyType type)
    {
        if (pools.ContainsKey(type) && pools[type].Count > 0)
        {
            GameObject enemy = pools[type].Dequeue();
            enemy.SetActive(true);
            return enemy;
        }
        // Expand: Instantiate from config
        var entry = config.pools.Find(p => p.type == type);
        if (entry != null)
        {
            GameObject newEnemy = Instantiate(entry.prefab, transform);
            return newEnemy;
        }
        Debug.LogError($"No prefab for {type}");
        return null;
    }

    public void ReturnToPool(GameObject enemy)
    {
        if (enemy.TryGetComponent<IPoolable>(out var poolable))
        {
            poolable.Reset();
            enemy.SetActive(false);
            pools[poolable.GetEnemyType()].Enqueue(enemy);
        }
    }
}