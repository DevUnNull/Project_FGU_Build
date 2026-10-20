using UnityEngine;

/// <summary>
/// Tiến trình chơi: ngày đang chơi + ngày đã mở khóa (lưu bằng PlayerPrefs).
/// </summary>
public static class GameProgress
{
    private const string UnlockedDayKey = "CallOutMyCells_UnlockedDay";
    private const string StarsKeyPrefix = "CallOutMyCells_Stars_Day";

    /// <summary>Ngày đang chơi (chọn ở MainMap). Mặc định Ngày 1.</summary>
    public static int CurrentDay { get; set; } = 1;

    public static int UnlockedDay => Mathf.Max(1, PlayerPrefs.GetInt(UnlockedDayKey, 1));

    public static bool IsUnlocked(int day) => day <= UnlockedDay;

    public static DayConfig GetDayConfig(int day) => Resources.Load<DayConfig>($"Days/Day{day}");

    public static DayConfig CurrentDayConfig => GetDayConfig(CurrentDay);

    /// <summary>Số sao tốt nhất đã đạt ở 1 ngày (0 nếu chưa thắng).</summary>
    public static int GetBestStars(int day) => Mathf.Clamp(PlayerPrefs.GetInt(StarsKeyPrefix + day, 0), 0, 3);

    /// <summary>Ghi sao nếu cao hơn kỷ lục cũ. Trả về số sao MỚI có thêm (0 nếu không phá kỷ lục).</summary>
    public static int RecordStars(int day, int stars)
    {
        int best = GetBestStars(day);
        if (stars <= best) return 0;
        PlayerPrefs.SetInt(StarsKeyPrefix + day, Mathf.Clamp(stars, 0, 3));
        PlayerPrefs.Save();
        return stars - best;
    }

    /// <summary>Tổng sao tốt nhất của mọi ngày đã mở (dùng để mua nâng cấp).</summary>
    public static int TotalStars
    {
        get
        {
            int total = 0;
            for (int day = 1; day <= UnlockedDay; day++) total += GetBestStars(day);
            return total;
        }
    }

    public const int MaxDays = 50;

    /// <summary>Số ngày đã có nội dung (Resources/Days/Day1, Day2... liên tiếp).</summary>
    public static int DayCount
    {
        get
        {
            int count = 0;
            while (count < MaxDays && GetDayConfig(count + 1) != null) count++;
            return count;
        }
    }

    /// <summary>Chơi lại từ đầu: khóa lại các ngày và xóa sao.</summary>
    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(UnlockedDayKey);
        for (int day = 1; day <= MaxDays; day++) PlayerPrefs.DeleteKey(StarsKeyPrefix + day);
        PlayerPrefs.Save();
        CurrentDay = 1;
    }

    /// <summary>Thắng 1 ngày -> mở khóa ngày kế tiếp.</summary>
    public static void CompleteDay(int day)
    {
        if (day + 1 > UnlockedDay)
        {
            PlayerPrefs.SetInt(UnlockedDayKey, day + 1);
            PlayerPrefs.Save();
        }
    }
}
