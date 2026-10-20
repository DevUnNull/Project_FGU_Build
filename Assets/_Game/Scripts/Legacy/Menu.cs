using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
public class Menu : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] TextMeshProUGUI currencyUI;
    private void OnGUI()
    {
        currencyUI.text = ShopManager.main.currency.ToString();
    }
}
