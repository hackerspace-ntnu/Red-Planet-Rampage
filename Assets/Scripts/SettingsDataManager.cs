using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class SettingsData
{
    public float SensitivityScale { get; internal set; }
    public float PlayerFOV { get; internal set; }
    public float ZoomFOV { get; internal set; }
    public float MasterVolume { get; internal set; }
    public float MusicVolume { get; internal set; }
    public float SfxVolume { get; internal set; }
    public int QualityPresetIndex { get; internal set; }
    public int DisplayModeIndex { get; internal set; }

    /// <summary>
    /// Constructor initialized with given parameter values.
    /// </summary>
    /// <param name="sensitivityScaleVal"></param>
    /// <param name="masterVolumeVal"></param>
    /// <param name="musicVolumeVal"></param>
    /// <param name="sfxVolumeVal"></param>
    /// <param name="qualityPresetIndexVal"></param>
    /// <param name="displayModeIndexVal"></param>
    public SettingsData(
        float sensitivityScaleVal, float playerFOV, float zoomFOV,
        float masterVolumeVal, float musicVolumeVal, float sfxVolumeVal,
        int qualityPresetIndexVal, int displayModeIndexVal)
    {
        SensitivityScale = sensitivityScaleVal;
        PlayerFOV = playerFOV;
        ZoomFOV = zoomFOV;
        MasterVolume = masterVolumeVal;
        MusicVolume = musicVolumeVal;
        SfxVolume = sfxVolumeVal;
        QualityPresetIndex = qualityPresetIndexVal;
        DisplayModeIndex = displayModeIndexVal;
    }

    /// <summary>
    /// Default constructor for SettingsData. Set with default values.
    /// </summary>
    public SettingsData()
    {
        SensitivityScale = 1.0f;
        PlayerFOV = 90f;
        ZoomFOV = 30f;
        MasterVolume = 1.0f;
        MusicVolume = 1.0f;
        SfxVolume = 1.0f;
        QualityPresetIndex = 0;
    }

    public SettingsData(SettingsDataStruct settingsData)
    {
        SensitivityScale = settingsData.SensitivityScale;
        PlayerFOV = settingsData.PlayerFOV;
        ZoomFOV = settingsData.ZoomFOV;
        MasterVolume = settingsData.MasterVolume;
        MusicVolume = settingsData.MusicVolume;
        SfxVolume = settingsData.SfxVolume;
        QualityPresetIndex = settingsData.QualityPresetIndex;
        DisplayModeIndex = settingsData.DisplayModeIndex;
    }

    public SettingsDataStruct ToDataStruct()
    {
        return new()
        {
            SensitivityScale = SensitivityScale,
            PlayerFOV = PlayerFOV,
            ZoomFOV = ZoomFOV,
            MasterVolume = MasterVolume,
            MusicVolume = MusicVolume,
            SfxVolume = SfxVolume,
            QualityPresetIndex = QualityPresetIndex,
            DisplayModeIndex = DisplayModeIndex
        };
    }
}

public struct SettingsDataStruct
{
    public float SensitivityScale;
    public float PlayerFOV;
    public float ZoomFOV;
    public float MasterVolume;
    public float MusicVolume;
    public float SfxVolume;
    public int QualityPresetIndex;
    public int DisplayModeIndex;
}

public class SettingsDataManager : MonoBehaviour
{
    public static SettingsDataManager Singleton { get; private set; }

    private const string FileName = "/Settings.json";

    private static string FilePath;


    #region Graphic variables
    public Resolution[] Resolutions { get; private set; }
    public string[] QualityNames { get; private set; }
    #endregion


    #region Audio variables
    private const string audioGroupMaster = "masterVolume";
    private const string audioGroupMusic = "musicVolume";
    private const string audioGroupSFX = "sfxVolume";

    // Adjust these based on volume.
    // TODO determine why Awake() is called multiple times here smh
    private const float maxVolumeMaster = 0;
    private const float maxVolumeMusic = -4;
    private const float maxVolumeSFX = 0;

    [SerializeField]
    private AudioMixer mainAudioMixer;
    #endregion


    #region Gameplay variables

    [Header("Sensitivity Limits")]
    public float LowerSensLimit = 0.1f;
    public float UpperSensLimit = 3f;

    [Header("FOV Limits")]
    public float LowerFOVLimit = 60f;
    public float UpperFOVLimit = 120f;

    [Header("Zoom FOV Limits")]
    public float LowerZoomFOVLimit = 20f;
    public float UpperZoomFOVLimit = 50f;

