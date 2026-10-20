using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Gắn MainMenuUI vào Canvas của MainMenuScene (đang mở) và nối OnClick:
/// BT_KhamPha → Sổ tay, BT_XemNgay → Nhật ký các ngày, BT_Setting → Cài đặt.
/// Tạo thêm nút BT_NangCap (→ Nâng cấp) nếu chưa có, để chỉnh vị trí trong Hierarchy.
/// </summary>
public static class MainMenuSetupTool
{
    private static readonly Vector2 UpgradeButtonPosition = new Vector2(760f, 440f);
    private static readonly Vector2 UpgradeButtonSize = new Vector2(300f, 100f);

    [MenuItem("Tools/Main Menu/Setup Menu Buttons In Open Scene")]
    public static void Setup()
    {
        GameObject khamPha = GameObject.Find("BT_KhamPha");
        if (khamPha == null)
        {
            EditorUtility.DisplayDialog("Main Menu", "Hãy mở MainMenuScene trước (không tìm thấy BT_KhamPha).", "OK");
            return;
        }

        Transform canvas = khamPha.transform.parent;
        MainMenuUI menu = canvas.GetComponent<MainMenuUI>();
        if (menu == null) menu = Undo.AddComponent<MainMenuUI>(canvas.gameObject);

        Rewire(khamPha, menu.OpenNotebook);
        Rewire(GameObject.Find("BT_XemNgay"), menu.OpenDayJournal);
        Rewire(GameObject.Find("BT_Setting"), menu.OpenSettings);

        GameObject upgradeGo = GameObject.Find("BT_NangCap");
        if (upgradeGo == null)
        {
            Button created = BattleHudSceneTool.CreateButton(canvas, "BT_NangCap", "Nâng cấp", UpgradeButtonSize, UpgradeButtonPosition);
            created.gameObject.AddComponent<UIButtonEffect>();
            Undo.RegisterCreatedObjectUndo(created.gameObject, "Create BT_NangCap");
            upgradeGo = created.gameObject;
        }
        Rewire(upgradeGo, menu.OpenUpgrades);

        var so = new SerializedObject(menu);
        so.FindProperty("upgradeButton").objectReferenceValue = upgradeGo.GetComponent<Button>();
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        Selection.activeGameObject = upgradeGo;
    }

    /// <summary>Xóa OnClick cũ (đều đang chuyển sang MainMap) và gán hàm mới, hiện trong Inspector.</summary>
    private static void Rewire(GameObject go, UnityAction action)
    {
        if (go == null) return;
        Button button = go.GetComponent<Button>();
        if (button == null) return;

        Undo.RecordObject(button, "Rewire menu button");
        for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            UnityEventTools.RemovePersistentListener(button.onClick, i);
        }
        UnityEventTools.AddVoidPersistentListener(button.onClick, action);
        EditorUtility.SetDirty(button);
    }
}
