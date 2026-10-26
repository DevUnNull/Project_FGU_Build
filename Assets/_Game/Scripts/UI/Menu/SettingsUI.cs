using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bảng "CÀI ĐẶT" (MainMenuScene → BT_Setting): âm lượng tổng, âm lượng hiệu ứng, tốc độ trận mặc định,
/// xem lại hướng dẫn, chơi lại từ đầu (có hỏi xác nhận).
/// </summary>
public class SettingsUI : MonoBehaviour
{
    private const float RowHeight = 84f;
    private const float LabelWidth = 400f;
    private const float IconSize = 72f;
    private const float SliderHeight = 54f;
    private static readonly Color RowColor = new Color(1f, 0.97f, 0.88f, 0.9f);

    private RectTransform list;
    private Canvas canvas;

    public static void Open()
    {
        Canvas canvas = RuntimeUI.CreateOverlayCanvas("Settings", 450);
        canvas.gameObject.AddComponent<SettingsUI>().Build(canvas);
    }

    private void Build(Canvas c)
    {
        canvas = c;
        CanvasGroup group = canvas.gameObject.AddComponent<CanvasGroup>();
        RectTransform root = RuntimeUI.Stretch(canvas.transform, "Backdrop");
        RuntimeUI.AddImage(root, RuntimeUI.Backdrop);

        RectTransform card = RuntimeUI.CreateCard(root, new Vector2(1150f, 860f), "CÀI ĐẶT", RuntimeUI.HeaderGreen, out RectTransform body);
        list = RuntimeUI.CreateRect(body, "List", Vector2.zero, Vector2.one, new Vector2(0f, 100f), Vector2.zero);

        int row = 0;
        UISkin skin = UISkin.Instance;
        AddSliderRow(row++, "Âm lượng tổng", null, GameSettings.MasterVolume, GameSettings.SetMasterVolume);
        AddSliderRow(row++, "Nhạc nền", skin != null ? skin.musicIcon : null, GameSettings.MusicVolume, GameSettings.SetMusicVolume);
        AddSliderRow(row++, "Hiệu ứng âm thanh", skin != null ? skin.sfxIcon : null, GameSettings.SfxVolume, GameSettings.SetSfxVolume);

        RectTransform speedRow = CreateRow(row++, "Tốc độ trận mặc định");
        Button speed = CreateRowButton(speedRow, SpeedLabel(), RuntimeUI.HeaderGreen, null);
        TextMeshProUGUI speedText = speed.GetComponentInChildren<TextMeshProUGUI>();
        speed.onClick.AddListener(() =>
        {
            GameSpeed.SetPreferred(GameSpeed.Preferred > 1.5f ? 1f : 2f);
            speedText.text = SpeedLabel();
        });

        RectTransform tutorialRow = CreateRow(row++, "Hướng dẫn chơi");
        Button tutorial = CreateRowButton(tutorialRow, "Xem lại hướng dẫn", RuntimeUI.HeaderOrange, null);
        TextMeshProUGUI tutorialText = tutorial.GetComponentInChildren<TextMeshProUGUI>();
        tutorial.onClick.AddListener(() =>
        {
            GameSettings.ResetTutorials();
            tutorialText.text = "Sẽ hiện lại ở trận sau";
            tutorial.interactable = false;
        });

        RectTransform resetRow = CreateRow(row++, "Tiến trình");
        CreateRowButton(resetRow, "Chơi lại từ đầu", RuntimeUI.HeaderRed, ConfirmReset);

        if (GameSettings.IsTestMuted && Debug.isDebugBuild)
        {
            RectTransform note = RuntimeUI.CreateRect(list, "TestMuteNote", new Vector2(0f, 1f), Vector2.one,
                new Vector2(10f, -(row * (RowHeight + 10f)) - 60f), new Vector2(-10f, -(row * (RowHeight + 10f))));
            RuntimeUI.CreateText(note, "Bản thử nghiệm đang tắt toàn bộ âm thanh (GlobalAudioMuter.isTestMuteActive = true).",
                22f, RuntimeUI.NegativeText, TextAlignmentOptions.MidlineLeft);
        }

        Button close = RuntimeUI.CreateButton(body, "Đóng", RuntimeUI.HeaderGreen, new Vector2(240f, 76f), () => Destroy(canvas.gameObject));
        RectTransform closeRt = (RectTransform)close.transform;
        closeRt.anchorMin = closeRt.anchorMax = new Vector2(0.5f, 0f);
        closeRt.anchoredPosition = new Vector2(0f, 40f);

        StartCoroutine(RuntimeUI.PopIn(group, card));
    }

    private static string SpeedLabel() => GameSpeed.Preferred > 1.5f ? "Nhanh (x2)" : "Bình thường (x1)";

