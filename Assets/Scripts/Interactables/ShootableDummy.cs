using UnityEngine;

public class ShootableDummy : MonoBehaviour
{
    [SerializeField]
    private float knockbackForceMultiplier = 40;

    private HealthController healthController;
    private Rigidbody body;

    private bool isFlying = false;
    private bool isEmptied = false;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        healthController = GetComponent<HealthController>();
        healthController.onDamageTaken += OnHit;
    }

    private void OnHit(HealthController healthController, float damage, DamageInfo info)
    {
        var force = info.force.normalized * info.damage * knockbackForceMultiplier;
        body.AddForce(force, ForceMode.Impulse);
    }
}
