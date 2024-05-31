using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class FallingHazard : MonoBehaviour
{
    [SerializeField] private float audibleVelocityThreshold = 50;
    [SerializeField] private float initialFallVelocity = 10;

    [SerializeField] private float spherecastRadius = 2;
    [SerializeField] private LayerMask spherecastMask;

    [SerializeField] private AudioGroup soundEffect;
    [SerializeField] private ExplosionController impactExplosion;

    private const float gravity = 9.81f; // >:)

    private Rigidbody body;
    private AudioSource audioSource;

    private bool isFalling = false;
    private float lastVelocity;
    private float fallVelocity;

    private Action<Vector3> onHit = (Vector3 p) => { };

    private void Start()
    {
        if (!body) body = GetComponent<Rigidbody>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }

    public void Launch(Vector3 position, Action<Vector3> onHit)
    {
        if (!body) body = GetComponent<Rigidbody>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();

        // Disable physics
        body.isKinematic = true;

        // Move to launch point
        var rotation = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up);
        transform.position = position;
        transform.rotation = rotation;
        body.position = position;
        body.rotation = rotation;

        fallVelocity = initialFallVelocity;
        isFalling = true;
        this.onHit = onHit;
    }

    private void OnGround(Vector3 point)
    {
        soundEffect.Play(audioSource);

        // Reset and enable physics
        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.isKinematic = false;

        // Stick to the target position
        transform.position = point;
        body.position = point;

        // before bouncing off
        body.AddForce(10f * (Random.rotation * Vector3.up), ForceMode.VelocityChange);

        isFalling = false;
        onHit(point);
    }

    private void FixedUpdate()
    {
        if (!isFalling)
            return;


        // Accelerate
        fallVelocity += gravity * Time.fixedDeltaTime;
        var fallDistance = fallVelocity * Time.fixedDeltaTime;

        // Check for collision in current distance
        var colliders = Physics.OverlapCapsule(transform.position + Vector3.down, transform.position + (1 + fallDistance) * Vector3.down, spherecastRadius, spherecastMask)
                               .Where(c => !(c.gameObject.transform.parent && c.gameObject.transform.parent.TryGetComponent<FallingHazard>(out var _)));
        if (colliders.Count() > 0)
        {
            // Touch down at closest point
            var point = colliders.Select(c => c.ClosestPoint(transform.position)).OrderBy(p => (transform.position - p).sqrMagnitude).First();
            OnGround(point);
            return;
        }

        transform.position += fallDistance * Vector3.down;
    }

    private void LateUpdate()
    {
        lastVelocity = Mathf.Lerp(lastVelocity, body.velocity.sqrMagnitude, .2f);
    }

    private void OnCollisionEnter(Collision other)
    {
        var isCollidingWithOtherHazard = other.gameObject.TryGetComponent<FallingHazard>(out var _);
        if (isCollidingWithOtherHazard)
            return;

        var isMovingFastEnoughToMakeNoise = lastVelocity > audibleVelocityThreshold;
        if (!isMovingFastEnoughToMakeNoise)
            return;

        soundEffect.Play(audioSource);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spherecastRadius);
    }
}
