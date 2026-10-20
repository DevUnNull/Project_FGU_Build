using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

public static class SetupChoicesHorizontalLayout
{
    [MenuItem("Tools/Visual Novel/Convert Choices to Horizontal Layout")]
    public static void ConvertToHorizontal()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas trong Scene!");
            return;
        }

        Transform choicesTr = canvas.transform.Find("ChoicesContainer");
        if (choicesTr == null)
        {
            Debug.LogError("Không tìm thấy ChoicesContainer trong Canvas!");
            return;
        }

        GameObject choicesObj = choicesTr.gameObject;
        Undo.RegisterCompleteObjectUndo(choicesObj, "Convert to Horizontal Layout");

        // Gỡ bỏ VerticalLayoutGroup nếu có
        VerticalLayoutGroup vlg = choicesObj.GetComponent<VerticalLayoutGroup>();
        if (vlg != null)
        {
            Object.DestroyImmediate(vlg);
        }

        // Thêm / cấu hình HorizontalLayoutGroup
        HorizontalLayoutGroup hlg = choicesObj.GetComponent<HorizontalLayoutGroup>();
        if (hlg == null)
        {
            hlg = choicesObj.AddComponent<HorizontalLayoutGroup>();
        }

        hlg.spacing = 25f;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        // Đặt kích thước và vị trí hàng ngang
        RectTransform rect = choicesObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 290f);
        rect.sizeDelta = new Vector2(1600f, 150f);

        EditorUtility.SetDirty(choicesObj);
        EditorSceneManager.MarkSceneDirty(choicesObj.scene);
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("<color=green>[ChoicesContainer]</color> Đã chuyển đổi ChoicesContainer sang Horizontal Layout (Hàng ngang) thành công!");
    }
}
