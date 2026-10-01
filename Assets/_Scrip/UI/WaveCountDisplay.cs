using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Script hiển thị số lượng wave (Wave hiện tại / Tổng số wave / Số wave còn lại).
/// Tự động gắn vào UI Text (TextMeshProUGUI hoặc UI Text) trong CanvasUi/CountWave/textBackground/Text.
/// </summary>
public class WaveCountDisplay : MonoBehaviour
{
    public enum DisplayMode
    {
        CurrentOverTotal,  // Ví dụ: 3/10 (Wave hiện tại / Tổng số wave)
        RemainingOnly,      // Ví dụ: 7 (Số wave còn lại)
        CurrentOnly,        // Ví dụ: 3
        CustomFormat        // Ví dụ: "WAVE {current}/{total}" hoặc "Còn {remaining} Wave"
    }

    [Header("UI Text Components (Auto-found if null)")]
    [Tooltip("Component TextMeshProUGUI hiển thị chữ")]
    public TextMeshProUGUI tmpText;

    [Tooltip("Component Legacy UI Text (nếu không dùng TextMeshPro)")]
    public Text legacyText;

    [Header("Display Settings")]
    [Tooltip("Kiểu hiển thị Wave")]
    public DisplayMode displayMode = DisplayMode.CurrentOverTotal;

    [Tooltip("Định dạng chữ tùy chỉnh (Dùng {current}, {total}, {remaining})")]
    public string customFormat = "{current}/{total}";

    private void Awake()
    {
        AutoFindTextComponent();
    }

    private void OnEnable()
    {
        AutoFindTextComponent();
        RegisterEvents();
        UpdateWaveUI();
    }

    private void OnDisable()
    {
        UnregisterEvents();
    }

    private void Start()
    {
        AutoFindTextComponent();
        RegisterEvents();
        UpdateWaveUI();
    }

    public void AutoFindTextComponent()
    {
        if (tmpText == null) tmpText = GetComponent<TextMeshProUGUI>();
        if (tmpText == null) tmpText = GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText == null && legacyText == null) legacyText = GetComponent<Text>();
        if (legacyText == null && tmpText == null) legacyText = GetComponentInChildren<Text>();
    }

    private void RegisterEvents()
    {
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveStart -= HandleWaveStart;
            WaveManager.Instance.OnWaveStart += HandleWaveStart;
            WaveManager.Instance.OnWaveProgressChanged -= HandleWaveProgressChanged;
            WaveManager.Instance.OnWaveProgressChanged += HandleWaveProgressChanged;
        }
    }

    private void UnregisterEvents()
    {
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveStart -= HandleWaveStart;
            WaveManager.Instance.OnWaveProgressChanged -= HandleWaveProgressChanged;
        }
    }

    private void HandleWaveStart(WaveConfig waveConfig)
    {
        UpdateWaveUI();
    }

    private void HandleWaveProgressChanged(int currentWave, int totalWaves)
    {
        UpdateWaveUI();
    }

    /// <summary>
    /// Cập nhật hiển thị số Wave lên UI
    /// </summary>
    public void UpdateWaveUI()
    {
        AutoFindTextComponent();
        if (WaveManager.Instance == null) return;

        int current = WaveManager.Instance.GetCurrentGlobalWaveIndex();
        int total = WaveManager.Instance.GetTotalWaveCount();
        int remaining = WaveManager.Instance.GetRemainingWaveCount();

        string displayText = "";

        switch (displayMode)
        {
            case DisplayMode.CurrentOverTotal:
                displayText = $"{current}/{total}";
                break;

            case DisplayMode.RemainingOnly:
                displayText = $"{remaining}";
                break;

            case DisplayMode.CurrentOnly:
                displayText = $"{current}";
                break;

            case DisplayMode.CustomFormat:
                displayText = customFormat
                    .Replace("{current}", current.ToString())
                    .Replace("{total}", total.ToString())
                    .Replace("{remaining}", remaining.ToString());
                break;
        }

        SetText(displayText);
    }

    private void SetText(string content)
    {
        if (tmpText != null)
        {
            tmpText.text = content;
        }
        else if (legacyText != null)
        {
            legacyText.text = content;
        }
    }
}
