using System.Collections;
using UnityEngine;

/// <summary>
/// Plays a configured audio clip whenever the attached GameObject is shown.
/// Designed for win/lose panels or other UI popups.
/// </summary>
public class PanelAudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clip;
    [SerializeField] private bool playOnEnable = true;
    [SerializeField] private bool loopClip = false;
    [SerializeField] private bool stopOnDisable = true;
    [SerializeField] private float playDelay = 0f;

    private Coroutine playRoutine;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void OnEnable()
    {
        if (playOnEnable)
        {
            Play();
        }
    }

    private void OnDisable()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        if (stopOnDisable && audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    /// <summary>
    /// Plays the configured clip (optionally delayed).
    /// </summary>
    public void Play()
    {
        if (clip == null || audioSource == null)
        {
            return;
        }

        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
        }

        playRoutine = StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        if (playDelay > 0f)
        {
            yield return new WaitForSeconds(playDelay);
        }

        audioSource.loop = loopClip;

        if (loopClip)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            audioSource.PlayOneShot(clip);
        }

        playRoutine = null;
    }
}

