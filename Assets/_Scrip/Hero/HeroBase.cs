using UnityEngine;

public class HeroBase : MonoBehaviour
{
    [SerializeField] protected HeroData heroData;

    public string enemyName;
    public int damage;
    public int health;
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
        speed = heroData.speed;
        price = heroData.price;
    }
}