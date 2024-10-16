﻿using System.Linq;
using CollectionExtensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "AudioGroup", menuName = "Audio/New Audio Group")]
public class AudioGroup : ScriptableObject
{
    private static readonly float SEMITONE_PITCH_CONVERSION_UNIT = 1.05946f;

    [SerializeField]
    private AudioClip[] sounds;

#if UNITY_EDITOR
    [MinMax(-10, 10), ContextMenuItem("Preview/Low Pitch", "PreviewLowPitch"), ContextMenuItem("Preview/High Pitch", "PreviewHighPitch")]
#endif
    [SerializeField]
    private IntRange semitoneRange;

    [SerializeField]
    private bool continuousPitchBend;

#if UNITY_EDITOR
    [MinMax(0, 1), ContextMenuItem("Preview/Low Volume", "PreviewLowVolume"), ContextMenuItem("Preview/High Volume", "PreviewHighVolume")]
#endif
    [SerializeField]
    private FloatRange volumeRange;

    [SerializeField]
    private bool is3D = true;

    [SerializeField]
    private bool shouldPlayInSplitscreen = true;

    public AudioGroup To2D() =>
        new()
        {
            is3D = false,
            sounds = sounds,
            semitoneRange = semitoneRange,
            volumeRange = volumeRange,
            continuousPitchBend = continuousPitchBend,
            shouldPlayInSplitscreen = shouldPlayInSplitscreen,
        };

    private void Modulate(AudioSource source)
    {
        // Range has override for ints, so we need to force the endpoints to be floats in order to achieve a continuous scale.
        var pitch = continuousPitchBend ? Random.Range((float)semitoneRange.Min, (float)semitoneRange.Max) : Random.Range(semitoneRange.Min, semitoneRange.Max);
        source.pitch = Mathf.Pow(SEMITONE_PITCH_CONVERSION_UNIT, pitch);
        source.volume = Random.Range(volumeRange.Min, volumeRange.Max);

        // Use 3D sound if there's only one local player,
        // otherwise we're in splitscreen and should not spatialize sounds.
        var isOnlyOneLocalPlayer = PlayerInputManagerController.Singleton.LocalPlayerInputs.Count == 1;
        // TODO replace this check?
        var isInArena = !Scenes.NotArenaScenes.Contains(SceneManager.GetActiveScene().name);
        source.spatialBlend = is3D && isOnlyOneLocalPlayer && isInArena ? 1 : 0;
        // Disable doppler effect as it sounds *very* weird
        source.dopplerLevel = 0;
    }

    public void Play(AudioSource source)
    {
        Modulate(source);
        if (!shouldPlayInSplitscreen && source.spatialBlend == 0)
            return;
        source.PlayOneShot(sounds.RandomElement());
    }

    public void PlayExclusively(AudioSource source)
    {
        Modulate(source);
        if (!shouldPlayInSplitscreen && source.spatialBlend == 0)
            return;
        source.clip = sounds.RandomElement();
        source.Play();
    }

    public void PlayDelayed(AudioSource source, float delay)
    {
        Modulate(source);
        if (!shouldPlayInSplitscreen && source.spatialBlend == 0)
            return;
        source.clip = sounds.RandomElement();
        source.PlayDelayed(delay);
    }

    #region PREVIEW_IN_EDITOR_FUNCTIONALITY
#if UNITY_EDITOR
    private AudioSource previewSource;

    private void OnEnable()
    {
        previewSource = UnityEditor.EditorUtility.CreateGameObjectWithHideFlags("Audio Preview", HideFlags.HideAndDontSave).AddComponent<AudioSource>();
    }

    private void OnDisable()
    {
        if (previewSource)
            DestroyImmediate(previewSource.gameObject);
    }

    [ContextMenu("Play Random")]
    private void PlayRandom()
    {
        AudioClip clip = sounds.RandomElement();
        clip.LoadAudioData();
        Modulate(previewSource);
        previewSource.clip = clip;
        previewSource.Play();
    }

    [ContextMenu("Play Min Pitch")]
    private void PreviewLowPitch()
    {
        AudioClip clip = sounds.RandomElement();
        clip.LoadAudioData();
        previewSource.pitch = Mathf.Pow(SEMITONE_PITCH_CONVERSION_UNIT, semitoneRange.Min);
        previewSource.volume = Mathf.Lerp(volumeRange.Min, volumeRange.Max, 0.5f);
        previewSource.clip = clip;
        previewSource.Play();
    }

    [ContextMenu("Play Max Pitch")]
    private void PreviewHighPitch()
    {
        AudioClip clip = sounds.RandomElement();
        clip.LoadAudioData();
        previewSource.pitch = Mathf.Pow(SEMITONE_PITCH_CONVERSION_UNIT, semitoneRange.Max);
        previewSource.volume = Mathf.Lerp(volumeRange.Min, volumeRange.Max, 0.5f);
        previewSource.clip = clip;
        previewSource.Play();
    }
    [ContextMenu("Play Low Volume")]
    private void PreviewLowVolume()
    {
        AudioClip clip = sounds.RandomElement();
        clip.LoadAudioData();
        previewSource.pitch = Mathf.Pow(SEMITONE_PITCH_CONVERSION_UNIT, Mathf.Lerp(semitoneRange.Min, semitoneRange.Max, 0.5f));
        previewSource.volume = volumeRange.Min;
        previewSource.clip = clip;
        previewSource.Play();
    }
    [ContextMenu("Play High Volume")]
    private void PreviewHighVolume()
    {
        AudioClip clip = sounds.RandomElement();
        clip.LoadAudioData();
        previewSource.pitch = Mathf.Pow(SEMITONE_PITCH_CONVERSION_UNIT, Mathf.Lerp(semitoneRange.Min, semitoneRange.Max, 0.5f));
        previewSource.volume = volumeRange.Max;
        previewSource.clip = clip;
        previewSource.Play();
    }
#endif
    #endregion
}
