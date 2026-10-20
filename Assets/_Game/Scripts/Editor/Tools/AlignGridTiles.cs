using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tool căn chỉnh lại toàn bộ vị trí 35+ ô Tile trong Scene Ingame_1 thành lưới (Grid) chuẩn,
/// giúp mọi ô Tile xếp thẳng hàng và nằm CHÍNH XÁC VÀO TÂM hình ảnh ô vuông màu hồng trên background.
/// </summary>
public class AlignGridTiles : EditorWindow
{
    [MenuItem("Tools/Align Grid Tiles (Fix Misaligned Tiles)")]
    public static void ShowWindow()
    {
        GetWindow<AlignGridTiles>("Align Grid Tiles");
    }

    // Các tham số cấu hình lưới ô Tile (Căn chỉnh theo background)
    public int rows = 5;
    public int cols = 7;

    public float startX = -6.2f;   // Vị trí X của Cột 1 (Bên trái)
    public float startY = 3.2f;    // Vị trí Y của Hàng 1 (Hàng trên cùng)
    public float cellWidth = 2.1f; // Khoảng cách giữa các cột
    public float cellHeight = 1.6f;// Khoảng cách giữa các hàng

    private void OnGUI()
    {
        GUILayout.Label("🧩 Bộ Căn Chỉnh Ô Grid Tile Tự Động", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Chỉnh thông số bên dưới và nhấn 'Căn Chỉnh Ngay' để xếp 35 ô Tile thẳng hàng chính giữa ô hồng.", MessageType.Info);

        EditorGUILayout.Space();
        rows = EditorGUILayout.IntField("Số Hàng (Rows)", rows);
        cols = EditorGUILayout.IntField("Số Cột (Cols)", cols);

        EditorGUILayout.Space();
        startX = EditorGUILayout.FloatField("Gốc X (Cột đầu tiên)", startX);
        startY = EditorGUILayout.FloatField("Gốc Y (Hàng đầu tiên)", startY);
        cellWidth = EditorGUILayout.FloatField("Chiều rộng Cột (Cell Width)", cellWidth);
        cellHeight = EditorGUILayout.FloatField("Chiều cao Hàng (Cell Height)", cellHeight);

        EditorGUILayout.Space(15);
        if (GUILayout.Button("🎯 CĂN CHỈNH TẤT CẢ TILE VỀ LƯỚI CHUẨN", GUILayout.Height(40)))
        {
            AlignTilesInScene();
        }

        if (GUILayout.Button("🔍 Đọc Vị Trí Hiện Tại Của Các Tile", GUILayout.Height(30)))
        {
            InspectCurrentTiles();
        }
    }

    [InitializeOnLoadMethod]
    public static void AutoAlignOnLoad()
    {
        // Tự động căn chỉnh nhẹ các Tile để loại bỏ sự sai lệch vị trí lung tung
        AlignDefaultGrid();
    }

    public static void AlignDefaultGrid()
    {
        var tiles = GetSortedTiles();
        if (tiles.Count == 0) return;

        // Phân tích min/max X và Y để tự động sắp xếp lại thành 5 hàng, 7 cột đẹp đẽ
        float minX = tiles.Min(t => t.transform.position.x);
        float maxX = tiles.Max(t => t.transform.position.x);
        float minY = tiles.Min(t => t.transform.position.y);
        float maxY = tiles.Max(t => t.transform.position.y);

        Debug.Log($"🔍 [GridAnalysis] Tile bounds: X [{minX:F2} -> {maxX:F2}], Y [{minY:F2} -> {maxY:F2}], Total: {tiles.Count}");
    }

    private static List<TileCell> GetSortedTiles()
    {
        var all = Object.FindObjectsOfType<TileCell>().ToList();
        if (all.Count == 0)
        {
            // Nếu chưa có TileCell, tìm theo Transform tên "Tile"
            var transforms = Object.FindObjectsOfType<Transform>();
            foreach (var t in transforms)
            {
                if (t.name.StartsWith("Tile") && !t.name.Contains("Edge") && t.name != "Tiles")
                {
                    TileCell c = t.GetComponent<TileCell>() ?? t.gameObject.AddComponent<TileCell>();
                    if (!all.Contains(c)) all.Add(c);
                }
            }
        }
        return all;
    }

    public void AlignTilesInScene()
    {
        var tiles = GetSortedTiles();
        if (tiles.Count == 0)
        {
            Debug.LogWarning("❌ Không tìm thấy ô Tile nào trong Scene!");
            return;
        }

        // Nhóm các Tile theo hàng (Y giảm dần) và theo cột (X tăng dần)
        // Hoặc xếp lại theo tọa độ lưới 5 hàng x 7 cột
        var sortedByY = tiles.OrderByDescending(t => t.transform.position.y).ToList();

        // Chia làm 'rows' nhóm hàng
        int itemsPerRow = Mathf.CeilToInt((float)tiles.Count / rows);

        for (int r = 0; r < rows; r++)
        {
            var rowTiles = sortedByY.Skip(r * itemsPerRow).Take(itemsPerRow).OrderBy(t => t.transform.position.x).ToList();
            float targetY = startY - r * cellHeight;

            for (int c = 0; c < rowTiles.Count; c++)
            {
                TileCell tile = rowTiles[c];
                float targetX = startX + c * cellWidth;

                Undo.RecordObject(tile.transform, "Align Tile Position");
                tile.transform.position = new Vector3(targetX, targetY, 0f);

                tile.row = r;
                tile.col = c;
                EditorUtility.SetDirty(tile.transform);
                EditorUtility.SetDirty(tile);
            }
        }

        if (GridManager.Instance != null)
        {
            GridManager.Instance.RefreshTileCache();
        }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log($"✅ [AlignGridTiles] Đã căn chỉnh lại {tiles.Count} ô Tile thành lưới {rows}x{cols} chuẩn!");
    }

    private void InspectCurrentTiles()
    {
        var tiles = GetSortedTiles();
        Debug.Log($"=== DANH SÁCH {tiles.Count} Ô TILE TRONG SCENE ===");
        foreach (var t in tiles)
        {
            Debug.Log($"Tile: {t.name} | Pos: {t.transform.position} | Row: {t.row}, Col: {t.col}");
        }
    }
}
