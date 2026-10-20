using UnityEngine;

/// <summary>
/// Tốc độ trận đấu (x1 / x2). Nhớ tốc độ người chơi chọn để khi hết Pause thì khôi phục đúng,
/// thay vì luôn đặt Time.timeScale = 1. Lựa chọn được lưu lại cho các trận sau.
/// </summary>
public static class GameSpeed
{
    private const string PrefKey = "CallOutMyCells_GameSpeed";

    public static float Current { get; private set; } = 1f;

    public static void Set(float speed)
    {
        Current = Mathf.Max(0.1f, speed);
        PlayerPrefs.SetFloat(PrefKey, Current);
        PlayerPrefs.Save();
        // Đang Pause (timeScale = 0) thì chỉ ghi nhớ, áp dụng khi Resume
        if (Time.timeScale > 0f) Time.timeScale = Current;
    }

    /// <summary>Đổi tốc độ mặc định từ menu Setting: chỉ lưu lại, không đổi Time.timeScale của menu.</summary>
    public static void SetPreferred(float speed)
    {
        Current = Mathf.Max(0.1f, speed);
        PlayerPrefs.SetFloat(PrefKey, Current);
        PlayerPrefs.Save();
    }

    public static float Preferred => Mathf.Clamp(PlayerPrefs.GetFloat(PrefKey, 1f), 1f, 2f);

    public static void Resume()
    {
        Time.timeScale = Current;
    }

    /// <summary>Vào trận: dùng lại tốc độ người chơi chọn ở trận trước.</summary>
    public static void LoadSaved()
    {
        Current = Mathf.Clamp(PlayerPrefs.GetFloat(PrefKey, 1f), 1f, 2f);
        if (Time.timeScale > 0f) Time.timeScale = Current;
    }

    /// <summary>Rời trận: trả về x1 cho các scene khác, vẫn giữ lựa chọn đã lưu.</summary>
    public static void ExitBattle()
    {
        Time.timeScale = 1f;
    }
}
