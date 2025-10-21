using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Enemy Settings")]
    public Animator animator;          // Animator của Enemy
    public float attackCooldown = 2f;  // Thời gian chờ giữa các đòn tấn công

    private bool isPlayerInZone = false;
    private bool canAttack = true;

    public PlayerInfomation PlayerInfomation;
    public Enemy Enemy;
    void Start()
    {
        PlayerInfomation = GameObject.Find("Player").GetComponent<PlayerInfomation>();
        Enemy = GameObject.Find("Enemy").GetComponent <Enemy>();
        if (animator == null)
        {
            animator = GetComponentInParent<Animator>(); // nếu zone là con của enemy
        }
    }

    void Update()
    {
        if (isPlayerInZone && canAttack)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private System.Collections.IEnumerator AttackRoutine()
    {
        canAttack = false;

        if (animator != null)
        {
            animator.SetTrigger("IsAttack");
            PlayerInfomation.TakeDame(Enemy.damage);
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInZone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInZone = false;
        }
    }
}
