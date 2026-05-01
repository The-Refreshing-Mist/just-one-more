using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public Slider volumeSlider;
    public Toggle fullscreenToggle;

    private void Start()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("Volume", 0.8f);
        fullscreenToggle.isOn = Screen.fullScreen;

        SetVolume(volumeSlider.value);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("Volume", volume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void BackToTitle()
    {
        SceneManager.LoadScene("TitleScreen");
    }
}