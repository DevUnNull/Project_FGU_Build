using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Class _Enemy - đây là class chính của Enemy
public class _Enemy : EnemyBase
{
    // Giá trị tiền thưởng khi tiêu diệt enemy này (đọc từ EnemyData)
    [SerializeField] private int currencyWorth = 0;
    
    // Reference đến EnemyPopUp component
    private EnemyPopUp enemyPopUp;

    // Máu gốc lúc sinh ra, dùng để hồi lại khi lấy enemy từ Pool ra tái sử dụng
    private int spawnHealth;
    private bool isDead = false;
    private const float DeathFadeDuration = 0.35f;
    private static readonly Color HitFlashTint = new Color(1f, 0.55f, 0.55f, 1f);
    
    // Hàm Awake được gọi khi GameObject được tạo
    private void Awake()
    {
        // Set dữ liệu từ enemyData
        if (enemyData != null) SetFromData(enemyData);
        
        // Lấy EnemyPopUp component (tự tạo nếu chưa gắn)
        enemyPopUp = GetComponent<EnemyPopUp>();
        if (enemyPopUp == null) enemyPopUp = gameObject.AddComponent<EnemyPopUp>();

        // Đồng bộ reward từ EnemyData (fallback nếu asset chưa set)
        if (rewardGold > 0)
        {
            currencyWorth = rewardGold;
        }

        // Auto-attach virus mechanics based on GameObject/Enemy Name
        AutoAttachVirusMechanics();

        spawnHealth = health;
    }

    // Được gọi mỗi lần Pool bật enemy lên -> reset trạng thái của lần sống trước
    private void OnEnable()
    {
        health = spawnHealth;
        isDead = false;
        SetCombatScriptsEnabled(true);
    }

    // Bật/tắt các script di chuyển & tấn công (tắt trong lúc đang mờ dần khi chết)
    private void SetCombatScriptsEnabled(bool enabled)
    {
        var movement = GetComponent<WaypointMovement>();
        if (movement != null) movement.enabled = enabled;
        var cumA = GetComponent<EnemyCumA>();
        if (cumA != null) cumA.enabled = enabled;
        var pheCau = GetComponent<EnemyPheCau>();
        if (pheCau != null) pheCau.enabled = enabled;
    }

    private void Start()
    {
        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in srs)
        {
            if (sr.sortingOrder < 5) sr.sortingOrder = 10;
        }
    }

    private void AutoAttachVirusMechanics()
    {
        string n = gameObject.name.ToLower();
        if (n.Contains("cuma") || n.Contains("cum"))
        {
            if (GetComponent<EnemyCumA>() == null) gameObject.AddComponent<EnemyCumA>();
        }
        else if (n.Contains("phecau") || n.Contains("phe"))
        {
            if (GetComponent<EnemyPheCau>() == null) gameObject.AddComponent<EnemyPheCau>();
        }
        else if (n.Contains("ecoli"))
        {
            if (GetComponent<EnemyEcoli>() == null) gameObject.AddComponent<EnemyEcoli>();
        }
    }
    
    // Hàm nhận sát thương từ Hero
    public void TakeDamage(int damageAmount)
    {
        // Đã chết (đang mờ dần / đang nổ) -> bỏ qua đòn đánh tới sau
        if (isDead) return;

        // Trừ máu
        health -= damageAmount;

        // Hiệu ứng trúng đòn: nháy đỏ + giật nhẹ
        UnitJuice juice = UnitJuice.GetOrAdd(gameObject);
        juice.Flash(HitFlashTint);
        juice.Punch(new Vector2(-0.1f, 0.1f), 0.18f);
        
        // Hiển thị popup damage (nếu có component)
        if (enemyPopUp != null)
        {
            enemyPopUp.PopUpDame(damageAmount);
        }
        
        // Nếu máu <= 0 thì enemy chết
        if (health <= 0)
        {
            // Gọi hàm Die để xử lý khi enemy chết
            Die();
        }
    }
    
    // Hàm xử lý khi enemy bị tiêu diệt
    private void Die()
    {
        isDead = true;

        int amount = Mathf.Max(0, currencyWorth);
        if (amount > 0)
        {
            bool paid = false;
            if (GoldManager.Instance != null)
            {
                GoldManager.Instance.AddGold(amount);
                paid = true;
                Debug.Log($"{enemyName} died! +{amount} gold (GoldManager)");
            }
            else if (ShopManager.main != null)
            {
                ShopManager.main.AddCurrency(amount);
                paid = true;
                Debug.Log($"{enemyName} died! +{amount} currency (ShopManager)");
            }
            if (!paid)
            {
                Debug.LogWarning($"⚠️ No GoldManager/ShopManager found. Reward {amount} not applied.");
            }

            // Popup vàng nếu có component
            if (enemyPopUp != null)
            {
                enemyPopUp.PopUpGold(amount);
            }
        }
        
        // E.coli tự xử lý cái chết: phát animation "die" -> nổ -> tự trả về Pool
        EnemyEcoli ecoli = GetComponent<EnemyEcoli>();
        if (ecoli != null)
        {
            ecoli.ExplodeOnSpot();
            return;
        }

        // Các enemy khác: dừng di chuyển/tấn công, mờ dần rồi mới trả về pool để tái sử dụng
        SetCombatScriptsEnabled(false);
        UnitJuice.GetOrAdd(gameObject).FadeOut(DeathFadeDuration, () =>
        {
            MultiEnemyPool.Instance?.ReturnToPool(gameObject);
        });
    }
}
