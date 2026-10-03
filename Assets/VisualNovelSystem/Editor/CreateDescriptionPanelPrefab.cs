using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class CreateDescriptionPanelPrefab
{
    [MenuItem("Tools/Visual Novel/Create Description Panel Prefab")]
    public static void CreatePrefab()
    {
        string prefabsDir = "Assets/VisualNovelSystem/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabsDir))
        {
            if (!AssetDatabase.IsValidFolder("Assets/VisualNovelSystem"))
            {
                AssetDatabase.CreateFolder("Assets", "VisualNovelSystem");
            }
            AssetDatabase.CreateFolder("Assets/VisualNovelSystem", "Prefabs");
        }

        string prefabPath = prefabsDir + "/DescriptionPanelTemplate.prefab";

        // 1. Tạo GameObject Gốc (DescriptionPanel)
        GameObject root = new GameObject("DescriptionPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(950f, 220f);
        rootRect.anchorMin = new Vector2(0.5f, 0f);
        rootRect.anchorMax = new Vector2(0.5f, 0f);
        rootRect.pivot = new Vector2(0.5f, 0f);
        rootRect.anchoredPosition = new Vector2(0f, 50f);

        Image bgImage = root.GetComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.88f); // Màu nền tối mờ sang trọng
        
        Sprite defaultSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        if (defaultSprite != null)
        {
            bgImage.sprite = defaultSprite;
            bgImage.type = Image.Type.Sliced;
        }

        // 2. Tạo Vùng Chứa Chữ (DescText) nằm con bên trong với Padding (Lề)
        GameObject textObj = new GameObject("DescText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(root.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        // Stretch tràn viền nhưng thụt lề: Trai 50, Phai 50, Tren 30, Duoi 30
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.offsetMin = new Vector2(50f, 30f);   // Left = 50, Bottom = 30
        textRect.offsetMax = new Vector2(-50f, -30f); // Right = 50, Top = 30

        TextMeshProUGUI tmpText = textObj.GetComponent<TextMeshProUGUI>();
        tmpText.text = "Đây là khung thoại mô tả mẫu. Bạn có thể kéo thả Prefab này vào Canvas và chỉnh sửa Sprite nền hoặc lề chữ (Left/Right/Top/Bottom) theo ý muốn!";
        tmpText.fontSize = 24;
        tmpText.color = Color.white;
        tmpText.alignment = TextAlignmentOptions.MidlineLeft;
        tmpText.enableWordWrapping = true;

        // Gán font Baloo2-Bold SDF mặc định
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/Baloo2-Bold SDF.asset");
        if (font != null)
        {
            tmpText.font = font;
        }

        // 3. Lưu thành Prefab
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.Refresh();
        Debug.Log("<color=green>[Visual Novel System]</color> Đã tạo Prefab mẫu Description Panel thành công tại: " + prefabPath);

        // Highlight Prefab trong cửa sổ Project
        Object prefabObj = AssetDatabase.LoadAssetAtPath<Object>(prefabPath);
        if (prefabObj != null)
        {
            EditorGUIUtility.PingObject(prefabObj);
            Selection.activeObject = prefabObj;
        }
    }
}
