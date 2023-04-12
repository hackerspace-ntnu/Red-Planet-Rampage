using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public enum MusicType
{
    MENU,
    BATTLE,
    BIDDING,
}

public class MusicTrackManager : MonoBehaviour
{
    public static MusicTrackManager Singleton { get; private set; }

    [SerializeField]
    private AudioMixer mixer;

    [SerializeField]
    private AudioSource[] layers;

    [SerializeField]
    private float fadeDuration = 0.5f;

    private float exponentialReductionFactor = 20;

    [SerializeField]
    private MusicTrack menuTheme;

    [SerializeField]
    private MusicTrack battleTheme;

    private Coroutine trackSwitchingRoutine;

    void Awake()
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

        AssignPlayedLayers(menuTheme);

        DontDestroyOnLoad(gameObject);
    }

    private MusicTrack GetTrack(MusicType type)
    {
        switch (type)
        {
            case MusicType.BATTLE:
                return battleTheme;
            case MusicType.BIDDING:
            case MusicType.MENU:
            default:
                return menuTheme;
        }
    }

    private void AssignPlayedLayers(MusicTrack track, bool loopStart = false)
    {
        AudioClip[] trackLayers = loopStart ? track.LoopLayers : track.Layers;
        for (int i = 0; i < layers.Count(); i++)
        {
            layers[i].Stop();
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

    public void SwitchTo(MusicType type)
    {
        MusicTrack track = GetTrack(type);
        StartCoroutine(FadeOutThenSwitchTo(track));
        if (trackSwitchingRoutine != null) StopCoroutine(trackSwitchingRoutine);
        if (track.LoopLayers.Length > 0)
        {
            trackSwitchingRoutine = StartCoroutine(WaitThenSwitchToLoop(track, fadeDuration + track.Layers.First().length));
        }
    }

    public void IntensifyBattleTheme()
    {
        FadeInLayer(1);
    }

    private IEnumerator Fade(float startVolume, float endVolume, string channel = "musicVolume")
    {
        float startTime = Time.time;
        float logarithmicStartVolume = Mathf.Max(0.0001f, Mathf.Pow(10, startVolume / exponentialReductionFactor));
        float logarithmicEndVolume = Mathf.Max(0.0001f, Mathf.Pow(10, endVolume / exponentialReductionFactor));

        while (Time.time - startTime < fadeDuration)
        {
            // Fade volume logarithmically (yeah I used a tutorial here, but I do see what's going on, yo)
            float newVolume = Mathf.Lerp(logarithmicStartVolume, logarithmicEndVolume, (Time.time - startTime) / fadeDuration);
            mixer.SetFloat(channel, Mathf.Log10(newVolume) * exponentialReductionFactor);
            yield return null;
        }
    }

    private IEnumerator FadeIn(string channel = "musicVolume")
    {
        // Get intended end point from current volume setting
        mixer.GetFloat("musicVolume", out float endVolume);
        yield return Fade(0, endVolume, channel);
    }

    private IEnumerator FadeOut(string channel = "musicVolume")
    {
        // Remember the volume we started with so we can reset it!
        mixer.GetFloat(channel, out float startVolume);
        yield return Fade(startVolume, 0, channel);
    }

    private IEnumerator FadeOutThenSwitchTo(MusicTrack track)
    {
        mixer.GetFloat("musicVolume", out float startVolume);
        yield return FadeOut();

        // Reset volume and switch track
        AssignPlayedLayers(track);
        mixer.SetFloat("musicVolume", startVolume);
    }

    private IEnumerator WaitThenSwitchToLoop(MusicTrack track, float time)
    {
        yield return new WaitForSecondsRealtime(time);
        AssignPlayedLayers(track, true);
    }
}
