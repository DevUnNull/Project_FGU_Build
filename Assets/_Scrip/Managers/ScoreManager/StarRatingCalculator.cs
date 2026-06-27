using UnityEngine;

/// <summary>
/// Tính toán số sao dựa trên máu còn lại (Strategy Pattern)
/// Tuân theo Open/Closed Principle - có thể mở rộng cách tính sao mà không sửa code
/// </summary>
public class StarRatingCalculator : IStarRatingCalculator
{
    private readonly float threeStarThreshold = 1.0f;  // 100% máu = 3 sao
    private readonly float twoStarThreshold = 0.5f;    // >= 50% máu = 2 sao
    private readonly float oneStarThreshold = 0.0f;    // < 50% máu = 1 sao

    /// <summary>
    /// Tính số sao dựa trên tỷ lệ máu còn lại
    /// 3 sao: Không mất máu nào (100%)
    /// 2 sao: Mất ít nhất 1 máu nhưng >= 50% máu còn lại
    /// 1 sao: < 50% máu còn lại
    /// </summary>
    public int CalculateStars(int currentLife, int maxLife)
    {
        if (maxLife <= 0) return 0;

        float lifePercentage = (float)currentLife / maxLife;

        // 3 sao: Không mất máu nào
        if (Mathf.Approximately(lifePercentage, threeStarThreshold))
        {
            return 3;
        }

        // 2 sao: Mất ít nhất 1 máu nhưng >= 50%
        if (lifePercentage >= twoStarThreshold)
        {
            return 2;
        }

        // 1 sao: < 50%
        if (lifePercentage >= oneStarThreshold)
        {
            return 1;
        }

        return 0;
    }
}

