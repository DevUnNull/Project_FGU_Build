using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Class _Enemy - đây là class chính của Enemy
public class _Enemy : EnemyBase
{
    // Giá trị tiền thưởng khi tiêu diệt enemy này
    [SerializeField] private int currencyWorth = 50;
    
    // Hàm Awake được gọi khi GameObject được tạo
    private void Awake()
    {
        // Set dữ liệu từ enemyData
        SetFromData(enemyData);
    }
    
    // Hàm nhận sát thương từ Hero
    public void TakeDamage(int damageAmount)
    {
        // Trừ máu
        health -= damageAmount;
        
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
        // Gọi ShopManager.AddCurrency để cộng tiền khi enemy bị tiêu diệt
        if (ShopManager.main != null)
        {
            ShopManager.main.AddCurrency(currencyWorth);
        }
        
        // Trả enemy về pool để tái sử dụng
        MultiEnemyPool.Instance?.ReturnToPool(gameObject);
    }
}
