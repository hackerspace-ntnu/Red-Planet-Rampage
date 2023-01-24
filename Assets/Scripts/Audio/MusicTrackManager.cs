using System.Collections;
using System.Collections.Generic;
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

    [SerializeField]
    private MusicTrack battleThemeLoop;

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

    private void AssignPlayedLayers(MusicTrack track)
    {
        for (int i = 0; i < layers.Count(); i++)
        {
            layers[i].Stop();
            if (i < track.Layers.Count())
            {
                layers[i].clip = track.Layers[i];
                layers[i].volume = 1;
            }
            else
            {
                layers[i].clip = null;
                layers[i].volume = 0;
            }
            layers[i].Play();
        }
    }

    public void SwitchTo(MusicType type)
    {
        MusicTrack track = GetTrack(type);
        StartCoroutine(FadeOutThenSwitch(track));
        if (type == MusicType.BATTLE)
        {
            // After fade + intro part of battle theme, we should loop the same intense part of the track
            StartCoroutine(SwitchAfterSeconds(battleThemeLoop, fadeDuration + track.Layers.First().length));
        }
    }

    private IEnumerator FadeOutThenSwitch(MusicTrack track)
    {
        // Remember the volume we started with so we can reset it!
        float startTime = Time.time;
        mixer.GetFloat("musicVolume", out float startVolume);
        float logarithmicStartVolume = Mathf.Pow(10, startVolume / exponentialReductionFactor);

        while (Time.time - startTime < fadeDuration)
        {
            // Fade volume logarithmically (yeah I used a tutorial here, but I do see what's going on, yo)
            float newVolume = Mathf.Lerp(logarithmicStartVolume, 0.0001f, (Time.time - startTime) / fadeDuration);
            mixer.SetFloat("musicVolume", Mathf.Log10(newVolume) * exponentialReductionFactor);
            yield return null;
        }

        // Reset volume and switch track
        AssignPlayedLayers(track);
        mixer.SetFloat("musicVolume", startVolume);
    }

    private IEnumerator SwitchAfterSeconds(MusicTrack track, float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);

        // Switch to (looping) track
        AssignPlayedLayers(track);
    }
}
