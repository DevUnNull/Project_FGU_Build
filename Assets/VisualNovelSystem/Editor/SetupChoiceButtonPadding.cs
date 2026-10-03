using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class SetupChoiceButtonPadding
{
    [MenuItem("Tools/Visual Novel/Adjust Choice Button Text Padding")]
    public static void AdjustPadding()
    {
        string path = "Assets/VisualNovelSystem/Prefabs/ChoiceButton.prefab";
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
        if (prefabRoot == null)
        {
            Debug.LogError("Không tìm thấy ChoiceButton.prefab tại: " + path);
            return;
        }

        TextMeshProUGUI tmp = prefabRoot.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            RectTransform textRect = tmp.rectTransform;
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            
            // Đặt lề (Padding): Trái 70px (né cuộn trái), Phải 85px (né cuộn phải & ngôi sao), Trên/Dưới 12px
            textRect.offsetMin = new Vector2(70f, 12f);   // Left = 70, Bottom = 12
            textRect.offsetMax = new Vector2(-85f, -12f); // Right = 85, Top = 12

            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 13f;
            tmp.fontSizeMax = 22f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;
        }

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
        PrefabUtility.UnloadPrefabContents(prefabRoot);
        AssetDatabase.Refresh();

        Debug.Log("<color=green>[ChoiceButton]</color> Đã cập nhật lề chữ cho ChoiceButton.prefab thành công!");
    }
}
