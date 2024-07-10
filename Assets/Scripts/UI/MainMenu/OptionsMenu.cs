using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    #region Graphics variables
    [Header("Graphics")]

    [SerializeField]
    private TMP_Dropdown resolutionDropdown;

    // The resolutions available as this menu starts its lifetime.
    Resolution[] resolutions;

    [SerializeField]
    private TMP_Dropdown fullscreenTypeDropdown;

    [SerializeField]
    private TMP_Dropdown qualityDropdown;

    private string[] qualityNames;

    #endregion
    #region Audio Variables
    [Header("Audio")]

    [SerializeField]
    private Slider masterVolumeSlider;
    [SerializeField]
    private Slider musicVolumeSlider;
    [SerializeField]
    private Slider sfxVolumeSlider;

    #endregion
    #region Sensitivity variables
    [Header("Sens Elements")]
    [SerializeField]
    private Slider sensitivitySlider;
    [SerializeField]
    private TMP_InputField sensitivityInputField;

    [Header("FOV Elements")]
    [SerializeField]
    private TMP_InputField FOVInputField;
    [SerializeField]
    private TMP_InputField ZoomFOVInputField;
    [SerializeField]
    private Slider FOVSlider;
    [SerializeField]
    private Slider ZoomFOVSlider;

    #endregion

    [SerializeField]
    private SettingsInfo settingsInfo;

    private void Start()
    {
        CheckResolutions();
        CheckQuality();

        settingsInfo = SettingsInfo.Singleton;
        sensitivitySlider.minValue = settingsInfo.lowerSensLimit;
        sensitivitySlider.maxValue = settingsInfo.upperSensLimit;

        FOVSlider.minValue = settingsInfo.lowerFOVLimit;
        FOVSlider.maxValue = settingsInfo.upperFOVLimit;

        ZoomFOVSlider.minValue = settingsInfo.lowerZoomFOVLimit;
        ZoomFOVSlider.maxValue = settingsInfo.upperZoomFOVLimit;

        SetOptionItems();
    }

    private void SetOptionItems()
    {
        resolutionDropdown.value = settingsInfo.settings.resolutionPresetIndex;

        fullscreenTypeDropdown.value = settingsInfo.settings.resolutionPresetIndex;

        qualityDropdown.value = settingsInfo.settings.qualityPresetIndex;

        masterVolumeSlider.value = settingsInfo.settings.masterVolume;

        musicVolumeSlider.value = settingsInfo.settings.musicVolume;

        sfxVolumeSlider.value = settingsInfo.settings.sfxVolume;

        sensitivitySlider.value = settingsInfo.settings.sensScale;
        sensitivityInputField.text = settingsInfo.settings.sensScale.ToString("0.00");

        FOVSlider.value = settingsInfo.settings.playerFOV;
        FOVInputField.text = settingsInfo.settings.playerFOV.ToString("0.00");

        ZoomFOVSlider.value = settingsInfo.settings.zoomFOV;
        ZoomFOVInputField.text = settingsInfo.settings.zoomFOV.ToString("0.00");

        settingsInfo.ApplyAllSettings();
    }

    /// <summary>
    /// Determine the different resolutions the display supports
    /// </summary>
    private void CheckResolutions()
    {
        Resolution[] resolutions = settingsInfo.resolutions;
        resolutionDropdown.AddOptions(resolutions.Select(r => $"{r.width} x {r.height}").ToList());
        resolutionDropdown.value = System.Array.FindIndex(resolutions, r => r.width == Screen.currentResolution.width && r.height == Screen.currentResolution.height);
        resolutionDropdown.RefreshShownValue();
    }

    private void CheckQuality()
    {
        // We invert this list so the dropdown goes from high to low quality.
        string[] qualityNames = settingsInfo.qualityNames;
        qualityDropdown.AddOptions(qualityNames.Reverse().ToList());
        qualityDropdown.value = QualitySettings.GetQualityLevel() - qualityNames.Length - 1;
        qualityDropdown.RefreshShownValue();
    }

    public void SetMasterVolume(float volume)
    {
        settingsInfo.SetMasterVolume(volume);
    }

    public void SetMusicVolume(float volume)
    {
        settingsInfo.SetMusicVolume(volume);
        
    }

    public void SetSFXVolume(float volume)
    {
        settingsInfo.SetSFXVolume(volume);
    }

    public void SetQualityLevel(int qualityPresetIndex)
    {
        settingsInfo.SetQualityLevel(qualityPresetIndex);
    }

    public void SetResolution(int dropdownIndex)
    {
        settingsInfo.SetResolutionLevel(dropdownIndex);
    }

    public void SetDisplayMode(int displayMode)
    {
        settingsInfo.SetDisplayMode(displayMode);
    }

    public void CheckSensInputValue(string inputValue)
    {
        sensitivityInputField.text = settingsInfo.ClampSensValue(float.Parse(inputValue)).ToString("0.00");
    }

    public void ChangeSensInputField(float value)
    {
        sensitivityInputField.text = value.ToString("0.00");
        settingsInfo.SetSensMultiplier(value);
    }

    public void ChangeSensSliderValue(string inputValue)
    {
        float value = settingsInfo.ClampSensValue(float.Parse(inputValue));
        sensitivitySlider.value = value;
        settingsInfo.SetSensMultiplier(value);
    }

    public void CheckFOVInputValue(string inputValue)
    {
        FOVInputField.text = settingsInfo.ClampFOVValue(float.Parse(inputValue)).ToString("0.00");
    }

    public void CheckZoomFOVInputValue(string inputValue)
    {
        ZoomFOVInputField.text = settingsInfo.ClampZoomFOVValue(float.Parse(inputValue)).ToString("0.00");
    }

    public void ChangeFOVInputField(float value)
    {
        FOVInputField.text = value.ToString("0.00");
        settingsInfo.SetFOV(value);
    }

    public void ChangeZoomFOVInputField(float value)
    {
        ZoomFOVInputField.text = value.ToString("0.00");
        settingsInfo.SetZoomFOV(value);
    }

    public void ChangeFOVSliderValue(string inputValue)
    {
        float value = settingsInfo.ClampFOVValue(float.Parse(inputValue));
        FOVSlider.value = value;
        settingsInfo.SetFOV(value);
    }

    public void ChangeZoomFOVSliderValue(string inputValue)
    {
        float value = settingsInfo.ClampZoomFOVValue(float.Parse(inputValue));
        ZoomFOVSlider.value = value;
        settingsInfo.SetZoomFOV(value);
    }
}
