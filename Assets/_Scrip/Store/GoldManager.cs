using TMPro;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance;

    public int gold = 10;

    [Header("UI")]
    public TextMeshProUGUI goldText;  // ← kéo GoldText vào đây trong Inspector

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Energy (Thể lực): Quyết định lượng ATP khởi điểm khi vào trận
        if (StomachDayData.Instance != null)
        {
            gold = Mathf.RoundToInt(gold * StomachDayData.Instance.energyMultiplier);
        }
        UpdateGoldUI();
    }

    private float atpTimer = 0f;

    [Header("=== Cấu Hình Tăng ATP/Tiền Theo Thời Gian ===")]
    [Tooltip("Khoảng thời gian (tính bằng giây) giữa mỗi đợt nhận ATP (Mặc định: 1 giây)")]
    public float baseAtpGenerationRate = 1f;

    [Tooltip("Số tiền ATP nhận được mỗi đợt (Mặc định: 5 ATP)")]
    public int atpPerTick = 5;

    [Tooltip("Tích chọn nếu chỉ bắt đầu tăng tiền khi người chơi ấn nút GỌI WAVE (StartWave)")]
    public bool requireStartWave = true;

    private void Update()
    {
        // 1. Nếu bật requireStartWave -> Chỉ tăng tiền khi WaveManager đã khởi chạy Wave!
        if (requireStartWave)
        {
            if (WaveManager.Instance == null || !WaveManager.Instance.IsWaveStarted)
            {
                return; // Chưa ấn GỌI WAVE -> Chưa bắt đầu tăng ATP theo thời gian
            }
        }

        // 2. Tính toán hệ số năng lượng và tăng tiền
        float multiplier = 1.0f;
        if (StomachDayData.Instance != null)
        {
            multiplier = StomachDayData.Instance.energyMultiplier;
        }

        if (multiplier > 0)
        {
            float actualRate = baseAtpGenerationRate / multiplier;
            atpTimer += Time.deltaTime;
            if (atpTimer >= actualRate)
            {
                atpTimer -= actualRate;
                AddGold(atpPerTick);
            }
        }
    }

    public bool HasEnoughGold(int amount)
    {
        return gold >= amount;
    }

    public void SpendGold(int amount)
    {
        gold -= amount;
        UpdateGoldUI();
    }

    public void AddGold(int amount)
    {
        gold += amount;
        UpdateGoldUI();
    }

    public void UpdateGoldUI()
    {
        if (goldText != null)
            goldText.text = gold.ToString();
    }
}
