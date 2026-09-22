using UnityEngine;

public class StomachDayData : MonoBehaviour
{
    public static StomachDayData Instance { get; private set; }

    [Header("Multipliers from Scene 1")]
    public float atpRecoveryMultiplier = 1.0f;
    public float mucosaHpMultiplier = 1.0f;
    public float cellDamageMultiplier = 1.0f;
    public float cellAttackSpeedMultiplier = 1.0f;
    
    [Header("Negative Events")]
    public float gastricAcidLevel = 0f;
    public int toxinObstaclesCount = 0;

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
        atpRecoveryMultiplier = 1.0f;
        mucosaHpMultiplier = 1.0f;
        cellDamageMultiplier = 1.0f;
        cellAttackSpeedMultiplier = 1.0f;
        gastricAcidLevel = 0f;
        toxinObstaclesCount = 0;
    }
}
