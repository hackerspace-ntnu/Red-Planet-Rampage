using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    // THIS NAME NEEEEEEDS TO MATCH THE GIVEN NAME OF THE EXPOSED AUDIO MIXER VARIABLE
    private const string AUDIO_MIXER_MASTER_VOLUME = "AUDIO_MASTER_VOLUME";

    [SerializeField]
    private AudioMixer mainAudioMixer;

    [SerializeField]
    private TMP_Dropdown resolutionDropdown;

    // The resolutions available as this menu starts its lifetime.
    Resolution[] resolutions;

    bool fullscreen;

    // Start is called before the first frame update
    void Start()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(resolutions.Select(r => $"{r.width} x {r.height}").ToList());
        resolutionDropdown.value = System.Array.FindIndex(resolutions, r => r.width == Screen.currentResolution.width && r.height == Screen.currentResolution.height);
        resolutionDropdown.RefreshShownValue();
    }

    public void SetMasterVolume(float volume)
    {
        mainAudioMixer.SetFloat(AUDIO_MIXER_MASTER_VOLUME, volume);
    }
    public void SetQualityPreset(int qualityPresetIndex)
    {
        QualitySettings.SetQualityLevel(qualityPresetIndex);
    }
    public void SetResolutionFromDropdown(int dropdownIndex)
    {
        var resolution = resolutions[dropdownIndex];
        Screen.SetResolution(resolution.width, resolution.height, true);
    }
}
