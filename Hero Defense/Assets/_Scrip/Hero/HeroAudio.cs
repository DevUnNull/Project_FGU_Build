using UnityEngine;

/// <summary>
/// Component xử lý audio cho Hero
/// Play sound khi hero spawn/ra trận
/// </summary>
[RequireComponent(typeof(HeroBase))]
public class HeroAudio : MonoBehaviour
{
    [Header("References")]
    private HeroBase heroBase;
    private HeroData heroData;

    // Audio chỉ được play khi gọi thủ công từ DragAndDrop khi đặt thành công
    // Không play tự động khi spawn

    private void Awake()
    {
        heroBase = GetComponent<HeroBase>();
    }

    // KHÔNG play tự động khi spawn - chỉ play khi được gọi từ DragAndDrop
    // private void Start() đã bị xóa để tránh tự động play

    /// <summary>
    /// Play sound khi hero spawn/ra trận
    /// </summary>
    public void PlaySpawnSound()
    {
        // Lấy HeroData từ HeroBase
        HeroData data = GetHeroData();
        
        if (data == null)
        {
            Debug.LogWarning($"⚠️ HeroData không tìm thấy cho {gameObject.name}!");
            return;
        }

        if (data.spawnSound == null)
        {
            // Không có sound thì không làm gì (không log warning để tránh spam)
            return;
        }

        // Play sound qua AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                data.spawnSound, 
                data.spawnSoundVolume,
                1f // pitch
            );
        }
        else
        {
            // Fallback: Play tại vị trí hero nếu không có AudioManager
            AudioSource.PlayClipAtPoint(
                data.spawnSound, 
                transform.position, 
                data.spawnSoundVolume
            );
        }
    }

    /// <summary>
    /// Lấy HeroData từ HeroBase
    /// </summary>
    private HeroData GetHeroData()
    {
        if (heroBase != null)
        {
            return heroBase.HeroData;
        }
        return null;
    }
}

