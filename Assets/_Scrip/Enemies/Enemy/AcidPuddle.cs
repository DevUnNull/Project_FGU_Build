using UnityEngine;

/// <summary>
/// Vũng Axit do Virus E.coli để lại khi nổ vào tường.
/// Gây sát thương theo giây cho tất cả các Tế bào nằm trong khu vực.
/// </summary>
public class AcidPuddle : MonoBehaviour
{
    [Header("Puddle Settings")]
    [SerializeField] private float duration = 8.0f;
    [SerializeField] private float radius = 1.5f;
    [SerializeField] private int damagePerSecond = 5;
    [SerializeField] private float tickInterval = 1.0f;

    private float tickTimer = 0f;
    private float lifeTimer = 0f;

    public void Init(float puddleDuration, float puddleRadius, int dps)
    {
        this.duration = puddleDuration;
        this.radius = puddleRadius;
        this.damagePerSecond = dps;

        EnsureVisuals();
    }

    [Header("Visual Customization")]
    [Tooltip("Sprite hình ảnh của vũng Axit (kéo Sprite vũng axit vào đây)")]
    [SerializeField] private Sprite customPuddleSprite;

    public void SetCustomSprite(Sprite sprite)
    {
        customPuddleSprite = sprite;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sprite != null)
        {
            sr.sprite = sprite;
        }
    }

    private void Start()
    {
        EnsureVisuals();
    }

    private void EnsureVisuals()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
        }

        if (customPuddleSprite != null)
        {
            sr.sprite = customPuddleSprite;
            sr.sortingOrder = 1;
        }
        else if (sr.sprite == null)
        {
            Texture2D texture = new Texture2D(32, 32);
            Color acidColor = new Color(0.2f, 0.9f, 0.1f, 0.45f); // Axit xanh lá trong suốt
            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 32; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f));
                    if (dist <= 15f)
                        texture.SetPixel(x, y, acidColor);
                    else
                        texture.SetPixel(x, y, Color.clear);
                }
            }
            texture.Apply();
            sr.sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 16);
            sr.sortingOrder = 1;
        }

        transform.localScale = new Vector3(radius * 2f, radius * 2f, 1f);
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;
        tickTimer += Time.deltaTime;

        if (tickTimer >= tickInterval)
        {
            DealPuddleDamage();
            tickTimer = 0f;
        }

        if (lifeTimer >= duration)
        {
            Destroy(gameObject);
        }
    }

    private void DealPuddleDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var col in hits)
        {
            HeroBase hero = col.GetComponent<HeroBase>() ?? col.GetComponentInParent<HeroBase>();
            if (hero != null && hero.health > 0 && DragAndDrop.IsUnitActiveOnBoard(hero.gameObject))
            {
                hero.TakeDamage(damagePerSecond);
                Debug.Log($"🧪 Vũng Axit E.coli gây {damagePerSecond} sát thương lên {hero.name}");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
