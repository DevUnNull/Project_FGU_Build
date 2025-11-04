using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PopUp : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textMeshProUGUI;
    public string text_Value;
    [Header("Style")]
    public Color textColor = Color.white;
    [SerializeField] private float lifetime = 1.5f;

    private void Start()
    {
        textMeshProUGUI.text = text_Value;
        textMeshProUGUI.color = textColor;
        Destroy(gameObject, lifetime);
    }
}
