using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class _Enemy :  EnemyBase
{
    [SerializeField] private int currencyWorth = 50;
    private void Awake()
    {
        SetFromData(enemyData);
    }
    //khi enemy bị tiêu diệt gọi ShopManager AddCurrency
    //để cộng tiền mỗi khi enemy bị tiêu diệt

}
