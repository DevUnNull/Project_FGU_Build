using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
public class ShopManager : MonoBehaviour
{
    
    public static ShopManager main;
    public int currency = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        main = this;
    }
    void Start()
    {
        currency = 300;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void AddCurrency(int amount)
    {
        currency += amount;
    }
    public bool SpendCurrency(int amount)
    {
        if (currency >= amount)
        {
            currency -= amount;
            return true;
        }
        else
        {
            Debug.Log("Not enough currency!");
            return false;
        }
    }
    }
