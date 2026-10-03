using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SetupProjectFonts
{
    [MenuItem("Tools/Setup All Project Fonts (Apply Baloo2-Bold Custom Font)")]
    public static void SetupBalooFont()
    {
        string ttfPath = "Assets/ImageCallOurMyCell/fontChu/Baloo_2,Chewy/Baloo_2/static/Baloo2-Bold.ttf";
        string fontAssetPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/Baloo2-Bold SDF.asset";

        CreateAndApplyCustomFont(ttfPath, fontAssetPath, "Baloo2-Bold SDF");
    }

    [MenuItem("Tools/Setup All Project Fonts (Apply Chewy Custom Font)")]
    public static void SetupChewyFont()
    {
        string ttfPath = "Assets/ImageCallOurMyCell/fontChu/Baloo_2,Chewy/Chewy/Chewy-Regular.ttf";
        string fontAssetPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/Chewy SDF.asset";

        CreateAndApplyCustomFont(ttfPath, fontAssetPath, "Chewy SDF");
    }

    [MenuItem("Tools/Setup All Project Fonts (Fallback to LiberationSans)")]
    public static void SetupLiberationFont()
    {
        TMP_FontAsset libFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (libFont == null)
        {
            Debug.LogError("Không tìm thấy Font LiberationSans SDF!");
            return;
        }

        ApplyFontToAllTexts(libFont, "LiberationSans SDF");
    }

    private static void CreateAndApplyCustomFont(string ttfPath, string fontAssetPath, string fontDisplayName)
    {
        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
        if (sourceFont == null)
        {
            Debug.LogError($"Không tìm thấy file Font .ttf tại đường dẫn: {ttfPath}");
            return;
        }

        AssetDatabase.DeleteAsset(fontAssetPath);

        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);
        if (fontAsset == null)
        {
            Debug.LogError($"Không thể tạo TMP_FontAsset cho {fontDisplayName}!");
            return;
        }

        fontAsset.name = fontDisplayName;

        Material mat = new Material(Shader.Find("TextMeshPro/Distance Field"));
        mat.name = fontAsset.name + " Material";
        mat.mainTexture = fontAsset.atlasTexture;

        AssetDatabase.CreateAsset(fontAsset, fontAssetPath);
        AssetDatabase.AddObjectToAsset(mat, fontAsset);

        if (fontAsset.atlasTexture != null)
        {
            fontAsset.atlasTexture.name = fontAsset.name + " Atlas";
            AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
        }

        fontAsset.material = mat;

        SerializedObject fontSo = new SerializedObject(fontAsset);
        fontSo.Update();

        SerializedProperty matProp = fontSo.FindProperty("m_Material");
        if (matProp != null)
        {
            matProp.objectReferenceValue = mat;
        }

        SerializedProperty atlasProp = fontSo.FindProperty("m_AtlasTextures");
        if (atlasProp != null && fontAsset.atlasTexture != null)
        {
            atlasProp.arraySize = 1;
            atlasProp.GetArrayElementAtIndex(0).objectReferenceValue = fontAsset.atlasTexture;
        }

        fontSo.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(fontAsset);
        EditorUtility.SetDirty(mat);
        if (fontAsset.atlasTexture != null) EditorUtility.SetDirty(fontAsset.atlasTexture);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        TMP_FontAsset loadedFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath);
        ApplyFontToAllTexts(loadedFont, fontDisplayName);
    }

    private static void ApplyFontToAllTexts(TMP_FontAsset targetFont, string fontName)
    {
        if (targetFont == null || targetFont.material == null) return;

        // 1. Update TMP Settings default font
        TMP_Settings settings = AssetDatabase.LoadAssetAtPath<TMP_Settings>("Assets/TextMesh Pro/Resources/TMP Settings.asset");
        if (settings != null)
        {
            SerializedObject so = new SerializedObject(settings);
            SerializedProperty defaultFontProp = so.FindProperty("m_defaultFontAsset");
            if (defaultFontProp != null)
            {
                defaultFontProp.objectReferenceValue = targetFont;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(settings);
            }
        }

        int fixedSceneTextCount = 0;
        int fixedPrefabTextCount = 0;

        // 2. Fix active open scene
        TextMeshProUGUI[] activeTmps = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var tmp in activeTmps)
        {
            Undo.RecordObject(tmp, "Set Custom TMP Font");
            tmp.font = targetFont;
            tmp.fontSharedMaterial = targetFont.material;
            EditorUtility.SetDirty(tmp);
            fixedSceneTextCount++;
        }

        if (activeTmps.Length > 0)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        // 3. Fix Prefabs in project (StatImpactPopupPrefab, ChoiceButton, DayReportCanvas...)
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.Contains("VisualNovelSystem") && !path.Contains("_Scrip") && !path.Contains("_Prefabs")) continue;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            TextMeshProUGUI[] prefabTmps = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
            if (prefabTmps != null && prefabTmps.Length > 0)
            {
                bool prefabModified = false;
                foreach (var tmp in prefabTmps)
                {
                    Undo.RecordObject(tmp, "Set Custom TMP Font Prefab");
                    tmp.font = targetFont;
                    tmp.fontSharedMaterial = targetFont.material;
                    EditorUtility.SetDirty(tmp);
                    fixedPrefabTextCount++;
                    prefabModified = true;
                }
                if (prefabModified)
                {
                    PrefabUtility.SavePrefabAsset(prefab);
                }
            }
        }

        AssetDatabase.SaveAssets();
        Canvas.ForceUpdateCanvases();

        Debug.Log($"[SetupProjectFonts SUCCESS] Đã áp dụng Font nghệ thuật {fontName} cho {fixedSceneTextCount} Text trong Scene và {fixedPrefabTextCount} Text trong Prefabs!");
    }
}
