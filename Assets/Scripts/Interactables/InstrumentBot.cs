using UnityEngine;

[RequireComponent(typeof(HealthController))]
public class InstrumentBot : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private string onHitStateName;

    [SerializeField]
    private float distortionDuration = 2;

    [SerializeField]
    private int framesPerBeat = 10;

    [SerializeField]
    private int framesInAnimation = 40;

    private const int FramesPerSecond = 24;

    private HealthController healthController;

    private void Start()
    {
        healthController = GetComponent<HealthController>();
        healthController.onDamageTaken += OnHit;

        // given fram
        // duration = frames in animation / frames per second
        // seconds per beat in animation = (frames per beat / frames in animation) * duration
        // then that divided by seconds per beat???
        var duration = framesInAnimation / FramesPerSecond;
        var frameRatio = framesPerBeat / (float)framesInAnimation;
        animator.speed = frameRatio * duration / MusicTrackManager.Singleton.SecondsPerBeat;

        // TODO speed is fine n dandy, but what about the delay from the track offset???
    }

    private void OnDestroy()
    {
        healthController.onDamageTaken -= OnHit;
    }

    private void OnHit(HealthController healthController, float damage, DamageInfo info)
    {
        animator.Play(onHitStateName, -1, 0);
        MusicTrackManager.Singleton.Distort(distortionDuration);
    }
}
