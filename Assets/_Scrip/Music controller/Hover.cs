using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;


public class Hover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float scaleDown = 0.9f;  // how small it gets
    [SerializeField] private float duration = 0.2f;   // how fast the tween is
    private Vector3 originalScale;

   public SceneUIController sceneMusicController;

    void Start()
    {
        originalScale = transform.localScale;  // remember original size
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // shrink smoothly
        transform.DOScale(originalScale * scaleDown, duration).SetEase(Ease.OutQuad);
       sceneMusicController.ButtonHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // go back to normal smoothly
        transform.DOScale(originalScale, duration).SetEase(Ease.OutBack);
    }
}
