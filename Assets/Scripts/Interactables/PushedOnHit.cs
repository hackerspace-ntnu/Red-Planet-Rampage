using UnityEngine;

public class PushedOnHit : MonoBehaviour
{
    [SerializeField]
    private float knockbackForceMultiplier = 1;

    private HealthController healthController;
    private Rigidbody body;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        healthController = GetComponent<HealthController>();
        healthController.onDamageTaken += OnHit;
    }

    private void Destroy()
    {
        healthController.onDamageTaken -= OnHit;
    }

    private void OnHit(HealthController healthController, float damage, DamageInfo info)
    {
        var force = info.force.normalized * info.damage * knockbackForceMultiplier;
        body.AddForce(force, ForceMode.Impulse);
    }
}
