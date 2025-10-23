using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    // Singleton để truy cập dễ dàng
    public static EnemyPool Instance { get; private set; }

    // Prefab của enemy
    public GameObject enemyPrefab;
    // Kích thước bể ban đầu
    public int poolSize = 10;

    // Hàng đợi để lưu trữ các enemy rảnh rỗi
    private Queue<GameObject> enemyPool = new Queue<GameObject>();

    private void Awake()
    {
        // Khởi tạo Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Khởi tạo bể enemy
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            // Tạo enemy và đưa vào hàng đợi
            GameObject enemy = Instantiate(enemyPrefab, transform);
            enemy.SetActive(false); // Vô hiệu hóa ngay lập tức
            enemyPool.Enqueue(enemy);
        }
    }

    // Phương thức để lấy một enemy từ bể
    public GameObject GetEnemy()
    {
        if (enemyPool.Count > 0)
        {
            // Lấy enemy từ đầu hàng đợi và kích hoạt nó
            GameObject enemy = enemyPool.Dequeue();
            enemy.SetActive(true);
            return enemy;
        }
        else
        {
            // Nếu bể trống, tạo thêm enemy mới (có thể mở rộng bể)
            Debug.LogWarning("Enemy pool is empty! Creating a new enemy.");
            GameObject newEnemy = Instantiate(enemyPrefab, transform);
            return newEnemy;
        }
    }

    // Phương thức để trả một enemy về bể
    public void ReturnEnemyToPool(GameObject enemy)
    {
        // Vô hiệu hóa và trả enemy về hàng đợi
        enemy.SetActive(false);
        enemyPool.Enqueue(enemy);
    }
}