using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "TypeEnemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public int damage;
    public int health;
    public int speed;
    [Header("Reward")]
    public int rewardGold = 10;
}
