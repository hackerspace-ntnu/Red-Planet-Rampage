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

    [SerializeField]
    private ExplosionController explosion;

    [SerializeField]
    private Renderer mesh;

    private HealthController healthController;
    private Rigidbody body;

    private bool isFlying = false;
    private bool isEmptied = false;

    private IEnumerator flyRoutine;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        healthController = GetComponent<HealthController>();
        healthController.onDeath += OnDeath;
    }

    private void OnDestroy()
    {
        if (healthController) healthController.onDeath -= OnDeath;
    }

    private void OnDeath(HealthController healthController, float damage, DamageInfo info)
    {
        if (isFlying)
            Explode(info);
        else if (!isEmptied)
            StartCoroutine(StartFlyingEventually());
    }

    private void Explode(DamageInfo info)
    {
        StopFlying();
        mesh.enabled = false;
        body.isKinematic = true;
        explosion.Explode(info.sourcePlayer);
    }

    private IEnumerator StartFlyingEventually()
    {
        yield return new WaitForSeconds(timeBeforeFlying);
        StartFlying();
        yield return new WaitForSeconds(timeSpentFlying);
        StopFlying();
    }

    private void StartFlying()
    {
        isFlying = true;
        sprayEffect.SendEvent(VisualEffectAsset.PlayEventID);
        sprayEffect.SetBool("IsSpraying", true);
    }

    private void StopFlying()
    {
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
