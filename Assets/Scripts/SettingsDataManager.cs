using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class SettingsData
{
    public float SensitivityScale { get; internal set; }
    public float PlayerFOV { get; internal set; }
    public float ZoomFOV { get; internal set; }
    public bool InvertX { get; internal set; } = false;
    public bool InvertY { get; internal set; } = false;
    public float MasterVolume { get; internal set; }
    public float MusicVolume { get; internal set; }
    public float SfxVolume { get; internal set; }
    public int QualityPresetIndex { get; internal set; }
    public int DisplayModeIndex { get; internal set; }
    public float CrosshairSize { get; internal set; }

    const float defaultSensitivityScale = 1f;
    const float defaultPlayerFOV = 90f;
    const float defaultZoomFOV = 30f;
    const float defaultMasterVolume = 1.0f;
    const float defaultMusicVolume = 0.6f;
    const float defaultSfxVolume = 1.0f;
    const int defaultQualityPresetIndex = 0;
    const float defaultCrosshairSize = 1f;

    /// <summary>
    /// Default constructor for SettingsData. Set with default values.
    /// </summary>
    public SettingsData()
    {
        SensitivityScale = defaultSensitivityScale;
        PlayerFOV = defaultPlayerFOV;
        ZoomFOV = defaultZoomFOV;
        MasterVolume = defaultMasterVolume;
        MusicVolume = defaultMusicVolume;
        SfxVolume = defaultSfxVolume;
        QualityPresetIndex = defaultQualityPresetIndex;
        CrosshairSize = defaultCrosshairSize;
    }

    public SettingsData(SettingsDataStruct settingsData)
    {
        SensitivityScale = settingsData.SensitivityScale;
        PlayerFOV = settingsData.PlayerFOV;
        ZoomFOV = settingsData.ZoomFOV;
        InvertY = settingsData.InvertY;
        InvertX = settingsData.InvertX;
        MasterVolume = settingsData.MasterVolume;
        MusicVolume = settingsData.MusicVolume;
        SfxVolume = settingsData.SfxVolume;
        QualityPresetIndex = settingsData.QualityPresetIndex;
        DisplayModeIndex = settingsData.DisplayModeIndex;
        // Settings added after this needs to be backwards compatible with players who saved old setttings
        // This can be achieved by checking for larger than 0
        CrosshairSize = settingsData.CrosshairSize > 0 ? settingsData.CrosshairSize : defaultCrosshairSize;
    }

    public SettingsDataStruct ToDataStruct()
    {
        return new()
        {
            SensitivityScale = SensitivityScale,
            PlayerFOV = PlayerFOV,
            ZoomFOV = ZoomFOV,
            InvertY = InvertY,
            InvertX = InvertX,
            MasterVolume = MasterVolume,
            MusicVolume = MusicVolume,
            SfxVolume = SfxVolume,
            QualityPresetIndex = QualityPresetIndex,
            DisplayModeIndex = DisplayModeIndex,
            CrosshairSize = CrosshairSize
        };
    }
}

public struct SettingsDataStruct
{
    public float SensitivityScale;
    public float PlayerFOV;
    public float ZoomFOV;
    public bool InvertY;
    public bool InvertX;
    public float MasterVolume;
    public float MusicVolume;
    public float SfxVolume;
    public int QualityPresetIndex;
    public int DisplayModeIndex;
    public float CrosshairSize;
}

public class SettingsDataManager : MonoBehaviour
{
    public static SettingsDataManager Singleton { get; private set; }

    private static string SettingsFilePath => $"{Application.persistentDataPath}/Settings.json";
    private static string KeybindsFilePath => $"{Application.persistentDataPath}/Keybinds.json";



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

    [Header("Crosshair Limits")]
    public float LowerCrosshairLimit = 0.01f;
    public float UpperCrosshairLimit = 3f;
    #endregion

    [Header("Keybinds")]
    [SerializeField]
    private InputActionAsset actions;

    public SettingsData Data = new();

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

        DontDestroyOnLoad(gameObject);

