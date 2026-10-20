using UnityEngine;
using System;

public class HeroBase : MonoBehaviour
{
    [SerializeField] protected HeroData heroData;

    public event Action<int, int> OnHealthChanged;
    public event Action OnHeroDied;

    public string enemyName;
    public int damage;
    public int health;
    public int maxHealth;
    public float speed;
    public int price;

    [Tooltip("Thời gian giữa 2 lần đánh (tính bằng giây). Đặt 1 = sau 1 giây đánh lại, 2 = sau 2 giây đánh lại")]
    public float attackInterval = 1f;

    // Property để HeroAudio có thể lấy HeroData
    public HeroData HeroData => heroData;

    private EnemyPopUp popUpComponent;

    private bool isDead = false;
    private const float DeathFadeDuration = 0.4f;

    private void Awake()
    {
        popUpComponent = GetComponent<EnemyPopUp>();
        if (popUpComponent == null) popUpComponent = gameObject.AddComponent<EnemyPopUp>();
    }

    private void Start()
    {
        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in srs)
        {
            if (sr.sortingOrder < 5) sr.sortingOrder = 10;
        }
    }

    public void SetFromData(HeroData heroData)
    {
        this.heroData = heroData;
        enemyName = heroData.name;
        
        // Base values
        float dmgMult = 1f;
        float hpMult = 1f;
        float atkSpeedMult = 1f;
        
        if (StomachDayData.Instance != null)
        {
            // Immunity (Kháng thể): Quyết định Máu & Sát thương gốc của các Tế bào
            dmgMult = StomachDayData.Instance.immunityMultiplier;
            hpMult = StomachDayData.Instance.immunityMultiplier;

            // Hydration (Nước): Quyết định tốc độ hồi chiêu (Cooldown/Tốc độ đánh) của Tế Bào B & Hồng Cầu
            if (heroData.affectedByHydration)
            {
                atkSpeedMult = StomachDayData.Instance.hydrationMultiplier;
            }
        }

        damage = Mathf.RoundToInt(heroData.damage * dmgMult);
        health = Mathf.RoundToInt(heroData.health * hpMult);
        maxHealth = health;
        
        speed = heroData.speed;
        price = heroData.price;
        attackInterval = (heroData.attackInterval > 0f) ? heroData.attackInterval : 1f;

        // Đồng bộ Thời gian hồi chiêu (Attack Interval) sang HeroStateMachine
        HeroStateMachine sm = GetComponent<HeroStateMachine>();
        if (sm != null)
        {
            sm.attackInterval = attackInterval / Mathf.Max(0.1f, atkSpeedMult);
        }
        
        // Áp dụng Attack Speed vào Animator
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.speed = atkSpeedMult;
        }

        // Update UI khi vừa khởi tạo
        OnHealthChanged?.Invoke(health, maxHealth);
    }

    public void TakeDamage(int damageAmount)
    {
        // Nếu tướng chưa được mua/chưa đặt lên bàn (đang trong shop) -> Không thể bị mất máu/bị đánh
        if (!DragAndDrop.IsUnitActiveOnBoard(gameObject)) return;

        // Đang chết (mờ dần) -> bỏ qua các đòn đánh tới sau
        if (isDead) return;

        health -= damageAmount;
        OnHealthChanged?.Invoke(health, maxHealth);

        // Hiệu ứng trúng đòn: nháy đỏ + co nhẹ
        UnitJuice juice = UnitJuice.GetOrAdd(gameObject);
        juice.Flash(new Color(1f, 0.45f, 0.45f, 1f));
        juice.Punch(new Vector2(0.08f, -0.08f), 0.18f);

        // Hiển thị popup sát thương nhận phải (Màu Cam Đỏ nổi bật cho Cell)
        if (popUpComponent == null) popUpComponent = GetComponent<EnemyPopUp>() ?? gameObject.AddComponent<EnemyPopUp>();
        if (popUpComponent != null)
        {
            popUpComponent.PopUpDame(damageAmount, new Color(1f, 0.35f, 0.1f));
        }

        if (health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        OnHeroDied?.Invoke();

        // Ngừng mọi hành động: tấn công, sinh tiền, kéo thả, va chạm
        HeroStateMachine sm = GetComponent<HeroStateMachine>();
        if (sm != null) sm.enabled = false;
        HongCauCoinSpawner coinSpawner = GetComponent<HongCauCoinSpawner>();
        if (coinSpawner != null) coinSpawner.enabled = false;
        DragAndDrop drag = GetComponent<DragAndDrop>();
        if (drag != null) drag.enabled = false;
        foreach (var col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }

        // Thanh máu (UI Canvas) không mờ theo SpriteRenderer -> ẩn luôn
        foreach (var canvas in GetComponentsInChildren<Canvas>())
        {
            canvas.enabled = false;
        }

        // Đứng hình ở tư thế hiện tại rồi mờ dần, sau đó mới xóa (ô Tile tự trống khi object bị hủy)
        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.speed = 0f;

        UnitJuice.GetOrAdd(gameObject).FadeOut(DeathFadeDuration, () => Destroy(gameObject));
    }
}