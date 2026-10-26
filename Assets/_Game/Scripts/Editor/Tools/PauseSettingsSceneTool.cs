using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bảng tạm dừng trong trận (Ingame_1, LuTile): thay 3 nút bật/tắt nhạc, âm thanh, ngôn ngữ bằng 2 thanh kéo
/// "Nhạc nền" / "Hiệu ứng" (PauseVolumeSliders) giống bảng Cài đặt ở menu, và đổi nhạc nền trận đấu.
/// Các dòng nằm trong Hierarchy (PauseGame/BoardMenu/VolumeRows) để chỉnh tay. Chạy lại an toàn.
/// </summary>
public static class PauseSettingsSceneTool
{
    private static readonly string[] Scenes =
    {
        "Assets/_Game/Scenes/Ingame_1.unity",
        "Assets/_Game/Scenes/LuTile.unity",
    };

    private const string BattleMusic = "Assets/_Game/Audio/Music/Battle_TowerDefense.mp3";
    private const string SettingArt = "Assets/_Game/Art/UI/Setting/";
    private static readonly string[] OldButtons = { "SoundEffect", "SoundMusic", "Button (Legacy) (2)" };

    private const float RowWidth = 780f;
    private const float RowHeight = 80f;
    private const float SliderHeight = 46f;

    [MenuItem("Tools/Battle/Setup Pause Volume Sliders")]
    public static void Setup()
    {
        var log = new StringBuilder();
        foreach (string path in Scenes)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            log.AppendLine(path + ": " + SetupScene());
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        Debug.Log("[PauseSettingsSceneTool]\n" + log);
    }

    private static string SetupScene()
    {
        var result = new StringBuilder();

        AudioClip music = AssetDatabase.LoadAssetAtPath<AudioClip>(BattleMusic);
        foreach (SceneUIController controller in Object.FindObjectsOfType<SceneUIController>(true))
        {
            AudioSource source = controller.GetComponent<AudioSource>();
            if (source == null || music == null) continue;
            source.clip = music;
            source.loop = true;
            EditorUtility.SetDirty(source);
            result.Append("nhạc trận → " + music.name + "; ");
        }

        Transform board = null;
        foreach (PauseMenu menu in Object.FindObjectsOfType<PauseMenu>(true))
        {
            if (menu.name == "PauseGame") board = menu.transform.Find("BoardMenu");
        }
        if (board == null) return result.Append("không có PauseGame/BoardMenu").ToString();

        foreach (string name in OldButtons)
        {
            Transform old = board.Find(name);
            if (old == null) continue;
            Object.DestroyImmediate(old.gameObject);
            result.Append("xóa " + name + "; ");
        }

        Transform existing = board.Find("VolumeRows");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        var rows = new GameObject("VolumeRows", typeof(RectTransform)).GetComponent<RectTransform>();
        rows.SetParent(board, false);
        rows.anchoredPosition = new Vector2(-20f, 30f);
        rows.sizeDelta = new Vector2(RowWidth, RowHeight * 2f + 20f);

        PauseVolumeSliders sliders = rows.gameObject.AddComponent<PauseVolumeSliders>();
        var so = new SerializedObject(sliders);
        CreateRow(rows, "MusicRow", SettingArt + "Music.png", RowHeight * 0.5f + 10f, out RectTransform musicSlot, out TextMeshProUGUI musicPercent);
        CreateRow(rows, "SfxRow", SettingArt + "EFX.png", -RowHeight * 0.5f - 10f, out RectTransform sfxSlot, out TextMeshProUGUI sfxPercent);
        so.FindProperty("musicSlot").objectReferenceValue = musicSlot;
        so.FindProperty("sfxSlot").objectReferenceValue = sfxSlot;
        so.FindProperty("musicPercent").objectReferenceValue = musicPercent;
        so.FindProperty("sfxPercent").objectReferenceValue = sfxPercent;
        so.ApplyModifiedPropertiesWithoutUndo();

        return result.Append("thêm VolumeRows").ToString();
    }

    // Dòng: [biểu tượng] [thanh kéo ........] [100%]
    private static void CreateRow(RectTransform parent, string name, string iconPath, float y, out RectTransform slot, out TextMeshProUGUI percent)
    {
        RectTransform row = NewRect(name, parent);
        row.anchoredPosition = new Vector2(0f, y);
        row.sizeDelta = new Vector2(RowWidth, RowHeight);

        RectTransform icon = NewRect("Icon", row);
        icon.anchorMin = icon.anchorMax = new Vector2(0f, 0.5f);
        icon.sizeDelta = new Vector2(RowHeight, RowHeight);
        icon.anchoredPosition = new Vector2(RowHeight * 0.5f, 0f);
        Image iconImg = icon.gameObject.AddComponent<Image>();
        iconImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
        iconImg.preserveAspect = true;
        iconImg.raycastTarget = false;

        slot = NewRect(name == "MusicRow" ? "MusicSlider" : "SfxSlider", row);
        slot.anchorMin = new Vector2(0f, 0.5f);
        slot.anchorMax = new Vector2(1f, 0.5f);
        slot.offsetMin = new Vector2(RowHeight + 40f, -SliderHeight * 0.5f);
        slot.offsetMax = new Vector2(-130f, SliderHeight * 0.5f);

        RectTransform percentRt = NewRect("Percent", row);
        percentRt.anchorMin = new Vector2(1f, 0f);
        percentRt.anchorMax = Vector2.one;
        percentRt.offsetMin = new Vector2(-110f, 0f);
        percentRt.offsetMax = Vector2.zero;
        percent = percentRt.gameObject.AddComponent<TextMeshProUGUI>();
        percent.font = RuntimeUI.Font;
        percent.text = "100%";
        percent.fontSize = 36f;
        percent.fontStyle = FontStyles.Bold;
        percent.color = RuntimeUI.TextDark;
        percent.alignment = TextAlignmentOptions.MidlineRight;
        percent.raycastTarget = false;
    }

    private static RectTransform NewRect(string name, Transform parent)
    {
        var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.gameObject.layer = parent.gameObject.layer;
        return rt;
    }
}
