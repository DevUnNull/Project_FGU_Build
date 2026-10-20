using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sổ tay sức khỏe: ghi nhớ các mục người chơi đã mở (thẻ kiến thức, tế bào, mầm bệnh) bằng PlayerPrefs.
/// Thẻ kiến thức mở khi đọc ở Scene 1; tế bào mở khi đặt lên bàn; mầm bệnh mở khi gặp trong trận.
/// </summary>
public static class NotebookProgress
{
    private const string PrefKey = "CallOutMyCells_Notebook";
    private const int MaxDaysToScan = 50;

    private static HashSet<string> unlocked;

    private static HashSet<string> Unlocked
    {
        get
        {
            if (unlocked == null) Load();
            return unlocked;
        }
    }

    public static string TipId(string tip) => "tip:" + StableHash(tip.Trim());
    public static string HeroId(string heroAssetName) => "hero:" + heroAssetName;
    public static string EnemyId(string enemyAssetName) => "enemy:" + enemyAssetName;

    public static bool IsUnlocked(string id) => Unlocked.Contains(id);

    /// <summary>Mở 1 mục. Trả về true nếu đây là lần đầu mở.</summary>
    public static bool Unlock(string id)
    {
        if (string.IsNullOrEmpty(id) || !Unlocked.Add(id)) return false;
        PlayerPrefs.SetString(PrefKey, string.Join("|", unlocked));
        PlayerPrefs.Save();
        return true;
    }

    /// <summary>Mầm bệnh xuất hiện lần đầu -> mở mục và báo cho người chơi.</summary>
    public static void UnlockEnemy(string enemyAssetName)
    {
        if (!Unlock(EnemyId(enemyAssetName))) return;

        NotebookCatalog.UnitEntry entry = NotebookCatalog.Instance != null ? NotebookCatalog.Instance.FindEnemy(enemyAssetName) : null;
        string name = entry != null ? entry.displayName : enemyAssetName;
        BattleHUD.Toast($"Mầm bệnh mới: {name}! Đã ghi vào Sổ tay sức khỏe.", BattleHUD.TipColor);
    }

    public static void UnlockHero(string heroAssetName)
    {
        Unlock(HeroId(heroAssetName));
    }

    /// <summary>Tất cả thẻ kiến thức có trong các ngày chơi (Resources/Days/Day1, Day2...), không trùng.</summary>
    public static List<string> GetAllTips()
    {
        var tips = new List<string>();
        var seen = new HashSet<string>();
        for (int day = 1; day <= MaxDaysToScan; day++)
        {
            DayConfig config = GameProgress.GetDayConfig(day);
            if (config == null) break;
            if (config.scenario == null) continue;

            foreach (var situation in config.scenario.situations)
            {
                foreach (var choice in situation.choices)
                {
                    if (string.IsNullOrEmpty(choice.knowledgeTip)) continue;
                    string tip = choice.knowledgeTip.Trim();
                    if (seen.Add(tip)) tips.Add(tip);
                }
            }
        }
        return tips;
    }

    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(PrefKey);
        PlayerPrefs.Save();
        unlocked = null;
    }

    /// <summary>Thẻ kiến thức của 1 ngày: (đã mở, tổng).</summary>
    public static (int opened, int total) GetDayTipProgress(DayConfig config)
    {
        int opened = 0, total = 0;
        if (config == null || config.scenario == null) return (0, 0);
        var seen = new HashSet<string>();
        foreach (var situation in config.scenario.situations)
        {
            foreach (var choice in situation.choices)
            {
                if (string.IsNullOrEmpty(choice.knowledgeTip)) continue;
                string tip = choice.knowledgeTip.Trim();
                if (!seen.Add(tip)) continue;
                total++;
                if (IsUnlocked(TipId(tip))) opened++;
            }
        }
        return (opened, total);
    }

    private static void Load()
    {
        unlocked = new HashSet<string>();
        string raw = PlayerPrefs.GetString(PrefKey, "");
        if (raw.Length == 0) return;
        foreach (string id in raw.Split('|'))
        {
            if (id.Length > 0) unlocked.Add(id);
        }
    }

    // Băm FNV-1a: string.GetHashCode không ổn định giữa các nền tảng nên không dùng để lưu
    private static string StableHash(string text)
    {
        uint hash = 2166136261;
        foreach (char c in text)
        {
            hash ^= c;
            hash *= 16777619;
        }
        return hash.ToString("x8");
    }
}
