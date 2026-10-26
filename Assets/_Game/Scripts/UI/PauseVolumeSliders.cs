using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Thanh kéo "Nhạc nền" và "Hiệu ứng" trên bảng tạm dừng trong trận, giống bảng Cài đặt ở menu.
/// Vị trí / kích thước chỉnh tay trong Hierarchy: mỗi ô (MusicSlider, SfxSlider) là chỗ đặt thanh kéo,
/// hình thanh kéo được dựng vào ô lúc chạy. Kéo về 0 chỉ hạ âm lượng nên nhạc không bị phát lại từ đầu.
/// </summary>
public class PauseVolumeSliders : MonoBehaviour
{
    [Tooltip("Ô đặt thanh kéo nhạc nền (thanh giãn theo ô)")]
    [SerializeField] private RectTransform musicSlot;
    [Tooltip("Ô đặt thanh kéo hiệu ứng âm thanh")]
    [SerializeField] private RectTransform sfxSlot;
    [Tooltip("Chữ phần trăm cạnh thanh nhạc nền (có thể để trống)")]
    [SerializeField] private TextMeshProUGUI musicPercent;
    [Tooltip("Chữ phần trăm cạnh thanh hiệu ứng (có thể để trống)")]
    [SerializeField] private TextMeshProUGUI sfxPercent;

    private Slider musicSlider;
    private Slider sfxSlider;

    private void Awake()
    {
        musicSlider = Build(musicSlot, GameSettings.MusicVolume, musicPercent, GameSettings.SetMusicVolume);
        sfxSlider = Build(sfxSlot, GameSettings.SfxVolume, sfxPercent, GameSettings.SetSfxVolume);
    }

    // Mỗi lần mở bảng: lấy lại giá trị (có thể đã đổi ở menu)
    private void OnEnable()
    {
        Sync(musicSlider, musicPercent, GameSettings.MusicVolume);
        Sync(sfxSlider, sfxPercent, GameSettings.SfxVolume);
    }

    private static Slider Build(RectTransform slot, float value, TextMeshProUGUI percent, System.Action<float> onChanged)
    {
        if (slot == null) return null;
        Slider slider = RuntimeUI.CreateVolumeSlider(slot, value, slot.rect.height > 1f ? slot.rect.height : 54f);
        slider.onValueChanged.AddListener(v =>
        {
            if (percent != null) percent.text = Percent(v);
            onChanged(v);
        });
        if (percent != null) percent.text = Percent(value);
        return slider;
    }

    private static void Sync(Slider slider, TextMeshProUGUI percent, float value)
    {
        if (slider != null) slider.SetValueWithoutNotify(value);
        if (percent != null) percent.text = Percent(value);
    }

    private static string Percent(float v) => Mathf.RoundToInt(v * 100f) + "%";
}
