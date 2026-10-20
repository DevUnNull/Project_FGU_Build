using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hiệu ứng vật phẩm lối sống trong trận + số vật phẩm còn lại của trận này.
/// Mỗi lần vào trận (kể cả Chơi lại) được dùng lại toàn bộ vật phẩm đã nhận ở Scene 1 hôm đó.
/// </summary>
public static class BattleBuffs
{
    public const float WaterDuration = 15f;
    public const float WaterAtpSpeed = 2f;
    public const float VitaminDuration = 12f;
    public const float VitaminDamage = 1.5f;
    public const int SleepHeartHeal = 2;
    public const int SoupAtp = 60;

    private static float waterEndTime;
    private static float vitaminEndTime;
    private static readonly Dictionary<LifestyleItem, int> remaining = new Dictionary<LifestyleItem, int>();
    private static readonly List<LifestyleItem> ownedTypes = new List<LifestyleItem>();

    /// <summary>Hệ số tốc độ sinh ATP (Hồng cầu + ATP theo thời gian).</summary>
    public static float AtpSpeedMultiplier => Time.time < waterEndTime ? WaterAtpSpeed : 1f;

    /// <summary>Hệ số sát thương của tế bào.</summary>
    public static float DamageMultiplier => Time.time < vitaminEndTime ? VitaminDamage : 1f;

    /// <summary>Các loại vật phẩm có trong trận này (theo thứ tự nhận).</summary>
    public static IReadOnlyList<LifestyleItem> OwnedTypes => ownedTypes;

    public static void StartBattle()
    {
        waterEndTime = 0f;
        vitaminEndTime = 0f;
        remaining.Clear();
        ownedTypes.Clear();
        if (StomachDayData.Instance == null) return;

        foreach (LifestyleItem item in StomachDayData.Instance.items)
        {
            if (!remaining.ContainsKey(item))
            {
                remaining[item] = 0;
                ownedTypes.Add(item);
            }
            remaining[item]++;
        }
    }

    public static int Count(LifestyleItem item) => remaining.TryGetValue(item, out int n) ? n : 0;

    /// <summary>Số giây hiệu ứng còn lại (0 nếu vật phẩm không có thời hạn hoặc đã hết).</summary>
    public static float ActiveTimeLeft(LifestyleItem item)
    {
        float end = item == LifestyleItem.WaterBottle ? waterEndTime : item == LifestyleItem.VitaminC ? vitaminEndTime : 0f;
        return Mathf.Max(0f, end - Time.time);
    }

    public static bool Use(LifestyleItem item)
    {
        if (Count(item) <= 0) return false;

        switch (item)
        {
            case LifestyleItem.WaterBottle:
                waterEndTime = Time.time + WaterDuration;
                BattleHUD.Toast($"Đủ nước! ATP sinh ra nhanh gấp đôi trong {WaterDuration:0} giây.", BattleHUD.TipColor);
                break;
            case LifestyleItem.VitaminC:
                vitaminEndTime = Time.time + VitaminDuration;
                BattleHUD.Toast($"Vitamin C giúp tế bào miễn dịch mạnh hơn trong {VitaminDuration:0} giây!", BattleHUD.TipColor);
                break;
            case LifestyleItem.GoodSleep:
                if (LifeManager.Instance != null) LifeManager.Instance.Heal(SleepHeartHeal);
                foreach (HeroBase hero in HeroBase.Active) hero.HealToFull();
                BattleHUD.Toast("Ngủ đủ giấc giúp cơ thể hồi phục: hồi máu tim và máu tế bào!", BattleHUD.TipColor);
                break;
            case LifestyleItem.WarmSoup:
                if (GoldManager.Instance != null) GoldManager.Instance.AddGold(SoupAtp);
                BattleHUD.Toast($"Bữa ăn ấm bổ sung năng lượng: +{SoupAtp} ATP!", BattleHUD.TipColor);
                break;
            default:
                return false;
        }

        remaining[item]--;
        GameSfx.Play(GameSfx.Kind.ItemUse);
        return true;
    }
}