    #endregion

    public SettingsData SettingsDataInstance;

    private void Awake()
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

        Resolutions = Screen.resolutions.Reverse().ToArray();
        QualityNames = QualitySettings.names;

        SettingsDataInstance = new();

        DontDestroyOnLoad(gameObject);

        LoadOrCreateFile();
    }

    private void LoadOrCreateFile()
    {
        FilePath = Application.persistentDataPath + FileName;

        if (!File.Exists(FilePath))
        {
            CreateDefaultFile();
        }
        LoadSettingsFile();
        ApplyAllSettings();
    }

    #region Save methods
    private void LoadSettingsFile()
    {
        try
        {
            string jsonData = File.ReadAllText(FilePath);
            SettingsDataInstance = new SettingsData(JsonUtility.FromJson<SettingsDataStruct>(jsonData));
            Debug.Log("Settings data loaded");
        }
        catch
        {
            Debug.Log("Settings file corrupted or missing");
            CreateDefaultFile();
        }
    }

    private void CreateDefaultFile()
    {
        string jsonData = JsonUtility.ToJson(SettingsDataInstance.ToDataStruct());
        File.WriteAllText(FilePath, jsonData);
    }

    public void SaveSettingsData()
    {
        CreateDefaultFile();
    }

    public void ApplyAllSettings()
    {
        SetDisplayMode(SettingsDataInstance.DisplayModeIndex);
        SetQualityLevel(SettingsDataInstance.QualityPresetIndex);
        SetMasterVolume(SettingsDataInstance.MasterVolume);
        SetMusicVolume(SettingsDataInstance.MusicVolume);
        SetSFXVolume(SettingsDataInstance.SfxVolume);
    }
    #endregion


    #region Audio methods
    private float LinearToLogarithmicVolume(float volume)
    {
        return 20 * (Mathf.Log10(10 * Mathf.Max(volume, .0001f)) - 1);
    }

    public void SetMasterVolume(float volume)
    {
        SettingsDataInstance.MasterVolume = Mathf.Clamp(volume, 0f, 1f);
        mainAudioMixer.SetFloat(audioGroupMaster, LinearToLogarithmicVolume(SettingsDataInstance.MasterVolume) + maxVolumeMaster);
    }

    public void SetMusicVolume(float volume)
    {
        SettingsDataInstance.MusicVolume = Mathf.Clamp(volume, 0f, 1f);
        mainAudioMixer.SetFloat(audioGroupMusic, LinearToLogarithmicVolume(SettingsDataInstance.MusicVolume) + maxVolumeMusic);
    }

    public void SetSFXVolume(float volume)
    {
        SettingsDataInstance.SfxVolume = Mathf.Clamp(volume, 0f, 1f);
        mainAudioMixer.SetFloat(audioGroupSFX, LinearToLogarithmicVolume(SettingsDataInstance.SfxVolume) + maxVolumeSFX);
    }
    #endregion


    #region Graphic methods
    public void SetQualityLevel(int index)
    {
        SettingsDataInstance.QualityPresetIndex = Math.Clamp(index, 0, QualityNames.Length - 1);
        QualitySettings.SetQualityLevel(QualityNames.Length - index - 1);
    }

    public void SetResolutionLevel(int index)
    {
        var resolution = Resolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, true);
    }

    public void SetDisplayMode(int index)
    {
        // A constant 3 because of the dropdown's children.
        SettingsDataInstance.DisplayModeIndex = Math.Clamp(index, 0, 3);
        Screen.fullScreenMode = (FullScreenMode)index;
    }
    #endregion


    #region Gameplay methods
    public float ClampSensValue(float value)
    {
        return Mathf.Clamp(value, LowerSensLimit, UpperSensLimit);
    }
    public void SetSensMultiplier(float scale)
    {
        SettingsDataInstance.SensitivityScale = Mathf.Max(scale, 0.1f);
    }

    public float ClampFOVValue(float value)
    {
        return Mathf.Clamp(value, LowerFOVLimit, UpperFOVLimit);
    }

    public float ClampZoomFOVValue(float value)
    {
        return Mathf.Clamp(value, LowerZoomFOVLimit, UpperZoomFOVLimit);
    }

    public void SetFOV(float fov)
    {
        SettingsDataInstance.PlayerFOV = Mathf.Clamp(fov, 1f, 179f);
    }

    public void SetZoomFOV(float zoomFOV)
    {
        SettingsDataInstance.ZoomFOV = Mathf.Clamp(zoomFOV, 1f, 179f);
    }
    #endregion
}
