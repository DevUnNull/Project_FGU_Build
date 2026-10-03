using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;

public class ApplyBalooToScenes
{
    [MenuItem("Tools/Apply Baloo2-Bold Font To Open Scene")]
    public static void ApplyToOpenScene()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/Baloo2-Bold SDF.asset");
        Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/TextMesh Pro/Resources/Fonts & Materials/Baloo2-Bold SDF Material.mat");

        if (font == null || mat == null)
        {
            Debug.LogError("Font asset hoặc Material không tìm thấy!");
            return;
        }

        TextMeshProUGUI[] tmps = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var tmp in tmps)
        {
            Undo.RecordObject(tmp, "Apply Baloo Font");
            tmp.font = font;
            tmp.fontSharedMaterial = mat;
            EditorUtility.SetDirty(tmp);
        }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Canvas.ForceUpdateCanvases();
        Debug.Log($"[ApplyBalooToScenes] Applied Baloo2-Bold font to {tmps.Length} text components in active scene!");
    }
}
