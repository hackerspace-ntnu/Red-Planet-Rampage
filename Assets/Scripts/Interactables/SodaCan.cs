using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

enum SodaCanState
{
    Inert,
    Flying,
    Emptied,
}

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

    [SerializeField]
    private AudioGroup sound;

    private AudioSource audioSource;
    private HealthController healthController;
    private Rigidbody body;

    private SodaCanState state = SodaCanState.Inert;

    private Coroutine flyRoutine;
    private Coroutine audioRoutine;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        healthController = GetComponent<HealthController>();
        healthController.onDeath += OnDeath;
        audioSource = GetComponent<AudioSource>();
    }

    private void OnDestroy()
    {
        if (healthController) healthController.onDeath -= OnDeath;
    }

    private void OnDeath(HealthController healthController, float damage, DamageInfo info)
    {
        if (state is SodaCanState.Inert && flyRoutine == null)
            flyRoutine = StartCoroutine(StartFlyingEventually());
        else if (state is SodaCanState.Flying)
            Explode(info);
    }

    private void Explode(DamageInfo info)
    {
        StopFlying();
        StopCoroutine(flyRoutine);
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
        state = SodaCanState.Flying;
        audioRoutine = StartCoroutine(PlayLoop());
        sprayEffect.SendEvent(VisualEffectAsset.PlayEventID);
        sprayEffect.SetBool("IsSpraying", true);
    }

    private IEnumerator PlayLoop()
    {
        while (true)
        {
            sound.Play(audioSource);
            while (audioSource.isPlaying)
                yield return null;
        }
    }

    private void StopFlying()
    {
        state = SodaCanState.Emptied;
        StopCoroutine(audioRoutine);
        audioSource.Stop();
        sprayEffect.SetBool("IsSpraying", false);
    }

    private void FixedUpdate()
    {
        if (state is not SodaCanState.Flying) return;

        var flyingDirection = (-transform.up + flyingDirectionOffset).normalized;
        body.AddForce(flyingDirection * flyingForce, ForceMode.VelocityChange);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (state is not SodaCanState.Flying) return;

        var contact = collision.GetContact(0);
        var flyingDirection = (-transform.up + flyingDirectionOffset).normalized;
        var direction = Vector3.Slerp(Vector3.Reflect(flyingDirection, contact.normal), contact.normal, .5f);
        body.AddForce(direction * flyingForce, ForceMode.Impulse);
    }
}
