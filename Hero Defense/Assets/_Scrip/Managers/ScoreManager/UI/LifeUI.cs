using UnityEngine;
using TMPro;

/// <summary>
/// UI hiển thị máu (Single Responsibility - chỉ hiển thị máu)
/// Tuân theo Observer Pattern - lắng nghe thay đổi từ LifeManager
/// </summary>
public class LifeUI : MonoBehaviour, ILifeListener
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI lifeText;
    [SerializeField] private string lifeTextFormat = "{0}/{1}";

    private LifeManager lifeManager;

    private void Start()
    {
        if (lifeText == null)
        {
            Debug.LogWarning("⚠️ LifeText chưa được gán trong Inspector!");
        }

        // Lấy LifeManager
        lifeManager = LifeManager.Instance;
        if (lifeManager != null)
        {
            lifeManager.Subscribe(this);
            // Cập nhật UI lần đầu
            OnLifeChanged(lifeManager.CurrentLife, lifeManager.MaxLife);
        }
        else
        {
            Debug.LogWarning("LifeManager.Instance is null!");
        }
    }

    private void OnDestroy()
    {
        // Hủy đăng ký khi bị destroy
        if (lifeManager != null)
        {
            lifeManager.Unsubscribe(this);
        }
    }

    /// <summary>
    /// Implement ILifeListener - cập nhật UI khi máu thay đổi
    /// </summary>
    public void OnLifeChanged(int currentLife, int maxLife)
    {
        if (lifeText != null && !string.IsNullOrEmpty(lifeTextFormat))
        {
            lifeText.text = string.Format(lifeTextFormat, currentLife, maxLife);
        }
    }

    /// <summary>
    /// Implement ILifeListener - được gọi khi hết máu
    /// </summary>
    public void OnLifeDepleted()
    {
        // Có thể thêm hiệu ứng visual ở đây nếu cần
        if (lifeText != null && !string.IsNullOrEmpty(lifeTextFormat))
        {
            lifeText.text = string.Format(lifeTextFormat, 0, lifeManager?.MaxLife ?? 0);
        }
    }
}

