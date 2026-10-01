using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tool căn chỉnh lại toàn bộ 35 ô Tile thành LƯỚI HOÀN HẢO (Perfect 5x7 Grid),
/// khắc phục triệt để lỗi "tile thì đứng chỗ này, tile thì đứng chỗ kia lung tung".
/// </summary>
public class SetupPerfectTileGrid : EditorWindow
{
    [MenuItem("Tools/Re-align All Tiles to Perfect Grid")]
    public static void ShowWindow()
    {
        GetWindow<SetupPerfectTileGrid>("Align Tile Grid");
    }

    [Header("Cấu hình Lưới 5x7 chuẩn")]
    public int rows = 5;
    public int cols = 7;

    // Tọa độ Thế giới (World Position) của ô Gốc (Hàng 0, Cột 0 - Top Left)
    public float startX = -4.40f;
    public float startY = 2.80f;

    // Khoảng cách đều giữa các Cột và Hàng
    public float stepX = 1.90f;
    public float stepY = 1.60f;

    private void OnGUI()
    {
        GUILayout.Label("📐 CĂN CHỈNH TILE THÀNH LƯỚI ĐỀU 100%", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Sử dụng công cụ này để sửa tất cả 35 ô Tile đang bị xếp lệch lung tung trong Scene Ingame_1 về một lưới đều đặn tuyệt đối.", MessageType.Info);

        EditorGUILayout.Space();
        rows = EditorGUILayout.IntField("Số Hàng (Rows)", rows);
        cols = EditorGUILayout.IntField("Số Cột (Cols)", cols);

        EditorGUILayout.Space();
        startX = EditorGUILayout.FloatField("Vị trí X Cột 1 (Top-Left X)", startX);
        startY = EditorGUILayout.FloatField("Vị trí Y Hàng 1 (Top-Left Y)", startY);

        EditorGUILayout.Space();
        stepX = EditorGUILayout.FloatField("Khoảng cách Cột (Step X)", stepX);
        stepY = EditorGUILayout.FloatField("Khoảng cách Hàng (Step Y)", stepY);

        EditorGUILayout.Space(15);
        if (GUILayout.Button("⚡ CĂN CHỈNH NGAY TẤT CẢ TILE", GUILayout.Height(45)))
        {
            AlignAllTiles();
        }
    }

    [InitializeOnLoadMethod]
    public static void AutoExecuteOnLoad()
    {
        AlignTilesWithParameters(-4.40f, 2.80f, 1.90f, 1.60f, 5, 7);
    }

    public void AlignAllTiles()
    {
        AlignTilesWithParameters(startX, startY, stepX, stepY, rows, cols);
    }

    public static void AlignTilesWithParameters(float sX, float sY, float stX, float stY, int numRows, int numCols)
    {
        // 1. Tìm tất cả GameObject có tên "Tile (" hoặc thuộc parent "Tiles"
        List<GameObject> tileObjs = new List<GameObject>();
        GameObject tilesParent = GameObject.Find("Tiles");

        if (tilesParent != null)
        {
            foreach (Transform child in tilesParent.transform)
            {
                if (child.name.StartsWith("Tile"))
                {
                    tileObjs.Add(child.gameObject);
                }
            }
        }

        if (tileObjs.Count == 0)
        {
            var allTransforms = Object.FindObjectsOfType<Transform>();
            foreach (var t in allTransforms)
            {
                if (t.name.StartsWith("Tile") && !t.name.Contains("Edge") && t.name != "Tiles")
                {
                    if (!tileObjs.Contains(t.gameObject)) tileObjs.Add(t.gameObject);
                }
            }
        }

        if (tileObjs.Count == 0)
        {
            Debug.LogWarning("❌ Không tìm thấy ô Tile nào trong Scene để căn chỉnh!");
            return;
        }

        // 2. Sắp xếp danh sách Tile theo vị trí Y (từ trên xuống dưới)
        var sortedByY = tileObjs.OrderByDescending(go => go.transform.position.y).ToList();

        int itemsPerRow = numCols;
        int countAligned = 0;

        for (int r = 0; r < numRows; r++)
        {
            // Lấy các Tile thuộc hàng r và sắp xếp theo X (từ trái sang phải)
            var rowItems = sortedByY.Skip(r * itemsPerRow).Take(itemsPerRow).OrderBy(go => go.transform.position.x).ToList();

            float targetY = sY - r * stY;

            for (int c = 0; c < rowItems.Count; c++)
            {
                GameObject tileGo = rowItems[c];
                float targetX = sX + c * stX;

                Undo.RecordObject(tileGo.transform, "Align Grid Tile");
                tileGo.transform.position = new Vector3(targetX, targetY, 0f);

                // Gắn/Cập nhật TileCell
                TileCell cell = tileGo.GetComponent<TileCell>();
                if (cell == null) cell = tileGo.AddComponent<TileCell>();
                cell.row = r;
                cell.col = c;
                EditorUtility.SetDirty(cell);

                // Gắn/Cập nhật BoxCollider2D vừa vặn ô
                BoxCollider2D col = tileGo.GetComponent<BoxCollider2D>();
                if (col == null) col = tileGo.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(stX * 0.9f, stY * 0.9f);
                EditorUtility.SetDirty(col);

                EditorUtility.SetDirty(tileGo.transform);
                countAligned++;
            }
        }

        if (GridManager.Instance != null)
        {
            GridManager.Instance.RefreshTileCache();
        }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log($"✅ [SetupPerfectTileGrid] Đã căn chỉnh chính xác {countAligned} ô Tile thành Lưới {numRows}x{numCols} đều 100%!");
    }
}
