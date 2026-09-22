using TMPro;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance;

    public int gold = 10;

    [Header("UI")]
    public TextMeshProUGUI goldText;  // ← kéo GoldText vào đây trong Inspector

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateGoldUI();
    }

    private float atpTimer = 0f;
    public float baseAtpGenerationRate = 1f; // seconds per tick
    public int atpPerTick = 5;

    private void Update()
    {
        // Simple Coroutine-like behaviour in Update for passive generation
        float multiplier = 1.0f;
        if (StomachDayData.Instance != null)
        {
            multiplier = StomachDayData.Instance.atpRecoveryMultiplier;
        }

        // If multiplier is <= 0, don't generate or handle differently. We assume it's > 0.
        if (multiplier > 0)
        {
            float actualRate = baseAtpGenerationRate / multiplier;
            atpTimer += Time.deltaTime;
            if (atpTimer >= actualRate)
            {
                atpTimer -= actualRate;
                AddGold(atpPerTick);
            }
        }
    }

    public bool HasEnoughGold(int amount)
    {
        return gold >= amount;
    }

    public void SpendGold(int amount)
    {
        gold -= amount;
        UpdateGoldUI();
    }

    public void AddGold(int amount)
    {
        gold += amount;
        UpdateGoldUI();
    }

    public void UpdateGoldUI()
    {
        if (goldText != null)
            goldText.text = gold.ToString();
    }
}
