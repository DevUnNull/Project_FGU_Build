using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cấu hình 1 ngày chơi: kịch bản lối sống (Scene 1) + bộ wave (Scene 3).
/// Đặt tại Resources/Days/Day{n}.asset để GameProgress tự tìm.
/// </summary>
[CreateAssetMenu(fileName = "Day", menuName = "Game/Day Config")]
public class DayConfig : ScriptableObject
{
    public int dayNumber = 1;
    public string displayName = "Ngày 1";

    [Tooltip("Kịch bản lối sống của ngày này")]
    public ScenarioData scenario;

    [Tooltip("Mỗi TuneConfig là 1 làn (Path1..Path5), theo đúng thứ tự làn")]
    public List<TuneConfig> tunes = new List<TuneConfig>();
}
