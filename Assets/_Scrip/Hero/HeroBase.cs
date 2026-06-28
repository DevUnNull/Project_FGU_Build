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

    public void SetFromData(HeroData heroData)
    {
        this.heroData = heroData;
        enemyName = heroData.name;
        damage = heroData.damage;
        health = heroData.health;
        maxHealth = heroData.health;
        speed = heroData.speed;
        price = heroData.price;
        
        // Update UI khi vừa khởi tạo
        OnHealthChanged?.Invoke(health, maxHealth);
    }

    public void TakeDamage(int damageAmount)
    {
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