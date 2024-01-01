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
    private VisualEffect sprayEffect;

    private HealthController healthController;
    private Rigidbody body;

    private bool isFlying = false;
    private bool isEmptied = false;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        healthController = GetComponent<HealthController>();
        healthController.onDeath += OnDeath;
    }

    private void OnDestroy()
    {
        healthController.onDeath -= OnDeath;
    }

    private void OnDeath(HealthController healthController, float damage, DamageInfo info)
    {
        if (!isEmptied)
            StartCoroutine(StartFlyingEventually());
    }

    private IEnumerator StartFlyingEventually()
    {
        yield return new WaitForSeconds(timeBeforeFlying);
        isFlying = true;
        sprayEffect.SendEvent(VisualEffectAsset.PlayEventID);
        sprayEffect.SetBool("IsSpraying", true);
        yield return new WaitForSeconds(timeSpentFlying);
        isFlying = false;
        isEmptied = true;
        sprayEffect.SetBool("IsSpraying", false);
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
        var flyingDirection = (-transform.up + flyingDirectionOffset).normalized;
        var direction = Vector3.Slerp(Vector3.Reflect(flyingDirection, contact.normal), contact.normal, .5f);
        body.AddForce(direction * flyingForce, ForceMode.Impulse);
    }
}
