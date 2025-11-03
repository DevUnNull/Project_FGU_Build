using UnityEngine;

[CreateAssetMenu(fileName = "HeroData", menuName = "TypeHero/HeroData")]
public class HeroData : ScriptableObject
{
    public string HeroName;
    public int damage;
    public int health;
    public int speed;
    public int price;
}
