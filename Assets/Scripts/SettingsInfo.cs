using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public struct Settings
{
    // Audio
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;

    // Graphics
    public int qualityPresetIndex;
    public int resolutionPresetIndex;
    public int displayMode;

    // Gameplay
    public float sensScale;
    public float playerFOV;
    public float zoomFOV;
}

public class SettingsInfo : MonoBehaviour
{
    public static SettingsInfo Singleton { get; private set; }

    private const string FileName = "/Settings.json";

    private static string FilePath;
    #region Graphics variables
    public Resolution[] resolutions { get; private set; }

    public string[] qualityNames { get; private set; }
    #endregion
    #region Audio variables
    private const string audioGroupMaster = "masterVolume";
    private const string audioGroupMusic = "musicVolume";
    private const string audioGroupSFX = "sfxVolume";

    private float maxVolumeMusic;
    private float maxVolumeSFX;

    [SerializeField]
    private AudioMixer mainAudioMixer;
    #endregion
    #region Gameplay variables

    [Header("Sensitivity Limits")]
    public float lowerSensLimit = 0.1f;
    public float upperSensLimit = 3f;

    [Header("FOV Limits")]
    public float lowerFOVLimit = 60f;
    public float upperFOVLimit = 120f;

    [Header("Zoom FOV Limits")]
    public float lowerZoomFOVLimit = 20f;
    public float upperZoomFOVLimit = 50f;

    #endregion
    public Settings settings = new()
    {
        sensScale = 1.0f,
        masterVolume = 1.0f,
        musicVolume = 1.0f,
        sfxVolume = 1.0f,
        qualityPresetIndex = 0,
        resolutionPresetIndex = 0,
        displayMode = 0,
    };

    private void Awake()
    {
        resolutions = Screen.resolutions.Reverse().ToArray();
        qualityNames = QualitySettings.names;

        mainAudioMixer.GetFloat(audioGroupMusic, out float musicVolume);
        maxVolumeMusic = musicVolume;
        mainAudioMixer.GetFloat(audioGroupSFX, out float sfxVolume);
        maxVolumeSFX = sfxVolume;
    }

    void Start()
    {
        #region Singleton boilerplate

        if (Singleton != null)
        {
            if (Singleton != this)
            {
                Debug.LogWarning($"There's more than one {Singleton.GetType()} in the scene!");
                Destroy(gameObject);
            }

            return;
        }

        Singleton = this;

        #endregion Singleton boilerplate

        FilePath = Application.persistentDataPath + FileName;

        if (!File.Exists(FilePath))
        {
            CreateDefaultFile();
        }
        LoadSettingsFile();
        ApplyAllSettings();

        DontDestroyOnLoad(gameObject);  
    }
    #region Save methods
    private void LoadSettingsFile()
    {
        try
        {
            string jsonData = File.ReadAllText(FilePath);
            settings = JsonUtility.FromJson<Settings>(jsonData);
            Debug.Log("Settings data loaded");
            ValidateSettings();
        }
        catch
        {
            Debug.Log("Settings file corrupted or missing");
            CreateDefaultFile();
        }
    }

    private void ValidateSettings()
    {
        // Check limits for Audio
        Mathf.Clamp(settings.masterVolume, 0f, 1f);
        Mathf.Clamp(settings.musicVolume, 0f, 1f);
        Mathf.Clamp(settings.sfxVolume, 0f, 1f);

        // Check limits for Graphics
        Math.Clamp(settings.resolutionPresetIndex, 0, resolutions.Length - 1);
        Math.Clamp(settings.qualityPresetIndex, 0, qualityNames.Length - 1);
        Math.Clamp(settings.displayMode, 0, 3);

        // Check lower limits for Gameplay
        if (settings.sensScale <= 0f)
            settings.sensScale = 0.01f;

        if (settings.playerFOV < 20f)
            settings.playerFOV = 20f;

        if (settings.zoomFOV < 5f)
            settings.zoomFOV = 5f;
    }

    private void CreateDefaultFile()
    {
        string jsonData = JsonUtility.ToJson(settings);
        File.WriteAllText(FilePath, jsonData);
    }

    public void SaveSettingsData()
    {
        CreateDefaultFile();
    }

    public void ApplyAllSettings()
    {
        SetResolutionLevel(settings.resolutionPresetIndex);
        SetDisplayMode(settings.resolutionPresetIndex);
        SetQualityLevel(settings.qualityPresetIndex);
        SetMasterVolume(settings.masterVolume);
        SetMusicVolume(settings.musicVolume);
    }
    #endregion
    #region Audio methods
    private float LinearToLogarithmicVolume(float volume)
    {
        return 20 * (Mathf.Log10(10 * Mathf.Max(volume, .0001f)) - 1);
    }

    public void SetMasterVolume(float volume)
    {
        settings.masterVolume = volume;
        mainAudioMixer.SetFloat(audioGroupMaster, LinearToLogarithmicVolume(settings.masterVolume));
    }

    public void SetMusicVolume(float volume)
    {
        settings.musicVolume = volume;
        mainAudioMixer.SetFloat(audioGroupMusic, LinearToLogarithmicVolume(settings.musicVolume) + maxVolumeMusic);
    }

    public void SetSFXVolume(float volume)
    {
        settings.sfxVolume = volume;
        mainAudioMixer.SetFloat(audioGroupSFX, LinearToLogarithmicVolume(settings.sfxVolume) + maxVolumeSFX);
    }
    #endregion
    #region Graphic methods
    public void SetQualityLevel(int index)
    {
        settings.qualityPresetIndex = index;
        QualitySettings.SetQualityLevel(qualityNames.Length - index - 1);
    }

    public void SetResolutionLevel(int index)
    {
        settings.resolutionPresetIndex = index;
        var resolution = resolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, true);
    }

    public void SetDisplayMode(int index)
    {
        settings.displayMode = index;
        Screen.fullScreenMode = (FullScreenMode)index;
    }
    #endregion
    #region Gameplay methods
    public float ClampSensValue(float value)
    {
        return Mathf.Clamp(value, lowerSensLimit, upperSensLimit);
    }
    public void SetSensMultiplier(float scale)
    {
        settings.sensScale = scale;
    }

    public float ClampFOVValue(float value)
    {
        return Mathf.Clamp(value, lowerFOVLimit, upperFOVLimit);
    }

    public float ClampZoomFOVValue(float value)
    {
        return Mathf.Clamp(value, lowerZoomFOVLimit, upperZoomFOVLimit);
    }

    public void SetFOV(float fov)
    {
        settings.playerFOV = fov;
    }

    public void SetZoomFOV(float zoomFOV)
    {
        settings.zoomFOV = zoomFOV;
    }
    #endregion
}
