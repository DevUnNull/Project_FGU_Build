using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 5 chỉ số cơ thể dạng dữ liệu thuần (không phụ thuộc Scene) + công thức quy đổi impact.
/// Dùng chung cho runtime (StomachDayData) và công cụ cân bằng trong Editor.
/// </summary>
[System.Serializable]
public struct DayStats
{
    public float immunity;
    public float energy;
    public float hydration;
    public float recovery;
    public float toxicity;

    public static DayStats Default => new DayStats { immunity = 1f, energy = 1f, hydration = 1f, recovery = 1f, toxicity = 0f };

    /// <summary>
    /// Áp 1 impact từ ScenarioData. Immunity/Energy/Hydration/Recovery nhập theo % (|giá trị| >= 1 thì chia 100),
    /// Toxicity luôn nhập theo đơn vị x10 (VD: +5 -> +0.5 ô, +30 -> +3 ô).
    /// </summary>
    public void ApplyImpact(StatImpactType stat, float rawValue)
    {
        float percentDelta = Mathf.Abs(rawValue) >= 1f ? rawValue / 100f : rawValue;
        switch (stat)
        {
            case StatImpactType.Immunity: immunity = ClampMultiplier(immunity + percentDelta); break;
            case StatImpactType.Energy: energy = ClampMultiplier(energy + percentDelta); break;
            case StatImpactType.Hydration: hydration = ClampMultiplier(hydration + percentDelta); break;
            case StatImpactType.Recovery: recovery = ClampMultiplier(recovery + percentDelta); break;
            case StatImpactType.Toxicity: toxicity = Mathf.Max(0f, toxicity + rawValue / 10f); break;
        }
    }

    private static float ClampMultiplier(float value) =>
        Mathf.Clamp(value, StomachDayData.MinMultiplier, StomachDayData.MaxMultiplier);

    /// <summary>
    /// Điểm sức khỏe tổng hợp: tổng mức lệch so với 1.0 của 4 chỉ số tích cực, trừ 0.1 cho mỗi điểm Độc tố.
    /// > 0 là khỏe hơn bình thường, < 0 là yếu hơn bình thường.
    /// </summary>
    public float HealthScore =>
        (immunity - 1f) + (energy - 1f) + (hydration - 1f) + (recovery - 1f) - toxicity * 0.1f;

    public int LockedTiles => Mathf.RoundToInt(toxicity);
    public bool HasGastricAcidWave => toxicity >= StomachDayData.GastricAcidThreshold;
}

public class StomachDayData : MonoBehaviour
{
    public static StomachDayData Instance { get; private set; }

    [Header("5 Core Stats from Scene 1")]
    [Tooltip("Immunity (Kháng thể): Quyết định Máu & Sát thương gốc của các Tế Bào (Bạch Cầu, Tế Bào B, Đại Thực Bào).")]
    public float immunityMultiplier = 1.0f;

    [Tooltip("Energy (Thể lực): Quyết định lượng ATP khởi điểm khi vào trận (Dùng để mua Tế bào).")]
    public float energyMultiplier = 1.0f;

    [Tooltip("Toxicity (Độc tố): Càng cao thì trên Map 2 càng xuất hiện nhiều Ô Vũng Axit bị khóa không cho đặt Tế bào.")]
    public float toxicityLevel = 0f;

    [Tooltip("Hydration (Nước): Quyết định tốc độ hồi chiêu (Cooldown) của Tế Bào B & Hồng Cầu.")]
    public float hydrationMultiplier = 1.0f;

    [Tooltip("Recovery (Hồi phục): Quyết định lượng Máu Cơ Thể (Body HP - Trái Tim) khởi điểm.")]
    public float recoveryMultiplier = 1.0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Giới hạn hệ số cho Immunity / Energy / Hydration / Recovery
    public const float MaxMultiplier = 2.0f;
    public const float MinMultiplier = 0.1f;

    // Độc tố tối thiểu để Sóng Axit Dạ Dày xảy ra trong trận (WaveManager dùng làm giá trị mặc định)
    public const float GastricAcidThreshold = 3f;

    public DayStats ToStats() => new DayStats
    {
        immunity = immunityMultiplier,
        energy = energyMultiplier,
        hydration = hydrationMultiplier,
        recovery = recoveryMultiplier,
        toxicity = toxicityLevel
    };

    public void SetStats(DayStats stats)
    {
        immunityMultiplier = stats.immunity;
        energyMultiplier = stats.energy;
        hydrationMultiplier = stats.hydration;
        recoveryMultiplier = stats.recovery;
        toxicityLevel = stats.toxicity;
    }

    public float GetHealthScore() => ToStats().HealthScore;

    // Vật phẩm nhận được từ lựa chọn tốt ở Scene 1 hôm nay (BattleBuffs chép ra khi vào trận)
    public readonly List<LifestyleItem> items = new List<LifestyleItem>();

    public void AddItem(LifestyleItem item)
    {
        if (item != LifestyleItem.None) items.Add(item);
    }

    public void ResetData()
    {
        items.Clear();
        immunityMultiplier = 1.0f;
        energyMultiplier = 1.0f;
        toxicityLevel = 0f;
        hydrationMultiplier = 1.0f;
        recoveryMultiplier = 1.0f;
    }
}
