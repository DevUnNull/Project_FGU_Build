using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    public int rows = 5;
    public int cols = 9;
    public float cellSize = 1.5f;

    [Header("Obstacles")]
    public GameObject toxinObstaclePrefab;
    private bool[,] gridStatus; // true = occupied, false = free

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        gridStatus = new bool[rows, cols];
    }

    private void Start()
    {
        SpawnToxinObstacles();
    }

    private void SpawnToxinObstacles()
    {
        if (StomachDayData.Instance == null || toxinObstaclePrefab == null) return;

        int obstaclesToSpawn = StomachDayData.Instance.toxinObstaclesCount;
        List<Vector2Int> freeCells = new List<Vector2Int>();

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                // Giữ lại cột 0 cho Tanker hoặc Tế bào quan trọng, không spawn vật cản ở cột đầu tiên
                if (c > 0)
                {
                    freeCells.Add(new Vector2Int(r, c));
                }
            }
        }

        // Randomly pick cells
        for (int i = 0; i < obstaclesToSpawn; i++)
        {
            if (freeCells.Count == 0) break;

            int randomIndex = Random.Range(0, freeCells.Count);
            Vector2Int pos = freeCells[randomIndex];
            freeCells.RemoveAt(randomIndex);

            gridStatus[pos.x, pos.y] = true; // Mark as occupied
            
            // Calculate world position based on grid
            Vector3 worldPos = GetWorldPosition(pos.x, pos.y);
            Instantiate(toxinObstaclePrefab, worldPos, Quaternion.identity, transform);
        }
    }

    public Vector3 GetWorldPosition(int row, int col)
    {
        // Tính toán vị trí thế giới dựa trên gốc toạ độ của GridManager
        return transform.position + new Vector3(col * cellSize, -row * cellSize, 0);
    }

    public bool IsCellOccupied(int row, int col)
    {
        if (row < 0 || row >= rows || col < 0 || col >= cols) return true;
        return gridStatus[row, col];
    }

    public void SetCellOccupied(int row, int col, bool occupied)
    {
        if (row >= 0 && row < rows && col >= 0 && col < cols)
        {
            gridStatus[row, col] = occupied;
        }
    }
}
