using UnityEngine;
using System.Collections;

public class SceneUIController : MonoBehaviour
{
    private AudioSource audioSource;
    //Specified audio clip
    public AudioClip buttonHover;
    public AudioClip campaignHover;

    //Music setting
    private bool isOn = true;
    //Sound setting
    private bool isMOn = true;
    //Setting panel
    public GameObject settingBoard;   //For Map

    private bool settingState = false;
    public GameObject music;         //For Menu
    public GameObject sound;        //For Menu
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.Play(); // Start music
        Debug.Log("it will run");

    }

    public void FadeOutAndStop(float duration)
    {
        StartCoroutine(FadeOutCoroutine(duration));
    }

    private IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = audioSource.volume;

        // Gradually lower the volume
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume; // reset for next play
    }
    public void ButtonHover()            //Sound when hover button
    {
        if (isMOn == true)
        { audioSource.PlayOneShot(buttonHover, 1.0f); }
    }

    public void CampaignHover()
    {
        if (isMOn == true)
        { audioSource.PlayOneShot(campaignHover, 1.0f); }
    }

    public void MusicSet()             //Sound when setting music
    {
        if (isOn) { audioSource.Stop(); isOn = false; }
        else { audioSource.Play(); isOn = true; }
    }

    public void SoundSet()         //Setting game sound
    {
        isMOn = !isMOn;
    }
    //Menu setting open/off
    public void SettingOpen()
    {
        settingBoard.SetActive(true);
        settingBoard.GetComponent<SmoothMove>().PlaySlideIn();
    }
    public void SettingCancel()
    {
        settingBoard.SetActive(false);
    }
    //Map setting open/off
    public void CheckSetting()
    {
        if (!settingState) { SetOpen1(); settingState = true; }
        else { SetCancel1(); settingState = false; }
    }
    public void SetOpen1()
    {
        music.SetActive(true);
        sound.SetActive(true);
        music.GetComponent<SmoothMoveSet>().PlaySlideIn(-303);
        sound.GetComponent<SmoothMoveSet>().PlaySlideIn(-231);
    }
    public void SetCancel1()
    {
        music.GetComponent<SmoothMoveSet>().PlaySlideIn(-353f);
        sound.GetComponent<SmoothMoveSet>().PlaySlideIn(-353f);
        music.SetActive(false);
        sound.SetActive(false);
    }
}
