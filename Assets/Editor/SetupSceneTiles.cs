using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Editor script tự động gắn TileCell & BoxCollider2D lên tất cả 35 ô Tile trong Scene.
/// </summary>
public class SetupSceneTiles
{
    [MenuItem("Tools/Setup Scene TileCells & Colliders")]
    [InitializeOnLoadMethod]
    public static void SetupTiles()
    {
        var allTransforms = Object.FindObjectsOfType<Transform>();
        int count = 0;

        foreach (var t in allTransforms)
        {
            if (t == null) continue;
            string name = t.name;

            // Dọn TileCell & Collider bị gắn nhầm lên object cha "Tiles" (ô giả nằm ngoài bàn cờ)
            if (name == "Tiles")
            {
                TileCell strayCell = t.GetComponent<TileCell>();
                if (strayCell != null)
                {
                    Object.DestroyImmediate(strayCell);
                    BoxCollider2D strayCol = t.GetComponent<BoxCollider2D>();
                    if (strayCol != null) Object.DestroyImmediate(strayCol);
                    t.gameObject.tag = "Untagged";
                    Debug.Log("🧹 [Tools] Đã gỡ TileCell & BoxCollider2D gắn nhầm trên object cha 'Tiles'.");
                }
                continue;
            }

            // Bỏ qua object cha "Tiles" (chứa các ô con), chỉ xử lý từng ô Tile
            if (name.StartsWith("Tile") && !name.Contains("Edge") && name != "Tiles")
            {
                // 1. Gắn hoặc cập nhật TileCell
                TileCell cell = t.GetComponent<TileCell>();
                if (cell == null)
                {
                    cell = t.gameObject.AddComponent<TileCell>();
                }

                // 2. Gắn hoặc cập nhật BoxCollider2D
                BoxCollider2D col = t.GetComponent<BoxCollider2D>();
                if (col == null)
                {
                    col = t.gameObject.AddComponent<BoxCollider2D>();
                }
                col.isTrigger = true;
                col.size = new Vector2(1.4f, 1.4f); // Kích thước phủ kín bề mặt ô Tile

                // 3. Gắn Tag Tile
                if (!t.CompareTag("Tile") && !t.CompareTag("TileEdge"))
                {
                    t.tag = "Tile";
                }

                count++;
            }
        }

        if (GridManager.Instance != null)
        {
            GridManager.Instance.RefreshTileCache();
        }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log($"✅ [Tools] Đã tự động cấu hình TileCell & BoxCollider2D cho {count} ô Tile trong Scene!");
    }
}
