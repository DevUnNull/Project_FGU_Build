using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
public class Plot : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Color hovercolor;
    private GameObject champ;
    private Color startcolor;
    private void Start()
    {
        startcolor = sr.color;
    }
    private void OnMouseEnter()
    {
        sr.color = hovercolor;
    }
    private void OnMouseExit()
    {
        sr.color = startcolor;
    }
    private void OnMouseDown()
    {
        if (champ != null) return;
        Champion champToBuild = BuildManager.main.GetSelectedChamp();
        if (champToBuild.cost > ShopManager.main.currency)
        {
            Debug.Log("Not enough currency to build ");
            return;
        }
        ShopManager.main.SpendCurrency(champToBuild.cost);
        champ = Instantiate(champToBuild.champPrefab, transform.position, Quaternion.identity);
    }
}
