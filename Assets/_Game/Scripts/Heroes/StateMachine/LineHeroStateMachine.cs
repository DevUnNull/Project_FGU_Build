using UnityEngine;

public class LineHeroStateMachine : HeroStateMachine
{
    public override GameObject FindTarget()
    {
        if (!DragAndDrop.IsUnitActiveOnBoard(gameObject)) return null;

        LayerMask mask = (enemyLayer.value != 0) ? enemyLayer : ~0;

        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            transform.position,
            new Vector2(checkHeight, checkHeight),
            0f,
            checkDirection.normalized,
            detectionRange,
            mask
        );

        GameObject nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null || hit.collider.gameObject == gameObject) continue;
            _Enemy enemyComp = hit.collider.GetComponent<_Enemy>() ?? hit.collider.GetComponentInParent<_Enemy>();
            if (enemyComp == null || enemyComp.health <= 0) continue;

            float distance = Vector2.Distance(transform.position, hit.collider.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = hit.collider.gameObject;
            }
        }

        if (nearestEnemy == null)
        {
            return base.FindTarget();
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
