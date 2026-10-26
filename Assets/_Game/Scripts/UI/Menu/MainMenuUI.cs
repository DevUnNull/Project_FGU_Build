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

    /// <summary>Đặt true trước khi về Menu chính để mở thẳng bảng Nâng cấp (nút "Đi nâng cấp" ở MainMap).</summary>
    public static bool OpenUpgradesOnLoad;

    private TextMeshProUGUI upgradeText;
    private Vector3 upgradeBaseScale = Vector3.one;
    private bool pulsing;
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
        upgradeBaseScale = upgradeButton.transform.localScale;
        RefreshUpgradeLabel();
    }

    private void Start()
    {
        if (OpenUpgradesOnLoad)
        {
            OpenUpgradesOnLoad = false;
            UpgradeUI.Open();
        }
    }

    private void Update()
    {
        // Còn sao chưa dùng -> nút Nâng cấp nhún nhẹ để người chơi chú ý
        // (chỉ đụng tới scale khi đang nhún, để hiệu ứng hover của UIButtonEffect vẫn hoạt động)
        if (upgradeButton != null)
        {
            if (shownStars > 0)
            {
                upgradeButton.transform.localScale = upgradeBaseScale * (1f + 0.06f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 3f)));
                pulsing = true;
            }
            else if (pulsing)
            {
                upgradeButton.transform.localScale = upgradeBaseScale;
                pulsing = false;
            }
        }

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
