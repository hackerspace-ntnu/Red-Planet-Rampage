using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using CollectionExtensions;

public enum MusicType
{
    Menu,
    ConstructionFanfare,
    Battle,
    Bidding,
}

public enum FadeMode
{
    FadeIn,
    FadeOut,
    None,
}

public class MusicTrackManager : MonoBehaviour
{
    public static MusicTrackManager Singleton { get; private set; }

    [SerializeField] private AudioMixer mixer;

    [SerializeField] private AudioSource[] layers;

    [SerializeField] private AudioSource[] backupLayers;

    [SerializeField] private double fadeDuration = 0.5f;
    public double TrackOffset => fadeDuration;

    [SerializeField] private double fadeInDuration = 3;

    private MusicTrack track;
    public float BeatsPerMinute => track ? track.BeatsPerMinute : 100;
    public float BeatsPerBar => track ? track.BeatsPerBar : 4;

    private float exponentialReductionFactor = 20;

    [SerializeField] private MusicTrack menuTheme;

    [SerializeField] private MusicTrack biddingTheme;

    [SerializeField] private MusicTrack[] battleThemes;

    [SerializeField] private MusicTrack constructionFanfare;

    private double trackStartTime;
    public double TimeSinceTrackStart => AudioSettings.dspTime - trackStartTime;

    private bool isFadingOutPreviousTrack = false;
    public bool IsfadingOutPreviousTrack => isFadingOutPreviousTrack;

    private bool isPlaying = false;
    public bool IsPlaying => isPlaying;

    private bool shouldSwap = false;
    private double nextSwapTime;

    private Coroutine trackSwitchingRoutine;

    private readonly float LOGARITHMIC_MUTE = -90f;

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

        // Set temporary state while intro scene is playing
        track = menuTheme;
        trackStartTime = AudioSettings.dspTime;
        isPlaying = false;

