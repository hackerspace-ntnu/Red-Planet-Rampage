using System;
using UnityEngine;

public enum MusicLayerType
{
    Backing,
    Brass,
    Intense,
}

[Serializable]
public struct MusicLayer
{
    public MusicLayerType type;
    public AudioClip audio;
    public AudioClip loopAudio;

    public float StartingVolume() =>
        type is not MusicLayerType.Intense ? 1 : 0;
}

[CreateAssetMenu(fileName = "MusicTrack", menuName = "Audio/New Music Track")]
public class MusicTrack : ScriptableObject
{
    [SerializeField]
    private string title;
    public string Title => title;

    [SerializeField]
    private string composer;
    public string Composer => composer;

    [SerializeField]
    private float beatsPerMinute;
    public float BeatsPerMinute => beatsPerMinute;

    [SerializeField]
    private float beatsPerBar;
    public float BeatsPerBar => beatsPerBar;

    [SerializeField]
    private bool shouldLoop = true;
    public bool ShouldLoop => shouldLoop;

    [SerializeField]
    private MusicLayer[] layers;
    public MusicLayer[] Layers => layers;
}
