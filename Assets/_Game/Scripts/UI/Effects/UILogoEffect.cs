using UnityEngine;

public class UILogoEffect : MonoBehaviour
{
    [Header("Breathing Effect Settings")]
    public float scaleAmount = 0.05f; // How much it scales up/down
    public float speed = 2f; // Speed of the breathing

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        // Calculate the scale offset using a sine wave
        float scaleOffset = Mathf.Sin(Time.unscaledTime * speed) * scaleAmount;
        
        // Apply the scale
        transform.localScale = originalScale + new Vector3(scaleOffset, scaleOffset, scaleOffset);
    }
}
