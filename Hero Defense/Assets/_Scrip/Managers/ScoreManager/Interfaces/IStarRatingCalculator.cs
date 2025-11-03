/// <summary>
/// Interface cho các Strategy tính toán sao (Strategy Pattern)
/// </summary>
public interface IStarRatingCalculator
{
    /// <summary>
    /// Tính toán số sao dựa trên máu còn lại
    /// </summary>
    /// <param name="currentLife">Máu hiện tại</param>
    /// <param name="maxLife">Máu tối đa</param>
    /// <returns>Số sao (1-3)</returns>
    int CalculateStars(int currentLife, int maxLife);
}

