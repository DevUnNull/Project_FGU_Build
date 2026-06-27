/// <summary>
/// Interface cho các class xử lý game over (Dependency Inversion)
/// </summary>
public interface IGameOverHandler
{
    void ShowGameOver(int stars);
    void HideGameOver();
}

