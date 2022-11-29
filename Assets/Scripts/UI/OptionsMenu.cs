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
    #region Resolution variables
    [Header("Resolution")]
    
    [SerializeField]
    private TMP_Dropdown resolutionDropdown;
    
    [SerializeField]
    private TMP_Dropdown fullscreenTypeDropdown;

    // The resolutions available as this menu starts its lifetime.
    Resolution[] resolutions;

    #endregion
    #region Audio Variables
    [Header("Audio")]
    /*
    [SerializeField]
    private Slider mainVolSlider;

    [SerializeField]
    private Slider musicVolSlider;

    [SerializeField]
    private Slider sfxVolSlider;
    */
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
    public void SetQualityPreset(int qualityPresetIndex)
    {
        QualitySettings.SetQualityLevel(qualityPresetIndex);
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