        LoadOrCreateSettingsFile();
        LoadKeybindsFile();
    }

    private void LoadOrCreateSettingsFile()
    {
        // TODO For some reason the settings will be loaded (and *need* to be loaded)
        //      every single time Mirror flings you back to the main menu.
        //      We may want to investigate why.

        if (!File.Exists(SettingsFilePath))
        {
            SaveSettingsFile();
        }
        LoadSettingsFile();
        ApplyAllSettings();
        StartCoroutine(MakeSureVolumeIsCorrectOnLaunch());
    }

    private void LoadKeybindsFile()
    {
        if (!File.Exists(KeybindsFilePath))
            return;

        var rebinds = File.ReadAllText(KeybindsFilePath);
        if (!string.IsNullOrEmpty(rebinds))
            actions.LoadBindingOverridesFromJson(rebinds);
    }

    #region Save methods
    private void LoadSettingsFile()
    {
        try
        {
            string jsonData = File.ReadAllText(SettingsFilePath);
            Data = new SettingsData(JsonUtility.FromJson<SettingsDataStruct>(jsonData));
            Debug.Log("Settings data loaded");
        }
        catch
        {
            Debug.Log("Settings file corrupted or missing");
            SaveSettingsFile();
        }
    }

    private void SaveSettingsFile()
    {
        string jsonData = JsonUtility.ToJson(Data.ToDataStruct());
        File.WriteAllText(SettingsFilePath, jsonData);
    }

    private void SaveKeybindsFile()
    {
        var rebinds = actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);
        File.WriteAllText(KeybindsFilePath, rebinds);
    }

    public void SaveSettingsData()
    {
        SaveSettingsFile();
        SaveKeybindsFile();
    }

    public void ApplyAllSettings()
    {
        SetDisplayMode(Data.DisplayModeIndex);
        SetQualityLevel(Data.QualityPresetIndex);
        SetMasterVolume(Data.MasterVolume);
        SetMusicVolume(Data.MusicVolume);
        SetSFXVolume(Data.SfxVolume);
    }

    /// <summary>
    /// Yes, this seems necessary.
    /// Unity just keeps its default volume levels if you don't set them *after* the first frame.
    /// </summary>
    private IEnumerator MakeSureVolumeIsCorrectOnLaunch()
    {
        yield return null;
        SetMasterVolume(Data.MasterVolume);
        SetMusicVolume(Data.MusicVolume);
        SetSFXVolume(Data.SfxVolume);
    }
    #endregion


    #region Audio methods
    private float LinearToLogarithmicVolume(float volume)
    {
        return 20 * (Mathf.Log10(10 * Mathf.Max(volume, .0001f)) - 1);
    }

    public void SetMasterVolume(float volume)
    {
        Data.MasterVolume = Mathf.Clamp(volume, 0f, 1f);
        mainAudioMixer.SetFloat(audioGroupMaster, LinearToLogarithmicVolume(Data.MasterVolume) + maxVolumeMaster);
    }

    public void SetMusicVolume(float volume)
    {
        Data.MusicVolume = Mathf.Clamp(volume, 0f, 1f);
        mainAudioMixer.SetFloat(audioGroupMusic, LinearToLogarithmicVolume(Data.MusicVolume) + maxVolumeMusic);
    }

    public void SetSFXVolume(float volume)
    {
        Data.SfxVolume = Mathf.Clamp(volume, 0f, 1f);
        mainAudioMixer.SetFloat(audioGroupSFX, LinearToLogarithmicVolume(Data.SfxVolume) + maxVolumeSFX);
    }
    #endregion


    #region Graphic methods
    public void SetQualityLevel(int index)
    {
        Data.QualityPresetIndex = Math.Clamp(index, 0, QualityNames.Length - 1);
        QualitySettings.SetQualityLevel(QualityNames.Length - index - 1);
    }

    public void SetResolutionLevel(int index)
    {
        var resolution = Resolutions[index];
        // Avoid changing resolution if it is set to the same already.
        // May cause glitchy-looking behaviour if we don't.
        if (!(Screen.currentResolution.width == resolution.width && Screen.currentResolution.height == resolution.height))
            Screen.SetResolution(resolution.width, resolution.height, true);
    }

    public void SetDisplayMode(int index)
    {
        // A constant 3 because of the dropdown's children.
        Data.DisplayModeIndex = Math.Clamp(index, 0, 3);
        var mode = (FullScreenMode)index;
        // Avoid changing fullscreen mode if it is set to the same already.
        // Causes glitchy-looking behaviour if we don't.
        if (Screen.fullScreenMode != mode)
            Screen.fullScreenMode = mode;
    }
    #endregion


    #region Gameplay methods
    public float ClampSensValue(float value)
    {
        return Mathf.Clamp(value, LowerSensLimit, UpperSensLimit);
    }
    public void SetSensMultiplier(float scale)
    {
        Data.SensitivityScale = Mathf.Max(scale, 0.1f);
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
        Data.PlayerFOV = Mathf.Clamp(fov, 1f, 179f);
    }

    public void SetZoomFOV(float zoomFOV)
    {
        Data.ZoomFOV = Mathf.Clamp(zoomFOV, 1f, 179f);
    }
    public float ClampCrosshairSize(float scale)
    {
        return Mathf.Clamp(scale, LowerCrosshairLimit, UpperCrosshairLimit);
    }
    public void SetCrosshairSize(float scale)
    {
        Data.CrosshairSize = Mathf.Clamp(scale, LowerCrosshairLimit, UpperCrosshairLimit);
    }
    public void ToggleInvertX()
    {
        Data.InvertX = !Data.InvertX;
    }

    public void ToggleInvertY()
    {
        Data.InvertY = !Data.InvertY;
    }
    #endregion
}
