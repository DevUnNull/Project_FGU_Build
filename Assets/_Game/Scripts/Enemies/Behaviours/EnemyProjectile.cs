using UnityEngine;

/// <summary>
/// Viên đạn từ Virus (như CumA) bắn vào Cell/Player.
/// Gây sát thương khi va chạm với HeroBase (Player).
/// </summary>
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 7f;
    [SerializeField] private float hitRadius = 0.3f;
    [SerializeField] private float lifeTime = 5f;

    private Transform target;
    private Vector3 targetPosition;
    private int damage;
    private bool hasTarget;

    private float targetX = -100f;
    private float initialY;

    public void Initialize(Transform target, int damage, float speedOverride = -1f)
    {
        this.target = target;
        this.damage = damage;
        if (target != null)
        {
            this.targetPosition = target.position;
            this.targetX = target.position.x;
            hasTarget = true;
        }
        if (speedOverride > 0f) speed = speedOverride;

        initialY = transform.position.y;
        EnsureVisuals();
    }

    public void InitializeDirection(Vector3 direction, int damage, float speedOverride = -1f)
    {
        this.damage = damage;
        this.targetPosition = transform.position + direction.normalized * 20f;
        this.targetX = transform.position.x - 20f;
        hasTarget = true;
        if (speedOverride > 0f) speed = speedOverride;

        initialY = transform.position.y;
        EnsureVisuals();
    }

    [Header("Visual Customization")]
    [Tooltip("Gán hình Sprite của viên đạn vào đây trong Inspector")]
    [SerializeField] private Sprite customBulletSprite;

    public void SetCustomSprite(Sprite sprite)
    {
        customBulletSprite = sprite;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sprite != null)
        {
            sr.sprite = sprite;
        }
    }

    private void EnsureVisuals()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
        }

        if (customBulletSprite != null)
        {
            sr.sprite = customBulletSprite;
            sr.sortingOrder = 10;
        }
        else if (sr.sprite == null)
        {
            // Fallback tạo hình khối tròn đỏ/cam nếu chưa gán Sprite
            Texture2D texture = new Texture2D(16, 16);
            Color virusColor = new Color(1f, 0.2f, 0.2f, 1f);
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(7.5f, 7.5f));
                    if (dist <= 7f)
                        texture.SetPixel(x, y, virusColor);
                    else
                        texture.SetPixel(x, y, Color.clear);
                }
            }
            texture.Apply();
            sr.sprite = Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);
            sr.sortingOrder = 10;
        }

        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.3f;
        }

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (target != null && target && target.gameObject != null && target.gameObject.activeInHierarchy)
        {
            targetX = target.position.x;
        }

        // ✅ Di chuyển BẮN THẲNG THEO TRỤC X NGANG (bên trái)
        Vector3 currentPos = transform.position;
        currentPos.x -= speed * Time.deltaTime;
        currentPos.y = initialY; // Giữ nguyên hàng Y
        transform.position = currentPos;

        if (transform.position.x <= targetX || CheckHit())
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HeroBase hero = other.GetComponent<HeroBase>() ?? other.GetComponentInParent<HeroBase>();
        if (hero != null && DragAndDrop.IsUnitActiveOnBoard(hero.gameObject))
        {
            hero.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    private bool CheckHit()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, hitRadius);
        foreach (var col in hits)
        {
            HeroBase hero = col.GetComponent<HeroBase>() ?? col.GetComponentInParent<HeroBase>();
            if (hero != null && DragAndDrop.IsUnitActiveOnBoard(hero.gameObject))
            {
                hero.TakeDamage(damage);
                return true;
            }
        }
        return false;
    }
}
