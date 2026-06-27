using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý máu tổng của màn chơi (Singleton Pattern + Observer Pattern)
/// Tuân theo Single Responsibility Principle - chỉ quản lý máu
/// </summary>
public class LifeManager : MonoBehaviour
{
    public static LifeManager Instance { get; private set; }

    [Header("Life Settings")]
    [SerializeField] private int maxLife = 10;
    [SerializeField] private int currentLife;

    // Observer Pattern - danh sách các listener
    private List<ILifeListener> lifeListeners = new List<ILifeListener>();

    // Events (optional, có thể dùng thay cho Observer)
    public System.Action<int, int> OnLifeChanged;
    public System.Action OnLifeDepleted;

    public int CurrentLife => currentLife;
    public int MaxLife => maxLife;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeLife();
        }
        else
        {
            // Xóa listeners cũ trước khi destroy instance mới
            Instance.ClearListeners();
            Destroy(gameObject);
        }
    }

    private void InitializeLife()
    {
        currentLife = maxLife;
        NotifyLifeChanged();
    }

    /// <summary>
    /// Trừ máu khi enemy đến đích
    /// </summary>
    /// <param name="damage">Lượng máu bị mất (mặc định 1)</param>
    public void TakeDamage(int damage = 1)
    {
        if (currentLife <= 0) return; // Đã thua rồi, không trừ nữa

        currentLife = Mathf.Max(0, currentLife - damage);
        NotifyLifeChanged();

        if (currentLife <= 0)
        {
            NotifyLifeDepleted();
        }
    }

    /// <summary>
    /// Reset máu về mặc định (khi bắt đầu màn mới)
    /// </summary>
    public void ResetLife()
    {
        currentLife = maxLife;
        NotifyLifeChanged();
    }

    /// <summary>
    /// Đăng ký listener (Observer Pattern)
    /// </summary>
    public void Subscribe(ILifeListener listener)
    {
        if (listener != null && !lifeListeners.Contains(listener))
        {
            lifeListeners.Add(listener);
        }
    }

    /// <summary>
    /// Hủy đăng ký listener
    /// </summary>
    public void Unsubscribe(ILifeListener listener)
    {
        if (listener != null)
        {
            lifeListeners.Remove(listener);
        }
    }

    private void NotifyLifeChanged()
    {
        // Thông báo qua Observer Pattern
        foreach (var listener in lifeListeners)
        {
            listener?.OnLifeChanged(currentLife, maxLife);
        }

        // Thông báo qua Action (optional)
        OnLifeChanged?.Invoke(currentLife, maxLife);
    }

    private void NotifyLifeDepleted()
    {
        // Thông báo qua Observer Pattern
        foreach (var listener in lifeListeners)
        {
            listener?.OnLifeDepleted();
        }

        // Thông báo qua Action (optional)
        OnLifeDepleted?.Invoke();
    }

    /// <summary>
    /// Xóa tất cả listeners (gọi khi reset scene để tránh null reference)
    /// </summary>
    public void ClearListeners()
    {
        lifeListeners.Clear();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}

