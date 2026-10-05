using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "TypeEnemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public int damage;
    public int health;
    public float speed;

    [Tooltip("Thời gian giữa 2 lần tấn công (tính bằng giây). Đặt 1 = sau 1s đánh lại, 2 = sau 2s đánh lại")]
    public float attackInterval = 1.5f;

    [Header("Reward")]
    public int rewardGold = 10;
}
