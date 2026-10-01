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
    
    // Hàm Awake được gọi khi GameObject được tạo
    private void Awake()
    {
        // Set dữ liệu từ enemyData
        if (enemyData != null) SetFromData(enemyData);
        
        // Lấy EnemyPopUp component (có thể không có nếu chưa gắn)
        enemyPopUp = GetComponent<EnemyPopUp>();

        // Đồng bộ reward từ EnemyData (fallback nếu asset chưa set)
        if (rewardGold > 0)
        {
            currencyWorth = rewardGold;
        }

        // Auto-attach virus mechanics based on GameObject/Enemy Name
        AutoAttachVirusMechanics();
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
        // Trừ máu
        health -= damageAmount;
        
        // Hiển thị popup damage (nếu có component)
        if (enemyPopUp != null)
        {
            enemyPopUp.PopUpDame(damageAmount);
        }
        
        // Nếu máu <= 0 thì enemy chết
        if (health <= 0)
        {
            // Nếu là Ecoli bị đánh bại -> Nổ tại chỗ
            EnemyEcoli ecoli = GetComponent<EnemyEcoli>();
            if (ecoli != null)
            {
                ecoli.ExplodeOnSpot();
            }

            // Gọi hàm Die để xử lý khi enemy chết
            Die();
        }
    }
    
    // Hàm xử lý khi enemy bị tiêu diệt
    private void Die()
    {
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
        
        // Trả enemy về pool để tái sử dụng
        MultiEnemyPool.Instance?.ReturnToPool(gameObject);
    }
}
