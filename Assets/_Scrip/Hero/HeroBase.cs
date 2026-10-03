using UnityEngine;
using System;

public class HeroBase : MonoBehaviour
{
    [SerializeField] protected HeroData heroData;

    public event Action<int, int> OnHealthChanged;
    public event Action OnHeroDied;

    public string enemyName;
    public int damage;
    public int health;
    public int maxHealth;
    public int speed;
    public int price;

    // Property để HeroAudio có thể lấy HeroData
    public HeroData HeroData => heroData;

    private void Start()
    {
        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in srs)
        {
            if (sr.sortingOrder < 5) sr.sortingOrder = 10;
        }
    }

    public void SetFromData(HeroData heroData)
    {
        this.heroData = heroData;
        enemyName = heroData.name;
        
        // Base values
        float dmgMult = 1f;
        float hpMult = 1f;
        float atkSpeedMult = 1f;
        
        if (StomachDayData.Instance != null)
        {
            // Immunity (Kháng thể): Quyết định Máu & Sát thương gốc của các Tế bào
            dmgMult = StomachDayData.Instance.immunityMultiplier;
            hpMult = StomachDayData.Instance.immunityMultiplier;

            // Hydration (Nước): Quyết định tốc độ hồi chiêu (Cooldown/Tốc độ đánh) của Tế Bào B & Hồng Cầu
            if (gameObject.name.Contains("CellB") || gameObject.name.Contains("Tế Bào B") || 
                gameObject.name.Contains("HongCau") || gameObject.name.Contains("Hồng Cầu"))
            {
                atkSpeedMult = StomachDayData.Instance.hydrationMultiplier;
            }
        }

        damage = Mathf.RoundToInt(heroData.damage * dmgMult);
        health = Mathf.RoundToInt(heroData.health * hpMult);
        maxHealth = health;
        
        speed = heroData.speed;
        price = heroData.price;
        
        // Áp dụng Attack Speed vào Animator
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.speed = atkSpeedMult;
        }

        // Update UI khi vừa khởi tạo
        OnHealthChanged?.Invoke(health, maxHealth);
    }

    public void TakeDamage(int damageAmount)
    {
        // Nếu tướng chưa được mua/chưa đặt lên bàn (đang trong shop) -> Không thể bị mất máu/bị đánh
        if (!DragAndDrop.IsUnitActiveOnBoard(gameObject)) return;

        health -= damageAmount;
        OnHealthChanged?.Invoke(health, maxHealth);

        if (health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        OnHeroDied?.Invoke();
        // Bạn có thể return về pool hoặc destroy
        Destroy(gameObject);
    }
}