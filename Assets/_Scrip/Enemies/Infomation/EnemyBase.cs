using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [SerializeField] protected EnemyData enemyData;

    public string enemyName;
    public int damage;
    public int health;
    public float speed;
    public float attackInterval = 1.5f;
    public int rewardGold;

    public void SetFromData(EnemyData enemyData)
    {
        this.enemyData = enemyData;
        enemyName = enemyData.name;
        damage = enemyData.damage;
        health = enemyData.health;
        speed = enemyData.speed;
        attackInterval = (enemyData.attackInterval > 0f) ? enemyData.attackInterval : 1.5f;
        rewardGold = enemyData.rewardGold;

        SyncAttackIntervalToMechanics();
    }

    public void SyncAttackIntervalToMechanics()
    {
        var cumA = GetComponent<EnemyCumA>();
        if (cumA != null) cumA.SetAttackInterval(attackInterval);

        var pheCau = GetComponent<EnemyPheCau>();
        if (pheCau != null) pheCau.SetAttackInterval(attackInterval);
    }
}
