using UnityEngine;

public class LineHeroStateMachine : HeroStateMachine
{
    [Header("Line Detection Settings")]
    public Vector2 checkDirection = Vector2.right;
    public float checkHeight = 1f;

    public override GameObject FindTarget()
    {
        // Dùng BoxCast để tìm enemy trên một đường thẳng (theo hướng checkDirection)
        // BoxCast giúp check một khoảng có độ rộng (checkHeight) chứ không chỉ 1 tia mỏng
        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            transform.position,
            new Vector2(checkHeight, checkHeight), // Kích thước box
            0f,                                    // Góc quay
            checkDirection.normalized,             // Hướng check
            detectionRange,                        // Tầm xa
            enemyLayer                             // Chỉ check layer Enemy
        );

        GameObject nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        foreach (RaycastHit2D hit in hits)
        {
            float distance = Vector2.Distance(transform.position, hit.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = hit.collider.gameObject;
            }
        }

        return nearestEnemy;
    }

    // Vẽ Gizmos để hiển thị vùng check
    void OnDrawGizmosSelected()
    {
        if (!showRanges) return;

        Gizmos.color = Color.yellow;
        Vector3 start = transform.position;
        Vector3 end = start + (Vector3)checkDirection.normalized * detectionRange;
        Gizmos.DrawLine(start, end);
        
        // Vẽ một hình hộp nhỏ ở cuối để thể hiện BoxCast
        Gizmos.DrawWireCube(end, new Vector3(checkHeight, checkHeight, 0));
    }
}
