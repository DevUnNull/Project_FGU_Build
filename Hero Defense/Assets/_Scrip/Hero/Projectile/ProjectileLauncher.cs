using UnityEngine;

/// <summary>
/// Gắn lên hero AD để bắn đạn tự dẫn.
/// </summary>
public class ProjectileLauncher : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private HomingProjectile projectilePrefab;
    [SerializeField] private Transform firePoint; // nơi spawn đạn (nếu null dùng transform của hero)
    [SerializeField] private float projectileSpeed = 10f;

    public void Launch(Transform target, int damage)
    {
        if (projectilePrefab == null || target == null) return;

        Transform spawnPoint = firePoint != null ? firePoint : transform;
        HomingProjectile proj = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
        proj.Initialize(target, damage, projectileSpeed);
    }
}


