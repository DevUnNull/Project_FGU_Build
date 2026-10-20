using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Các bảng mở từ MainMenuScene (gán vào OnClick của nút trong Inspector):
/// BT_KhamPha → Sổ tay sức khỏe, BT_XemNgay → Nhật ký các ngày, BT_Setting → Cài đặt, BT_NangCap → Nâng cấp.
/// Nút Nâng cấp hiện số sao chưa dùng, VD "Nâng cấp (2)".
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Tooltip("Nút Nâng cấp. Nếu ảnh nút đang là countWave (ảnh tạm) thì lúc chạy tự đổi sang khung viên thuốc 9-slice")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private string upgradeLabel = "Nâng cấp";

    private TextMeshProUGUI upgradeText;
    private int shownStars = -1;
    private float refreshTimer;

    private void Awake()
    {
        if (upgradeButton == null) return;

        Image img = upgradeButton.targetGraphic as Image;
        if (img != null && UISkin.Instance != null && img.sprite == UISkin.Instance.pillButton)
        {
            RuntimeUI.ApplyPillStyle(upgradeButton);
        }
        upgradeText = upgradeButton.GetComponentInChildren<TextMeshProUGUI>();
        RefreshUpgradeLabel();
    }

    private void Update()
    {
        // Cập nhật số sao chưa dùng sau khi mua / đặt lại nâng cấp
        refreshTimer -= Time.unscaledDeltaTime;
        if (refreshTimer > 0f) return;
        refreshTimer = 0.5f;
        RefreshUpgradeLabel();
    }

    private void RefreshUpgradeLabel()
    {
        if (upgradeText == null) return;
        int stars = UpgradeProgress.AvailableStars;
        if (stars == shownStars) return;
        shownStars = stars;
        upgradeText.text = stars > 0 ? $"{upgradeLabel} ({stars})" : upgradeLabel;
    }

    public void OpenNotebook() => NotebookUI.Open();
    public void OpenDayJournal() => DayJournalUI.Open();
    public void OpenSettings() => SettingsUI.Open();
    public void OpenUpgrades() => UpgradeUI.Open();
}
