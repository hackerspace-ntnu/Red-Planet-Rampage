using UnityEngine;

[CreateAssetMenu(fileName = "Voice", menuName = "Audio/Voice")]
public class Voice : ScriptableObject
{
    [SerializeField]
    private string vibe;
    public string Vibe => vibe;

    [SerializeField]
    private string actor;
    public string Actor => actor;

    [SerializeField]
    private AudioGroup killLines;
    public AudioGroup KillLines => killLines;

    [SerializeField]
    private AudioGroup panshotLines;
    public AudioGroup PanshotLines => panshotLines;

    [SerializeField]
    private AudioGroup fireLines;
    public AudioGroup FireLines => fireLines;

    [SerializeField]
    private AudioGroup fallLines;
    public AudioGroup FallLines => fallLines;

    [SerializeField]
    private AudioGroup deathLines;
    public AudioGroup DeathLines => deathLines;

    public Voice To2D() =>
        new()
        {
            actor = actor,
            vibe = vibe,
            killLines = killLines.To2D(),
            panshotLines = panshotLines.To2D(),
            fallLines = fallLines.To2D(),
            deathLines = deathLines.To2D(),
            fireLines = fireLines.To2D(),
        };
}
