using UnityEngine;
using DG.Tweening;

public class SmoothMove : MonoBehaviour
{
    public float moveDistance = 774f; // how far to slide from the left (UI units)
    public float duration = 1f;       // animation time

    private RectTransform rectTransform;
    private Vector2 originalPos;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPos = rectTransform.anchoredPosition;
    }

    // Call this to play the slide-in animation
    public void PlaySlideIn()
    {
        // start off-screen to the left
        rectTransform.anchoredPosition = new Vector2(0, moveDistance);

        // move back to original position smoothly
        rectTransform.DOAnchorPosY(originalPos.y, duration)
            .SetEase(Ease.OutBack);
    }
}
