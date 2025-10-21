using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayer : MonoBehaviour
{
    public static UIPlayer Instance; // Singleton để PlayerInfomation gọi đến

    [Header("Player UI Elements")]
    public Slider hpSlider;          // Thanh máu
    public TextMeshProUGUI hpText;              // Text hiện số HP (tùy chọn)

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Gán giá trị ban đầu khi game bắt đầu
        PlayerInfomation player = GameObject.FindWithTag("Player").GetComponent<PlayerInfomation>();
        UpdateHP(player.currentHP, player.maxHP);
    }

    public void UpdateHP(int current, int max)
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = max;
            hpSlider.value = current;
        }

        if (hpText != null)
        {
            hpText.text = current + " / " + max;
        }
    }
}
