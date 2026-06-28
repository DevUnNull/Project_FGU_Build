using UnityEngine;
using UnityEngine.UI;

public class HeroHealthUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Kéo Slider máu của Hero vào đây")]
    public Slider healthSlider;

    private HeroBase heroBase;

    private void Awake()
    {
        // Tự động tìm HeroBase ở parent hoặc trên cùng object
        heroBase = GetComponentInParent<HeroBase>();
        if (heroBase == null)
        {
            Debug.LogWarning("HeroHealthUI needs to be a child of a GameObject with HeroBase.");
            return;
        }
    }

    private void OnEnable()
    {
        if (heroBase != null)
        {
            heroBase.OnHealthChanged += UpdateHealthBar;
        }
    }

    private void OnDisable()
    {
        if (heroBase != null)
        {
            heroBase.OnHealthChanged -= UpdateHealthBar;
        }
    }

    private void Start()
    {
        // Khởi tạo giá trị ban đầu nếu healthSlider đã có
        if (heroBase != null && healthSlider != null)
        {
            UpdateHealthBar(heroBase.health, heroBase.maxHealth > 0 ? heroBase.maxHealth : heroBase.health);
        }
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }
}
