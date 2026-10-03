using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class CreateStatImpactPopupPrefab
{
    [MenuItem("Tools/Visual Novel/Create Stat Impact Popup Prefab")]
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

        string prefabPath = prefabsDir + "/StatImpactPopupPrefab.prefab";

        // Tạo Canvas tạm thời để cấu hình UI
        GameObject tempCanvas = new GameObject("TempCanvas", typeof(Canvas));

        // 1. Root GameObject (StatImpactPopup)
        GameObject root = new GameObject("StatImpactPopup", typeof(RectTransform), typeof(CanvasGroup), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter), typeof(StatImpactPopup));
        root.transform.SetParent(tempCanvas.transform, false);

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(280f, 50f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);

        HorizontalLayoutGroup hlg = root.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10f;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        ContentSizeFitter csf = root.GetComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 2. Child Icon Image
        GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObj.transform.SetParent(root.transform, false);
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(38f, 38f);

        // 3. Child Text
        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(root.transform, false);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(200f, 45f);

        TextMeshProUGUI tmpText = textObj.GetComponent<TextMeshProUGUI>();
        tmpText.text = "+10 Energy";
        tmpText.fontSize = 24;
        tmpText.fontStyle = FontStyles.Bold;
        tmpText.color = new Color(0.2f, 1f, 0.3f, 1f); // Green
        tmpText.alignment = TextAlignmentOptions.MidlineLeft;

        // Bật nét viền đen cho chữ dễ nhìn trên mọi nền
        tmpText.outlineWidth = 0.2f;
        tmpText.outlineColor = Color.black;

        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/Baloo2-Bold SDF.asset");
        if (font != null) tmpText.font = font;

        // Link các tham chiếu vào script StatImpactPopup
        StatImpactPopup popupScript = root.GetComponent<StatImpactPopup>();
        popupScript.iconImage = iconObj.GetComponent<Image>();
        popupScript.textUI = tmpText;
        popupScript.canvasGroup = root.GetComponent<CanvasGroup>();

        // 4. Lưu thành Prefab
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(tempCanvas);

        AssetDatabase.Refresh();
        Debug.Log("<color=green>[StatImpactPopup]</color> Đã tạo Prefab mẫu thành công tại: " + prefabPath);

        Object prefabObj = AssetDatabase.LoadAssetAtPath<Object>(prefabPath);
        if (prefabObj != null)
        {
            EditorGUIUtility.PingObject(prefabObj);
            Selection.activeObject = prefabObj;
        }
    }
}
