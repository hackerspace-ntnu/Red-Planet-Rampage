using CollectionExtensions;
using UnityEngine;

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

#if UNITY_EDITOR
    [MinMax(0, 1), ContextMenuItem("Preview/Low Volume", "PreviewLowVolume"), ContextMenuItem("Preview/High Volume", "PreviewHighVolume")]
#endif
    [SerializeField]
    private FloatRange volumeRange;
    public void Modulate(AudioSource source)
    {
        source.pitch = Mathf.Pow(SEMITONE_PITCH_CONVERSION_UNIT, Random.Range(semitoneRange.Min, semitoneRange.Max));
        source.volume = Random.Range(volumeRange.Min, volumeRange.Max);
    }
    public void PlayRandomFrom(AudioSource source)
    {
        source.pitch = Mathf.Pow(SEMITONE_PITCH_CONVERSION_UNIT, Random.Range(semitoneRange.Min, semitoneRange.Max));
        source.volume = Random.Range(volumeRange.Min, volumeRange.Max);
        source.PlayOneShot(sounds.RandomElement());
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
        DestroyImmediate(previewSource.gameObject);
    }

    [ContextMenu("Play Random")]
    private void PlayRandom()
    {
        AudioClip clip = sounds.RandomElement();
        clip.LoadAudioData();
        Modulate(previewSource);
        previewSource.PlayOneShot(clip);
    }

    private void PreviewLowPitch()
    {
        AudioClip clip = sounds.RandomElement();
        clip.LoadAudioData();
        previewSource.pitch = Mathf.Pow(SEMITONE_PITCH_CONVERSION_UNIT, semitoneRange.Min);
        previewSource.PlayOneShot(clip);
    }

    private void PreviewHighPitch()
    {
        AudioClip clip = sounds.RandomElement();
        clip.LoadAudioData();
        previewSource.pitch = Mathf.Pow(SEMITONE_PITCH_CONVERSION_UNIT, semitoneRange.Max);
        previewSource.PlayOneShot(clip);
    }
    private void PreviewLowVolume()
    {
        AudioClip clip = sounds.RandomElement();
        clip.LoadAudioData();
        previewSource.PlayOneShot(clip, volumeRange.Min);
    }
    private void PreviewHighVolume()
    {
        AudioClip clip = sounds.RandomElement();
        clip.LoadAudioData();
        previewSource.PlayOneShot(clip, volumeRange.Max);
    }
#endif
    #endregion
}
