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
