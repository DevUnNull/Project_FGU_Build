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

    [Header("Audio")]
    [Tooltip("Sound khi hero ra trận (spawn)")]
    public AudioClip spawnSound;
    
    [Tooltip("Volume của spawn sound (0-1)")]
    [Range(0f, 1f)]
    public float spawnSoundVolume = 1f;
}
