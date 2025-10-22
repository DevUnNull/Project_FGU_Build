using UnityEngine;

public class _Enemy :  EnemyBase
{
    private void Awake()
    {
        SetFromData(enemyData);
    }

}
