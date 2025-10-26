using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [SerializeField] protected EnemyData enemyData;

    public string enemyName;
    public int damage;
    public int health;
    public int speed;

    public void SetFromData(EnemyData enemyData)
    {
        enemyName = enemyData.name;
        damage = enemyData.damage;
        health = enemyData.health;
        speed = enemyData.speed;
    }
}
