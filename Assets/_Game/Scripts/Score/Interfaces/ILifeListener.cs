/// <summary>
/// Interface cho các class muốn lắng nghe sự thay đổi về máu (Observer Pattern)
/// </summary>
public interface ILifeListener
{
    void OnLifeChanged(int currentLife, int maxLife);
    void OnLifeDepleted();
}

