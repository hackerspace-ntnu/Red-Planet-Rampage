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

    private HealthController healthController;

    private void Start()
    {
        healthController = GetComponent<HealthController>();
        healthController.onDamageTaken += OnHit;
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
