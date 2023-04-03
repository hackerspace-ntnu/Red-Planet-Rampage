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
    private bool[] enabledLayers;
    public bool[] EnabledLayers => enabledLayers;

    [SerializeField]
    private AudioClip[] layers;
    public AudioClip[] Layers => layers;

    [SerializeField]
    private AudioClip[] loopLayers;
    public AudioClip[] LoopLayers => loopLayers;
}
