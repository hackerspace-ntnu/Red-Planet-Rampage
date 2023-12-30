using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class SodaCan : MonoBehaviour
{
    [SerializeField]
    private float flyingForce = 1;

    [SerializeField]
    private Vector3 flyingDirectionOffset = new Vector3(.1f, 0f, -.2f);

    [SerializeField]
    private float timeBeforeFlying = 1;

    [SerializeField]
    private float timeSpentFlying = 10;

    [SerializeField]
    private float knockbackForceMultiplier = 10;

    [SerializeField]
    private VisualEffect sprayEffect;

    private HealthController healthController;
    private Rigidbody body;

    private bool isFlying = false;
    private Vector3 flyingDirection;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        healthController = GetComponent<HealthController>();
        healthController.onDeath += OnDeath;
    }

    private void OnDeath(HealthController healthController, float damage, DamageInfo info)
    {
        var force = info.force.normalized * info.damage * knockbackForceMultiplier;
        body.AddForce(force, ForceMode.Impulse);
        StartCoroutine(StartFlyingEventually());
    }

    private IEnumerator StartFlyingEventually()
    {
        yield return new WaitForSeconds(timeBeforeFlying);
        isFlying = true;
        sprayEffect.SendEvent(VisualEffectAsset.PlayEventID);
        yield return new WaitForSeconds(timeSpentFlying);
        isFlying = false;
        sprayEffect.SendEvent(VisualEffectAsset.StopEventID);
    }


    private void FixedUpdate()
    {
        if (!isFlying) return;

        var flyingDirection = (-transform.up + flyingDirectionOffset).normalized;
        body.AddForce(flyingDirection * flyingForce, ForceMode.VelocityChange);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isFlying) return;

        var contact = collision.GetContact(0);

        Debug.Log("ENTERED");

        var flyingDirection = (-transform.up + flyingDirectionOffset).normalized;
        var direction = Vector3.Slerp(Vector3.Reflect(flyingDirection, contact.normal), contact.normal, .5f);
        body.AddForce(direction * flyingForce, ForceMode.Impulse);
    }
}