        DontDestroyOnLoad(gameObject);
    }

    private MusicTrack GetTrack(MusicType type)
    {
        switch (type)
        {
            case MusicType.Battle:
                return battleThemes.RandomElement();
            case MusicType.ConstructionFanfare:
                return constructionFanfare;
            case MusicType.Bidding:
                return biddingTheme;
            case MusicType.Menu:
            default:
                return menuTheme;
        }
    }

    private void SwapLayers()
    {
        // Swap references
        var temp = layers;
        layers = backupLayers;
        backupLayers = temp;

        // Swap which layers are audible
        for (int i = 0; i < layers.Count(); i++)
        {
            if (i < track.Layers.Count())
            {
                // Enable layers that are part of the track
                layers[i].volume = track.EnabledLayers[i] ? 1 : 0;
            }

            // Disable and reset all backup layers
            backupLayers[i].volume = 0;
            backupLayers[i].clip = null;
        }
    }

    // Should only be used in Start()
    private void AssignPlayedLayers(MusicTrack track, bool loopStart = false)
    {
        AudioClip[] trackLayers = loopStart ? track.LoopLayers : track.Layers;
        for (int i = 0; i < layers.Count(); i++)
        {
            if (i < trackLayers.Count())
            {
                layers[i].clip = trackLayers[i];
                if (!loopStart) layers[i].volume = track.EnabledLayers[i] ? 1 : 0;
            }
            else
            {
                layers[i].clip = null;
                layers[i].volume = 0;
            }

            layers[i].Play();
        }
    }

    // Switches tracks at the requested time, see Update()
    private void ScheduleLayers(MusicTrack track, double time, bool loopStart = false)
    {
        shouldSwap = true;
        nextSwapTime = time;
        AudioClip[] trackLayers = loopStart ? track.LoopLayers : track.Layers;
        for (int i = 0; i < layers.Count(); i++)
        {
            backupLayers[i].volume = 0;
            if (i < trackLayers.Count())
            {
                // TODO set looping or nah
                backupLayers[i].clip = trackLayers[i];
                // TODO set looping or nah
                backupLayers[i].PlayScheduled(time);
            }
            else
            {
                backupLayers[i].clip = null;
            }
        }
    }

    public void MuteLayer(int index)
    {
#if DEBUG
        Debug.Assert(index < layers.Length, $"Failed accessing music layer {index}");
#endif
        layers[index].volume = 0;
        mixer.SetFloat(layers[index].outputAudioMixerGroup.name, 0);
    }

    public void FadeInLayer(int index)
    {
#if DEBUG
        Debug.Assert(index < layers.Length, $"Failed accessing music layer {index}");
#endif
        if (layers[index].volume > 0) return;
        layers[index].volume = 1;
        StartCoroutine(FadeIn(layers[index].outputAudioMixerGroup.name));
    }

    public void SwitchTo(MusicType type, FadeMode fadeMode = FadeMode.FadeOut)
    {
        isPlaying = true;
        track = GetTrack(type);
        double offset = 0;
        var hasLoopLayers = track.LoopLayers.Length > 0;

        switch (fadeMode)
        {
            case FadeMode.None:
                AssignPlayedLayers(track);
                break;
            case FadeMode.FadeIn:
                StartCoroutine(FadeInTo(track));
                break;
            case FadeMode.FadeOut:
            default:
                offset = fadeDuration;
                StartCoroutine(FadeOutThenSwitchTo(track));
                break;
        }

        if (trackSwitchingRoutine != null) StopCoroutine(trackSwitchingRoutine);
        if (hasLoopLayers)
        {
            trackSwitchingRoutine = StartCoroutine(WaitThenSwitchToLoop(track, offset + track.Layers.First().length));
        }
    }

    public void IntensifyBattleTheme()
    {
        FadeInLayer(1);
    }

    private IEnumerator Fade(float startVolume, float endVolume, double duration, string channel = "musicVolume")
    {
        double startTime = AudioSettings.dspTime;
        float logarithmicStartVolume = Mathf.Max(0.0001f, Mathf.Pow(10, startVolume / exponentialReductionFactor));
        float logarithmicEndVolume = Mathf.Max(0.0001f, Mathf.Pow(10, endVolume / exponentialReductionFactor));

        while (AudioSettings.dspTime - startTime < duration)
        {
            // Fade volume logarithmically (yeah I used a tutorial here, but I do see what's going on, yo)
            float newVolume = Mathf.Lerp(logarithmicStartVolume, logarithmicEndVolume,
                (float)((AudioSettings.dspTime - startTime) / duration));
            mixer.SetFloat(channel, Mathf.Log10(newVolume) * exponentialReductionFactor);
            yield return null;
        }
    }

    private IEnumerator FadeIn(string channel = "musicVolume")
    {
        // Get intended end point from current volume setting
        mixer.GetFloat("musicVolume", out float endVolume);
        yield return Fade(LOGARITHMIC_MUTE, endVolume, fadeInDuration, channel);
    }

    private IEnumerator FadeOut(string channel = "musicVolume")
    {
        // Remember the volume we started with so we can reset it!
        mixer.GetFloat(channel, out float startVolume);
        yield return Fade(startVolume, LOGARITHMIC_MUTE, fadeDuration, channel);
    }

    private IEnumerator FadeOutThenSwitchTo(MusicTrack track)
    {
        mixer.GetFloat("musicVolume", out float startVolume);
        isFadingOutPreviousTrack = true;
        ScheduleLayers(track, AudioSettings.dspTime + fadeDuration);
        yield return FadeOut();
        mixer.SetFloat("musicVolume", startVolume);
        isFadingOutPreviousTrack = false;
    }

    private IEnumerator FadeInTo(MusicTrack track)
    {
        AssignPlayedLayers(track);
        yield return FadeIn();
        isFadingOutPreviousTrack = false;
    }

    private IEnumerator WaitThenSwitchToLoop(MusicTrack track, double waitTime)
    {
        var time = AudioSettings.dspTime + waitTime;
        yield return new WaitForSecondsRealtime((float)waitTime - 2);
        ScheduleLayers(track, time, true);
    }

    private void Update()
    {
        if (shouldSwap && AudioSettings.dspTime >= nextSwapTime)
        {
            SwapLayers();
            trackStartTime = nextSwapTime;
            shouldSwap = false;
        }
    }
}
