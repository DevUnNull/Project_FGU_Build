using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Gate : MonoBehaviour
{
    public RectTransform leftGate;
    public RectTransform rightGate;
    public float duration = 0.7f;

    private Vector2 leftStart;
    private Vector2 rightStart;
    private Vector2 leftClosedPos;
    private Vector2 rightClosedPos;

    void Start()
    {
        // Save starting positions (off-screen)
        leftStart = leftGate.anchoredPosition;
        rightStart = rightGate.anchoredPosition;
        
        // Closed positions (meet at middle)
        leftClosedPos = new Vector2(-508, leftStart.y);
        rightClosedPos = new Vector2(587, rightStart.y);
        OpenGate();
    }

    public void OpenGate()
    {
        Sequence seq = DOTween.Sequence();
        leftGate.anchoredPosition = leftClosedPos;
        rightGate.anchoredPosition = rightClosedPos;

        seq.Append(leftGate.DOAnchorPos(leftStart, duration).SetEase(Ease.InOutCubic));
        seq.Join(rightGate.DOAnchorPos(rightStart, duration).SetEase(Ease.InOutCubic));
    }
    
    public void CloseGate()
    {
        Sequence seq = DOTween.Sequence();

        seq.Append(leftGate.DOAnchorPos(leftClosedPos, duration).SetEase(Ease.InOutCubic));
        seq.Join(rightGate.DOAnchorPos(rightClosedPos, duration).SetEase(Ease.InOutCubic));
    }
}
