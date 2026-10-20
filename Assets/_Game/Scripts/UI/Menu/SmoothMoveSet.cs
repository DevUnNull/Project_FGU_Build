using UnityEngine;
using DG.Tweening;

public class SmoothMoveSet : MonoBehaviour
{
    public float duration = 1f;       // animation time
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Call this to play the slide-in animation
    public void PlaySlideIn(float moveDistance)
    {
        // move back to original position smoothly
        rectTransform.DOAnchorPosY( moveDistance, duration)
        .SetEase(Ease.OutBack);
    }
}
