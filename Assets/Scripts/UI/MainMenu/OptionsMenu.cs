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

    [SerializeField]
    private TMP_Dropdown fullscreenTypeDropdown;

    [SerializeField]
    private TMP_Dropdown qualityDropdown;
    [SerializeField]
    private Slider CrosshairSlider;
    [SerializeField]
    private TMP_InputField CrosshairInputField;

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

    private SettingsDataManager settingsDataManager;

    private void Start()
    {
        settingsDataManager = SettingsDataManager.Singleton;

        CheckResolutions();
        CheckQuality();

        sensitivitySlider.minValue = settingsDataManager.LowerSensLimit;
        sensitivitySlider.maxValue = settingsDataManager.UpperSensLimit;

        FOVSlider.minValue = settingsDataManager.LowerFOVLimit;
        FOVSlider.maxValue = settingsDataManager.UpperFOVLimit;

        ZoomFOVSlider.minValue = settingsDataManager.LowerZoomFOVLimit;
        ZoomFOVSlider.maxValue = settingsDataManager.UpperZoomFOVLimit;

        CrosshairSlider.minValue = settingsDataManager.LowerCrosshairLimit;
        CrosshairSlider.maxValue = settingsDataManager.UpperCrosshairLimit;

        SetOptionItems();
    }

    private void SetOptionItems()
    {
        resolutionDropdown.value = System.Array.FindIndex(settingsDataManager.Resolutions, r => r.width == Screen.currentResolution.width && r.height == Screen.currentResolution.height);

        fullscreenTypeDropdown.value = settingsDataManager.SettingsDataInstance.DisplayModeIndex;

        qualityDropdown.value = settingsDataManager.SettingsDataInstance.QualityPresetIndex;

        masterVolumeSlider.value = settingsDataManager.SettingsDataInstance.MasterVolume;

        musicVolumeSlider.value = settingsDataManager.SettingsDataInstance.MusicVolume;

        sfxVolumeSlider.value = settingsDataManager.SettingsDataInstance.SfxVolume;

        sensitivitySlider.value = settingsDataManager.SettingsDataInstance.SensitivityScale;
        sensitivityInputField.text = settingsDataManager.SettingsDataInstance.SensitivityScale.ToString("0.00");

        FOVSlider.value = settingsDataManager.SettingsDataInstance.PlayerFOV;
        FOVInputField.text = settingsDataManager.SettingsDataInstance.PlayerFOV.ToString("0");

        ZoomFOVSlider.value = settingsDataManager.SettingsDataInstance.ZoomFOV;
        ZoomFOVInputField.text = settingsDataManager.SettingsDataInstance.ZoomFOV.ToString("0");

        CrosshairSlider.value = settingsDataManager.SettingsDataInstance.CrosshairSize;
        CrosshairInputField.text = settingsDataManager.SettingsDataInstance.CrosshairSize.ToString("0.00");

        settingsDataManager.ApplyAllSettings();
    }

    /// <summary>
    /// Determine the different resolutions the display supports
    /// </summary>
    private void CheckResolutions()
    {
        Resolution[] resolutions = settingsDataManager.Resolutions;
        resolutionDropdown.AddOptions(resolutions.Select(r => $"{r.width} x {r.height}").ToList());
        resolutionDropdown.value = System.Array.FindIndex(resolutions, r => r.width == Screen.currentResolution.width && r.height == Screen.currentResolution.height);
        resolutionDropdown.RefreshShownValue();
    }

    private void CheckQuality()
    {
        // We invert this list so the dropdown goes from high to low quality.
        string[] qualityNames = settingsDataManager.QualityNames;
        qualityDropdown.AddOptions(qualityNames.Reverse().ToList());
        qualityDropdown.value = settingsDataManager.SettingsDataInstance.QualityPresetIndex;
        qualityDropdown.RefreshShownValue();
    }

    public void SetMasterVolume(float volume)
    {
        settingsDataManager.SetMasterVolume(volume);
    }

    public void SetMusicVolume(float volume)
    {
        settingsDataManager.SetMusicVolume(volume);
        
    }

    public void SetSFXVolume(float volume)
    {
        settingsDataManager.SetSFXVolume(volume);
    }

    public void SetQualityLevel(int qualityPresetIndex)
    {
        settingsDataManager.SetQualityLevel(qualityPresetIndex);
    }

    public void SetResolution(int dropdownIndex)
    {
        settingsDataManager.SetResolutionLevel(dropdownIndex);
    }

    public void SetDisplayMode(int displayMode)
    {
        settingsDataManager.SetDisplayMode(displayMode);
    }

    public void CheckSensInputValue(string inputValue)
    {
        sensitivityInputField.text = settingsDataManager.ClampSensValue(float.Parse(inputValue)).ToString("0.00");
    }

    public void ChangeSensInputField(float value)
    {
        sensitivityInputField.text = value.ToString("0.00");
        settingsDataManager.SetSensMultiplier(value);
    }

    public void ChangeSensSliderValue(string inputValue)
    {
        float value = settingsDataManager.ClampSensValue(float.Parse(inputValue));
        sensitivitySlider.value = value;
        settingsDataManager.SetSensMultiplier(value);
    }

    public void CheckFOVInputValue(string inputValue)
    {
        FOVInputField.text = settingsDataManager.ClampFOVValue(float.Parse(inputValue)).ToString("0");
    }

    public void CheckZoomFOVInputValue(string inputValue)
    {
        ZoomFOVInputField.text = settingsDataManager.ClampZoomFOVValue(float.Parse(inputValue)).ToString("0");
    }

    public void ChangeFOVInputField(float value)
    {
        FOVInputField.text = value.ToString("0");
        settingsDataManager.SetFOV(value);
    }

    public void ChangeZoomFOVInputField(float value)
    {
        ZoomFOVInputField.text = value.ToString("0");
        settingsDataManager.SetZoomFOV(value);
    }

    public void ChangeFOVSliderValue(string inputValue)
    {
        float value = settingsDataManager.ClampFOVValue(float.Parse(inputValue));
        FOVSlider.value = value;
        settingsDataManager.SetFOV(value);
    }

    public void ChangeZoomFOVSliderValue(string inputValue)
    {
        float value = settingsDataManager.ClampZoomFOVValue(float.Parse(inputValue));
        ZoomFOVSlider.value = value;
        settingsDataManager.SetZoomFOV(value);
    }

    public void ChangeCrosshairInputField(float value)
    {
        CrosshairInputField.text = value.ToString("0.00");
        settingsDataManager.SetCrosshairSize(value);
    }

    public void CheckCrosshairInputValue(string inputValue)
    {
        CrosshairInputField.text = settingsDataManager.ClampCrosshairSize(float.Parse(inputValue)).ToString("0.00");
    }

    public void ChangeCrosshairSliderValue(string inputValue)
    {
        float value = settingsDataManager.ClampCrosshairSize(float.Parse(inputValue));
        CrosshairSlider.value = value;
        settingsDataManager.SetCrosshairSize(value);
    }
}
