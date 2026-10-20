using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tạo object "BattleHUD" trong scene đang mở để chỉnh tay vị trí các nút HUD trong Hierarchy:
/// Button_x1, Button_Help (?), Button_CallAllWaves, ToastArea, ItemBarArea.
/// Lúc chạy, BattleHUD đổi các nút sang khung viên thuốc và gắn sự kiện bấm (không cần gán OnClick).
/// </summary>
public static class BattleHudSceneTool
{
    private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

    [MenuItem("Tools/Battle HUD/Create HUD Objects In Open Scene")]
    public static void Create()
    {
        if (Object.FindFirstObjectByType<BattleHUD>() != null)
        {
            EditorUtility.DisplayDialog("Battle HUD", "Scene đã có BattleHUD.", "OK");
            return;
        }

        var root = new GameObject("BattleHUD", typeof(RectTransform));
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 300;
        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        root.AddComponent<GraphicRaycaster>();
        BattleHUD hud = root.AddComponent<BattleHUD>();

        Button speed = CreateButton(root.transform, "Button_x1", "x1", new Vector2(150f, 80f), BattleHUD.DefaultSpeedPosition);
        Button help = CreateButton(root.transform, "Button_Help", "?", new Vector2(100f, 80f), BattleHUD.DefaultHelpPosition);
        Button callAll = CreateButton(root.transform, "Button_CallAllWaves", "Gọi wave sớm", new Vector2(260f, 80f), BattleHUD.DefaultCallAllPosition);

        RectTransform toast = CreateArea(root.transform, "ToastArea", new Vector2(0.5f, 1f), BattleHUD.DefaultToastSize, BattleHUD.DefaultToastPosition);
        RectTransform itemBar = CreateArea(root.transform, "ItemBarArea", Center, new Vector2(104f, 104f), BattleHUD.DefaultItemBarPosition);

        var so = new SerializedObject(hud);
        so.FindProperty("speedButtonRef").objectReferenceValue = speed;
        so.FindProperty("helpButtonRef").objectReferenceValue = help;
        so.FindProperty("callAllButtonRef").objectReferenceValue = callAll;
        so.FindProperty("toastArea").objectReferenceValue = toast;
        so.FindProperty("itemBarArea").objectReferenceValue = itemBar;
        so.ApplyModifiedPropertiesWithoutUndo();

        Undo.RegisterCreatedObjectUndo(root, "Create Battle HUD");
        EditorSceneManager.MarkSceneDirty(root.scene);
        Selection.activeGameObject = root;
    }

    internal static Button CreateButton(Transform parent, string name, string label, Vector2 size, Vector2 position)
    {
        var go = new GameObject(name, typeof(RectTransform));
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = Center;
        rt.sizeDelta = size;
        rt.anchoredPosition = position;

        // Ảnh xem trước trong Editor; lúc chạy được thay bằng khung viên thuốc cắt 9-slice
        Image img = go.AddComponent<Image>();
        if (UISkin.Instance != null) img.sprite = UISkin.Instance.pillButton;
        Button button = go.AddComponent<Button>();
        button.targetGraphic = img;

        var textGo = new GameObject("Label", typeof(RectTransform));
        RectTransform textRt = (RectTransform)textGo.transform;
        textRt.SetParent(rt, false);
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = textRt.offsetMax = Vector2.zero;
        TextMeshProUGUI text = textGo.AddComponent<TextMeshProUGUI>();
        if (RuntimeUI.Font != null) text.font = RuntimeUI.Font;
        text.text = label;
        text.fontSize = size.y * 0.42f;
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return button;
    }

    private static RectTransform CreateArea(Transform parent, string name, Vector2 anchor, Vector2 size, Vector2 position)
    {
        var go = new GameObject(name, typeof(RectTransform));
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.sizeDelta = size;
        rt.anchoredPosition = position;
        return rt;
    }
}
