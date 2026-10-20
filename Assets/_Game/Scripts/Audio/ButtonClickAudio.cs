using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Plays a one-shot sound whenever the associated UI element is clicked.
/// </summary>
public class ButtonClickAudio : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private AudioClip clickClip;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField] private AudioSource overrideSource;

    /// <summary>
    /// Public entry point so the sound can also be triggered from UnityEvents.
    /// </summary>
    public void Play()
    {
        if (clickClip == null)
        {
            return;
        }

        if (overrideSource != null)
        {
            overrideSource.PlayOneShot(clickClip, volume);
            return;
        }

        Vector3 playPosition = Camera.main != null ? Camera.main.transform.position : transform.position;
        AudioSource.PlayClipAtPoint(clickClip, playPosition, volume);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Play();
    }
}

