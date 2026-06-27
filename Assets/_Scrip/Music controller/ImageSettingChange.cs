using UnityEngine;
using UnityEngine.UI;

public class ImageSettingChange : MonoBehaviour
{
    public Image targetImage;      // The image to change (usually the button itself)
    public Sprite spriteOn;        // The "ON" or "active" image
    public Sprite spriteOff;       // The "OFF" or "inactive" image
    private bool isOn = true;     // Start state (false = Off)

    public void ToggleImage()
    {
        isOn = !isOn; // switch between true/false
        targetImage.sprite = isOn ? spriteOn : spriteOff;
    }
}
