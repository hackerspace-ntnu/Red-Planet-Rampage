using Unity.VisualScripting;
using UnityEngine;

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
    private bool[] enabledLayers;
    public bool[] EnabledLayers => enabledLayers;

    [SerializeField]
    private AudioClip[] layers;
    public AudioClip[] Layers => layers;

    [SerializeField]
    private AudioClip[] loopLayers;
    public AudioClip[] LoopLayers => loopLayers;
}
