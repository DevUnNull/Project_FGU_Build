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
    [Tooltip("Hình ảnh ô Độc tố (dùng khi toxinObstaclePrefab không có SpriteRenderer)")]
    public Sprite toxinObstacleSprite;
    [Tooltip("Kích thước hiển thị của ô Độc tố (đơn vị world)")]
    public float toxinObstacleSize = 1.5f;
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

        // Tự động sử dụng DayReportUI đã có trong Scene (hoặc gắn thêm nếu chưa có)
        if (FindObjectOfType<DayReportUI>() == null)
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

    /// <summary>
    /// Danh sách các ô thật trên bàn cờ (bỏ qua object cha chứa các ô con bị gắn nhầm TileCell).
    /// </summary>
    public List<TileCell> GetBoardTiles()
    {
        if (allTiles.Count == 0) RefreshTileCache();

        List<TileCell> result = new List<TileCell>();
        foreach (var tile in allTiles)
        {
            if (tile == null || !tile.gameObject.activeInHierarchy) continue;
            if (tile.GetComponentsInChildren<TileCell>(true).Length > 1) continue;
            result.Add(tile);
        }
        return result;
    }

    private void SpawnToxinObstacles()
    {
        if (StomachDayData.Instance == null || toxinObstaclePrefab == null) return;

        int obstaclesToSpawn = Mathf.RoundToInt(StomachDayData.Instance.toxicityLevel);
        if (obstaclesToSpawn <= 0) return;

        // Khóa ngẫu nhiên các ô thật còn trống (trừ cột 0 để người chơi luôn có chỗ đặt phòng thủ)
        List<TileCell> freeTiles = new List<TileCell>();
        foreach (var tile in GetBoardTiles())
        {
            if (tile.col > 0 && IsTileFree(tile)) freeTiles.Add(tile);
        }

        for (int i = 0; i < obstaclesToSpawn && freeTiles.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, freeTiles.Count);
            TileCell tile = freeTiles[randomIndex];
            freeTiles.RemoveAt(randomIndex);

            GameObject obstacle = Instantiate(toxinObstaclePrefab, tile.CenterPosition, Quaternion.identity, transform);
            obstacle.SetActive(true);
            EnsureToxinVisual(obstacle);
            OccupyTile(tile, obstacle);
        }
    }

    private void EnsureToxinVisual(GameObject obstacle)
    {
        if (toxinObstacleSprite == null || obstacle.GetComponentInChildren<SpriteRenderer>() != null) return;

        SpriteRenderer sr = obstacle.AddComponent<SpriteRenderer>();
        sr.sprite = toxinObstacleSprite;
        sr.sortingOrder = 2; // Trên ô Tile, dưới Tế bào & quái
        sr.color = new Color(0.75f, 0.85f, 0.45f, 0.95f);

        float spriteWidth = Mathf.Max(0.01f, toxinObstacleSprite.bounds.size.x);
        float scale = toxinObstacleSize / spriteWidth;
        obstacle.transform.localScale = new Vector3(scale, scale, 1f);
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
