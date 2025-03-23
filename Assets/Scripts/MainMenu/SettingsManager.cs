using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public Slider volumeSlider;
    public Slider brightnessSlider;

    void Start()
    {
        // Load saved settings
        volumeSlider.value = PlayerPrefs.GetFloat("VolumeLevel", 1f); // Default 1 (max)
        brightnessSlider.value = PlayerPrefs.GetFloat("BrightnessLevel", 1f); // Default 1 (max)

        // Apply loaded settings
        AudioListener.volume = volumeSlider.value;
        SetBrightness(brightnessSlider.value);

        // Add listeners to sliders
        volumeSlider.onValueChanged.AddListener(SetVolume);
        brightnessSlider.onValueChanged.AddListener(SetBrightness);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("VolumeLevel", volume); // Save volume level
    }

    public void SetBrightness(float brightness)
    {
        // Adjust brightness using ambient light
        RenderSettings.ambientLight = Color.white * brightness;
        PlayerPrefs.SetFloat("BrightnessLevel", brightness); // Save brightness level
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}