    private RectTransform CreateRow(int index, string label, Sprite icon = null)
    {
        float top = -index * (RowHeight + 10f);
        RectTransform row = RuntimeUI.CreateRect(list, "Row_" + label, new Vector2(0f, 1f), Vector2.one,
            new Vector2(0f, top - RowHeight), new Vector2(0f, top));
        RuntimeUI.AddImage(row, RowColor, false);

        float textLeft = 28f;
        if (icon != null)
        {
            RectTransform iconRt = RuntimeUI.CreateRect(row, "Icon", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
            iconRt.sizeDelta = new Vector2(IconSize, IconSize);
            iconRt.anchoredPosition = new Vector2(24f + IconSize * 0.5f, 0f);
            Image img = RuntimeUI.AddImage(iconRt, Color.white, false);
            img.sprite = icon;
            img.preserveAspect = true;
            textLeft = 34f + IconSize;
        }

        RectTransform labelArea = RuntimeUI.CreateRect(row, "Label", Vector2.zero, new Vector2(0f, 1f), new Vector2(textLeft, 0f), new Vector2(LabelWidth, 0f));
        TextMeshProUGUI text = RuntimeUI.CreateText(labelArea, label, 32f, RuntimeUI.TextDark, TextAlignmentOptions.MidlineLeft);
        text.fontStyle = FontStyles.Bold;
        text.enableAutoSizing = true;
        text.fontSizeMin = 22f;
        text.fontSizeMax = 32f;
        return row;
    }

    private static Button CreateRowButton(RectTransform row, string label, Color color, Action onClick)
    {
        Button button = RuntimeUI.CreateButton(row, label, color, new Vector2(380f, 72f), onClick);
        RectTransform rt = (RectTransform)button.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 0.5f);
        rt.anchoredPosition = new Vector2(-220f, 0f);
        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
        text.enableAutoSizing = true;
        text.fontSizeMin = 18f;
        text.fontSizeMax = 30f;
        return button;
    }

    private void AddSliderRow(int index, string label, Sprite icon, float value, Action<float> onChanged)
    {
        RectTransform row = CreateRow(index, label, icon);

        RectTransform percentArea = RuntimeUI.CreateRect(row, "Percent", new Vector2(1f, 0f), Vector2.one, new Vector2(-110f, 0f), new Vector2(-20f, 0f));
        TextMeshProUGUI percent = RuntimeUI.CreateText(percentArea, Percent(value), 30f, RuntimeUI.TextDark, TextAlignmentOptions.MidlineRight);

        RectTransform sliderRt = RuntimeUI.CreateRect(row, "Slider", new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(LabelWidth + 30f, -SliderHeight * 0.5f), new Vector2(-140f, SliderHeight * 0.5f));
        Slider slider = RuntimeUI.CreateVolumeSlider(sliderRt, value, SliderHeight);
        slider.onValueChanged.AddListener(v =>
        {
            percent.text = Percent(v);
            onChanged(v);
        });
    }

    private static string Percent(float v) => Mathf.RoundToInt(v * 100f) + "%";

    private void ConfirmReset()
    {
        Canvas confirm = RuntimeUI.CreateOverlayCanvas("ConfirmReset", 460);
        RectTransform root = RuntimeUI.Stretch(confirm.transform, "Backdrop");
        RuntimeUI.AddImage(root, RuntimeUI.Backdrop);
        RectTransform card = RuntimeUI.CreateCard(root, new Vector2(900f, 460f), "CHƠI LẠI TỪ ĐẦU?", RuntimeUI.HeaderRed, out RectTransform body);

        RectTransform textArea = RuntimeUI.CreateRect(body, "Text", Vector2.zero, Vector2.one, new Vector2(10f, 100f), new Vector2(-10f, 0f));
        RuntimeUI.CreateText(textArea, "Các ngày sẽ khóa lại, sao, nâng cấp và Sổ tay sức khỏe bị xóa. Cài đặt âm thanh được giữ nguyên.",
            28f, RuntimeUI.TextDark);

        Button yes = RuntimeUI.CreateButton(body, "Xóa và chơi lại", RuntimeUI.HeaderRed, new Vector2(320f, 72f), () =>
        {
            GameSettings.ResetProgress();
            Destroy(confirm.gameObject);
        });
        RectTransform yesRt = (RectTransform)yes.transform;
        yesRt.anchorMin = yesRt.anchorMax = new Vector2(0.5f, 0f);
        yesRt.anchoredPosition = new Vector2(-180f, 40f);

        Button no = RuntimeUI.CreateButton(body, "Hủy", RuntimeUI.HeaderGreen, new Vector2(240f, 72f), () => Destroy(confirm.gameObject));
        RectTransform noRt = (RectTransform)no.transform;
        noRt.anchorMin = noRt.anchorMax = new Vector2(0.5f, 0f);
        noRt.anchoredPosition = new Vector2(180f, 40f);
    }
}
