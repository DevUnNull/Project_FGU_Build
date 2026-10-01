using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Quản lý bàn cờ Grid & Danh sách tất cả các ô TileCell.
/// Cung cấp API tìm kiếm ô Tile chính xác theo vị trí chuột, quản lý bận/rỗng & reset highlight.
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    public int rows = 5;
    public int cols = 9;
    public float cellSize = 1.5f;

    [Header("Obstacles")]
    public GameObject toxinObstaclePrefab;
    private bool[,] gridStatus; // true = occupied, false = free

    private List<TileCell> allTiles = new List<TileCell>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        gridStatus = new bool[rows, cols];
    }

    private void Start()
    {
        RefreshTileCache();
        SpawnToxinObstacles();

        // Tự động gắn và hiển thị báo cáo ngày
        if (gameObject.GetComponent<DayReportUI>() == null)
        {
            gameObject.AddComponent<DayReportUI>();
        }
    }

    public void RefreshTileCache()
    {
        allTiles.Clear();
        TileCell[] tiles = FindObjectsOfType<TileCell>();
        allTiles.AddRange(tiles);
    }

    /// <summary>
    /// Tìm ô TileCell nằm bên dưới hoặc gần nhất với vị trí con trỏ chuột 2D.
    /// </summary>
    public TileCell GetTileAtWorldPosition(Vector3 worldPos)
    {
        worldPos.z = 0f;

        // 1. Raycast / OverlapPoint trực tiếp ngay dưới con trỏ chuột
        Collider2D hitCol = Physics2D.OverlapPoint(worldPos);
        if (hitCol != null)
        {
            TileCell tile = hitCol.GetComponent<TileCell>() ?? hitCol.GetComponentInParent<TileCell>();
            if (tile != null) return tile;
        }

        // 2. Tìm TileCell có tâm gần nhất với vị trí con trỏ chuột (Bán kính 1.8f)
        TileCell closestTile = null;
        float shortestDist = Mathf.Infinity;

        if (allTiles.Count == 0) RefreshTileCache();

        foreach (var tile in allTiles)
        {
            if (tile == null || !tile.gameObject.activeInHierarchy) continue;

            float dist = Vector2.Distance(worldPos, tile.transform.position);
            if (dist < 1.8f && dist < shortestDist)
            {
                shortestDist = dist;
                closestTile = tile;
            }
        }

        return closestTile;
    }

    /// <summary>
    /// Kiểm tra ô TileCell có hợp lệ & rỗng để đặt Tế bào không.
    /// </summary>
    public bool IsTileFree(TileCell tile)
    {
        if (tile == null || tile.isEdge || tile.IsOccupied) return false;
        return true;
    }

    /// <summary>
    /// Đánh dấu ô TileCell bận (đang chứa Tế bào).
    /// </summary>
    public void OccupyTile(TileCell tile, GameObject unit)
    {
        if (tile != null)
        {
            tile.Occupy(unit);
            SetCellOccupied(tile.row, tile.col, true);
        }
    }

    /// <summary>
    /// Giải phóng ô TileCell thành rỗng.
    /// </summary>
    public void FreeTile(TileCell tile)
    {
        if (tile != null)
        {
            tile.Free();
            SetCellOccupied(tile.row, tile.col, false);
        }
    }

    /// <summary>
    /// Clear toàn bộ màu highlight của tất cả ô Tile trên bàn cờ.
    /// </summary>
    public void ClearAllHighlights()
    {
        if (allTiles.Count == 0) RefreshTileCache();

        foreach (var tile in allTiles)
        {
            if (tile != null)
            {
                tile.ResetHighlight();
            }
        }
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
                if (c > 0)
                {
                    freeCells.Add(new Vector2Int(r, c));
                }
            }
        }

        for (int i = 0; i < obstaclesToSpawn; i++)
        {
            if (freeCells.Count == 0) break;

            int randomIndex = Random.Range(0, freeCells.Count);
            Vector2Int pos = freeCells[randomIndex];
            freeCells.RemoveAt(randomIndex);

            gridStatus[pos.x, pos.y] = true;
            
            Vector3 worldPos = GetWorldPosition(pos.x, pos.y);
            Instantiate(toxinObstaclePrefab, worldPos, Quaternion.identity, transform);
        }
    }

    public Vector3 GetWorldPosition(int row, int col)
    {
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
