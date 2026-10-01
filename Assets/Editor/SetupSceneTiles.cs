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
            if (name.StartsWith("Tile") && !name.Contains("Edge"))
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
