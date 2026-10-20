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

        // HUD trận đấu: nút x2, thông báo, viền cảnh báo
        BattleHUD.Ensure(gameObject);
    }

    public void RefreshTileCache()
    {
        allTiles.Clear();
        TileCell[] tiles = FindObjectsOfType<TileCell>();
        allTiles.AddRange(tiles);
        boardTiles = null;
        cellSizeReady = false;
    }

    /// <summary>
    /// Tìm ô TileCell mà vị trí thả nằm BÊN TRONG (theo kích thước ô thật trên bàn cờ).
    /// Thả ra ngoài bàn cờ (VD: vẫn ở khung shop) -> trả về null, không tự hút vào ô gần nhất.
    /// </summary>
    public TileCell GetTileAtWorldPosition(Vector3 worldPos)
    {
        Vector2 half = GetCellSize() * 0.5f;
        TileCell closestTile = null;
        float shortestDist = Mathf.Infinity;

        foreach (var tile in GetBoardTiles())
        {
            Vector2 delta = (Vector2)(worldPos - tile.CenterPosition);
            if (Mathf.Abs(delta.x) > half.x || Mathf.Abs(delta.y) > half.y) continue;

            float dist = delta.sqrMagnitude;
            if (dist < shortestDist)
            {
                shortestDist = dist;
                closestTile = tile;
            }
        }

        return closestTile;
    }

    private Vector2 cellSizeCache;
    private bool cellSizeReady;

    /// <summary>
    /// Kích thước 1 ô = khoảng cách trung vị giữa các cột / các hàng liền kề (ô được đặt tay nên không đều tuyệt đối).
    /// </summary>
    public Vector2 GetCellSize()
    {
        if (cellSizeReady) return cellSizeCache;

        var colX = new SortedDictionary<int, List<float>>();
        var rowY = new SortedDictionary<int, List<float>>();
        foreach (var tile in GetBoardTiles())
        {
            if (!colX.ContainsKey(tile.col)) colX[tile.col] = new List<float>();
            if (!rowY.ContainsKey(tile.row)) rowY[tile.row] = new List<float>();
            colX[tile.col].Add(tile.CenterPosition.x);
            rowY[tile.row].Add(tile.CenterPosition.y);
        }

        float width = MedianStep(colX);
        float height = MedianStep(rowY);
        cellSizeCache = new Vector2(width > 0f ? width : cellSize, height > 0f ? height : cellSize);
        cellSizeReady = true;
        return cellSizeCache;
    }

    private static float MedianStep(SortedDictionary<int, List<float>> groups)
    {
        List<float> centers = new List<float>();
        foreach (var g in groups.Values)
        {
            float sum = 0f;
            foreach (float v in g) sum += v;
            centers.Add(sum / g.Count);
        }

        List<float> steps = new List<float>();
        for (int i = 1; i < centers.Count; i++) steps.Add(Mathf.Abs(centers[i] - centers[i - 1]));
        if (steps.Count == 0) return 0f;
        steps.Sort();
        return steps[steps.Count / 2];
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
    private List<TileCell> boardTiles;

    public List<TileCell> GetBoardTiles()
    {
        if (allTiles.Count == 0) RefreshTileCache();
        if (boardTiles != null) return boardTiles;

        List<TileCell> result = new List<TileCell>();
        foreach (var tile in allTiles)
        {
            if (tile == null || !tile.gameObject.activeInHierarchy) continue;
            if (tile.GetComponentsInChildren<TileCell>(true).Length > 1) continue;
            result.Add(tile);
        }
        boardTiles = result;
        return boardTiles;
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
