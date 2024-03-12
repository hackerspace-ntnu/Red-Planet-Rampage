using CollectionExtensions;
using UnityEngine;

public class PinAnimator : AugmentAnimator
{
    [SerializeField]
    private float maxDist;

    private float time;

    [SerializeField]
    private float delay = 0f;

    [SerializeField]
    private AnimationCurve easeCurve;

    [SerializeField]
    private AudioGroup gunCocking;
    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private bool playAudio;

    public override void OnInitialize(GunStats stats)
    {
        time = 1 / stats.Firerate.Value();
    }

    public override void OnReload(GunStats stats) { }

    public override void OnFire(GunStats stats)
    {
        if (playAudio)
            PlayCockingSound();
        transform.localPosition = Vector3.zero;
        LeanTween.moveLocalZ(gameObject, maxDist, time * (1 - delay))
            .setDelay(delay * time)
            .setEase(easeCurve);
        OnShotFiredAnimation?.Invoke();
        OnAnimationEnd?.Invoke();
    }

    private void PlayCockingSound()
    {
        gunCocking.PlayDelayed(audioSource, delay);
    }
}
