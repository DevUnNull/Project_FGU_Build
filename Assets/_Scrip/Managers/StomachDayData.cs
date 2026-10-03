using UnityEngine;

public class StomachDayData : MonoBehaviour
{
    public static StomachDayData Instance { get; private set; }

    [Header("5 Core Stats from Scene 1")]
    [Tooltip("Immunity (Kháng thể): Quyết định Máu & Sát thương gốc của các Tế Bào (Bạch Cầu, Tế Bào B, Đại Thực Bào).")]
    public float immunityMultiplier = 1.0f;

    [Tooltip("Energy (Thể lực): Quyết định lượng ATP khởi điểm khi vào trận (Dùng để mua Tế bào).")]
    public float energyMultiplier = 1.0f;

    [Tooltip("Toxicity (Độc tố): Càng cao thì trên Map 2 càng xuất hiện nhiều Ô Vũng Axit bị khóa không cho đặt Tế bào.")]
    public float toxicityLevel = 0f;

    [Tooltip("Hydration (Nước): Quyết định tốc độ hồi chiêu (Cooldown) của Tế Bào B & Hồng Cầu.")]
    public float hydrationMultiplier = 1.0f;

    [Tooltip("Recovery (Hồi phục): Quyết định lượng Máu Cơ Thể (Body HP - Trái Tim) khởi điểm.")]
    public float recoveryMultiplier = 1.0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetData()
    {
        immunityMultiplier = 1.0f;
        energyMultiplier = 1.0f;
        toxicityLevel = 0f;
        hydrationMultiplier = 1.0f;
        recoveryMultiplier = 1.0f;
    }
}
