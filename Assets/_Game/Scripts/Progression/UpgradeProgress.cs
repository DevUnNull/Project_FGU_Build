using UnityEngine;

public enum UpgradeType
{
    CellHealth,
    CellDamage,
    StartAtp,
    HeartLife
}

/// <summary>
/// Nâng cấp lâu dài mua bằng sao (sao tốt nhất của mỗi ngày, xem GameProgress).
/// Mỗi cấp tốn 1 sao, tối đa 3 cấp mỗi loại; có thể đặt lại để lấy lại toàn bộ sao.
/// </summary>
public static class UpgradeProgress
{
    public const int MaxLevel = 3;
    public const int CostPerLevel = 1;

    private const string KeyPrefix = "CallOutMyCells_Upgrade_";

    public const float CellHealthPerLevel = 0.1f;
    public const float CellDamagePerLevel = 0.1f;
    public const int StartAtpPerLevel = 25;
    public const int HeartLifePerLevel = 1;

    public static readonly UpgradeType[] All =
        { UpgradeType.CellHealth, UpgradeType.CellDamage, UpgradeType.StartAtp, UpgradeType.HeartLife };

    public static int GetLevel(UpgradeType type) => Mathf.Clamp(PlayerPrefs.GetInt(KeyPrefix + type, 0), 0, MaxLevel);

    public static int SpentStars
    {
        get
        {
            int spent = 0;
            foreach (UpgradeType type in All) spent += GetLevel(type) * CostPerLevel;
            return spent;
        }
    }

    public static int AvailableStars => Mathf.Max(0, GameProgress.TotalStars - SpentStars);

    public static bool CanBuy(UpgradeType type) => GetLevel(type) < MaxLevel && AvailableStars >= CostPerLevel;

    public static bool TryBuy(UpgradeType type)
    {
        if (!CanBuy(type)) return false;
        PlayerPrefs.SetInt(KeyPrefix + type, GetLevel(type) + 1);
        PlayerPrefs.Save();
        return true;
    }

    /// <summary>Trả lại toàn bộ sao đã dùng để phân bổ lại.</summary>
    public static void ResetAll()
    {
        foreach (UpgradeType type in All) PlayerPrefs.DeleteKey(KeyPrefix + type);
        PlayerPrefs.Save();
    }

    // ===== Hiệu ứng áp dụng trong trận =====
    public static float CellHealthMultiplier => 1f + CellHealthPerLevel * GetLevel(UpgradeType.CellHealth);
    public static float CellDamageMultiplier => 1f + CellDamagePerLevel * GetLevel(UpgradeType.CellDamage);
    public static int BonusStartAtp => StartAtpPerLevel * GetLevel(UpgradeType.StartAtp);
    public static int BonusHeartLife => HeartLifePerLevel * GetLevel(UpgradeType.HeartLife);

    public static string GetTitle(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.CellHealth: return "Tế bào bền bỉ";
            case UpgradeType.CellDamage: return "Tế bào mạnh mẽ";
            case UpgradeType.StartAtp: return "Dự trữ năng lượng";
            case UpgradeType.HeartLife: return "Trái tim khỏe";
            default: return type.ToString();
        }
    }

    /// <summary>Mô tả tác dụng: mỗi cấp được gì, và tổng hiện tại nếu đã nâng.</summary>
    public static string GetEffectText(UpgradeType type)
    {
        int level = GetLevel(type);
        string perLevel, total;
        switch (type)
        {
            case UpgradeType.CellHealth:
                perLevel = $"máu mọi tế bào +{Mathf.RoundToInt(CellHealthPerLevel * 100f)}%";
                total = $"+{Mathf.RoundToInt(CellHealthPerLevel * 100f * level)}% máu";
                break;
            case UpgradeType.CellDamage:
                perLevel = $"sát thương mọi tế bào +{Mathf.RoundToInt(CellDamagePerLevel * 100f)}%";
                total = $"+{Mathf.RoundToInt(CellDamagePerLevel * 100f * level)}% sát thương";
                break;
            case UpgradeType.StartAtp:
                perLevel = $"vào trận có thêm {StartAtpPerLevel} ATP";
                total = $"+{StartAtpPerLevel * level} ATP khởi đầu";
                break;
            case UpgradeType.HeartLife:
                perLevel = $"máu tim +{HeartLifePerLevel}";
                total = $"+{HeartLifePerLevel * level} máu tim";
                break;
            default:
                return "";
        }
        return level > 0 ? $"Mỗi cấp: {perLevel}. Hiện có: {total}." : $"Mỗi cấp: {perLevel}.";
    }
}
