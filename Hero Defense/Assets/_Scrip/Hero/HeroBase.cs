using UnityEngine;

public class HeroBase : MonoBehaviour
{
    [SerializeField] protected HeroData heroData;

    public string enemyName;
    public int damage;
    public int health;
    public int speed;
    public int price;

    public void SetFromData(HeroData heroData)
    {
        enemyName = heroData.name;
        damage = heroData.damage;
        health = heroData.health;
        speed = heroData.speed;
        price = heroData.price;
    }
}
