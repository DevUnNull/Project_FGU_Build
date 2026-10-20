using UnityEngine;

[CreateAssetMenu(fileName = "HeroData", menuName = "TypeHero/HeroData")]
public class HeroData : ScriptableObject
{
    [Header("Basic Info")]
    public string HeroName;
    public int damage;
    public int health;
    public float speed;
    public int price;

    [Tooltip("Thời gian giữa 2 lần đánh (tính bằng giây). Đặt 1 = sau 1s đánh lại, 2 = sau 2s đánh lại")]
    public float attackInterval = 1f;

    [Tooltip("Tích chọn nếu tốc độ đánh/hồi chiêu của tướng này chịu ảnh hưởng bởi chỉ số Hydration (Nước) từ Scene 1")]
    public bool affectedByHydration = false;

    [Tooltip("Tích chọn nếu tướng đánh cận chiến (không bắn đạn dù prefab có ProjectileLauncher)")]
    public bool isMelee = false;

    [Tooltip("Tích chọn nếu tướng sinh ATP (rơi đồng xu định kỳ như Hồng Cầu)")]
    public bool producesATP = false;

    [Header("Audio")]
    [Tooltip("Sound khi hero ra trận (spawn)")]
    public AudioClip spawnSound;
    
    [Tooltip("Volume của spawn sound (0-1)")]
    [Range(0f, 1f)]
    public float spawnSoundVolume = 1f;
}
