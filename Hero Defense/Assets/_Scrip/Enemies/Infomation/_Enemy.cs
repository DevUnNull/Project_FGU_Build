using UnityEngine;

public class _Enemy :  EnemyBase
{
    private void Update()
    {
        SetFromData(enemyData);
    }
}
