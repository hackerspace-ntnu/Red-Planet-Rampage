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
    // Exposed Audiomixer variable names
    private const string audioGroupMaster = "masterVolume";
    private const string audioGroupMusic = "musicVolume";
    private const string audioGroupSFX = "sfxVolume";

    private float maxVolumeMusic;
    private float maxVolumeSFX;

    [SerializeField]
    private AudioMixer mainAudioMixer;

    #endregion
    #region Sensitivity variables
    [SerializeField]
    private Slider sensitivitySlider;
    private float lowerLimit, upperLimit;

    [SerializeField]
    private TMP_InputField sensitivityInputField;
    #endregion

    private InputManager playerInput;

    void Awake()
    {
        CheckResolutions();
        CheckQuality();
        mainAudioMixer.GetFloat(audioGroupMusic, out float musicVolume);
        maxVolumeMusic = musicVolume;
        mainAudioMixer.GetFloat(audioGroupSFX, out float sfxVolume);
        maxVolumeSFX = sfxVolume;

        lowerLimit = sensitivitySlider.minValue; upperLimit = sensitivitySlider.maxValue;
    }


    /// <summary>
    /// Determine the different resolutions the display supports
    /// </summary>
    private void CheckResolutions()
    {
        resolutions = Screen.resolutions.Reverse().ToArray();
        resolutionDropdown.AddOptions(resolutions.Select(r => $"{r.width} x {r.height}").ToList());
        resolutionDropdown.value = System.Array.FindIndex(resolutions, r => r.width == Screen.currentResolution.width && r.height == Screen.currentResolution.height);
        resolutionDropdown.RefreshShownValue();
    }

    private void CheckQuality()
    {
        // We invert this list so the dropdown goes from high to low quality.
        qualityNames = QualitySettings.names;
        qualityDropdown.AddOptions(qualityNames.Reverse().ToList());
        qualityDropdown.value = QualitySettings.GetQualityLevel() - qualityNames.Length - 1;
        qualityDropdown.RefreshShownValue();
    }

    private float LinearToLogarithmicVolume(float volume)
    {
        return 20 * (Mathf.Log10(10 * Mathf.Max(volume, .0001f)) - 1);
    }

    public void SetMasterVolume(float volume)
    {
        mainAudioMixer.SetFloat(audioGroupMaster, LinearToLogarithmicVolume(volume));
    }

    public void SetMusicVolume(float volume)
    {
        mainAudioMixer.SetFloat(audioGroupMusic, LinearToLogarithmicVolume(volume) + maxVolumeMusic);
    }

    public void SetSFXVolume(float volume)
    {
        mainAudioMixer.SetFloat(audioGroupSFX, LinearToLogarithmicVolume(volume) + maxVolumeSFX);
    }

    public void SetQualityLevel(int qualityPresetIndex)
    {
        QualitySettings.SetQualityLevel(qualityNames.Length - qualityPresetIndex - 1);
    }

    public void SetResolution(int dropdownIndex)
    {
        var resolution = resolutions[dropdownIndex];
        Screen.SetResolution(resolution.width, resolution.height, true);
    }

    public void SetDisplayMode(int displayMode)
    {
        Screen.fullScreenMode = (FullScreenMode)displayMode;
    }

    public void CheckSensInputValue(string inputValue)
    {
        sensitivityInputField.text = ClampSensValue(float.Parse(inputValue)).ToString("0.00");
    }

    public void ChangeSensInputField(float value)
    {
        sensitivityInputField.text = value.ToString("0.00");
        SetSensMultiplier(value);
    }

    private float ClampSensValue(float inputValue)
    {
        return Mathf.Clamp(inputValue, lowerLimit, upperLimit);
    }

    public void ChangeSensSliderValue(string inputValue)
    {
        float value = ClampSensValue(float.Parse(inputValue));
        sensitivitySlider.value = value;
        SetSensMultiplier(value);
    }

    public void SetSensMultiplier(float multiplier)
    {
        playerInput.adjustScaleMulti = multiplier;
    }

    public void SetPlayerInput(InputManager inputManager)
    {
        playerInput = inputManager;
    }
}
