using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

    [SerializeField]
    private AudioMixer mainAudioMixer;

    #endregion

    void Awake()
    {
        CheckResolutions();
        CheckQuality();
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
    }

    public void SetMasterVolume(float volume)
    {
        mainAudioMixer.SetFloat(audioGroupMaster, volume);
    }

    public void SetMusicVolume(float volume)
    {
        mainAudioMixer.SetFloat(audioGroupMusic, volume);
    }

    public void SetSFXVolume(float volume)
    {
        mainAudioMixer.SetFloat(audioGroupSFX, volume);
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
}
