using UnityEngine;

[CreateAssetMenu(fileName = "HeroData", menuName = "TypeHero/HeroData")]
public class HeroData : ScriptableObject
{
    [Header("Basic Info")]
    public string HeroName;
    public int damage;
    public int health;
    public int speed;
    public int price;

    [Header("Audio")]
    [Tooltip("Sound khi hero ra trận (spawn)")]
    public AudioClip spawnSound;
    
    [Tooltip("Volume của spawn sound (0-1)")]
    [Range(0f, 1f)]
    public float spawnSoundVolume = 1f;
}
