using UnityEditor;
using UnityEngine;

/// <summary>
/// Tool tự động sửa Pivot của Sprite Sheet CellB về Tâm (0.5, 0.5) 
/// giúp nhân vật luôn hiển thị CHÍNH XÁC VÀO GIỮA TÂM TILE khi kéo thả.
/// </summary>
public class FixSpritePivots
{
    [MenuItem("Tools/Fix CellB Sprite Pivots to Center")]
    [InitializeOnLoadMethod]
    public static void FixPivots()
    {
        string[] spritePaths = new string[]
        {
            "Assets/ImageCallOurMyCell/animation/CellB/Bộ sprite bốn khung robot pháo xanh.png",
            "Assets/ImageCallOurMyCell/animation/CellB/CellAtcak.png",
            "Assets/ImageCallOurMyCell/Cell/TẾ BÀO B.png"
        };

        bool changedAny = false;
        foreach (string path in spritePaths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.isReadable = true;
                bool fileChanged = false;

                if (importer.spriteImportMode == SpriteImportMode.Multiple)
                {
                    SpriteMetaData[] metaData = importer.spritesheet;
                    for (int i = 0; i < metaData.Length; i++)
                    {
                        if (metaData[i].pivot != new Vector2(0.5f, 0.5f))
                        {
                            metaData[i].alignment = (int)SpriteAlignment.Center;
                            metaData[i].pivot = new Vector2(0.5f, 0.5f);
                            fileChanged = true;
                        }
                    }
                    if (fileChanged)
                    {
                        importer.spritesheet = metaData;
                    }
                }
                else if (importer.spriteImportMode == SpriteImportMode.Single)
                {
                    TextureImporterSettings settings = new TextureImporterSettings();
                    importer.ReadTextureSettings(settings);
                    if (settings.spritePivot != new Vector2(0.5f, 0.5f))
                    {
                        settings.spriteAlignment = (int)SpriteAlignment.Center;
                        settings.spritePivot = new Vector2(0.5f, 0.5f);
                        importer.SetTextureSettings(settings);
                        fileChanged = true;
                    }
                }

                if (fileChanged)
                {
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    changedAny = true;
                    Debug.Log($"🎯 [FixSpritePivots] Đã chỉnh Sprite Pivot của '{path}' về chính giữa Tâm (0.5, 0.5)");
                }
            }
        }

        if (changedAny)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
