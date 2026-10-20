using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WaveConfig", menuName = "Game/WaveConfig")]
public class WaveConfig : ScriptableObject
{
    [System.Serializable]
    public class SpawnGroup
    {
        public EnemyType type;
        public int count;
        public float interval = 1f;
    }

    public PathID pathID;

    [Header("Cấu hình Lượt Trống (Skip Wave)")]
    [Tooltip("Tích chọn nếu không muốn sinh quái trong lượt này (lượt này sẽ KHÔNG được tính là 1 Wave)")]
    public bool isEmptyWave = false;

    public List<SpawnGroup> groups = new();

    /// <summary>
    /// Kiểm tra xem WaveConfig này có phải lượt trống hay không.
    /// Trả về true nếu tích chọn isEmptyWave, danh sách groups rỗng, hoặc tất cả group đều có type = None / count <= 0.
    /// </summary>
    public bool IsEmptyWave()
    {
        if (isEmptyWave) return true;
        if (groups == null || groups.Count == 0) return true;

        foreach (var group in groups)
        {
            if (group.type != EnemyType.None && group.count > 0)
            {
                return false;
            }
        }
        return true;
    }
}