using UnityEngine;

public class FallingHazard : MonoBehaviour
{
    [SerializeField] private float deadlyVelocityThreshold = 100;
    [SerializeField] private float audibleVelocityThreshold = 50;
    [SerializeField] private AudioGroup soundEffect;
    [SerializeField] private ExplosionController impactExplosion;

    private PlayerManager player;
    public PlayerManager Player
    {
        get => player;
        set => player = value;
    }

    private Rigidbody body;
    private AudioSource audioSource;

    private bool isFirstImpact = true;

    private float lastVelocity = 0f;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        body.AddForce(500f * Vector3.down, ForceMode.Impulse);
        body.AddForce(50f * Vector3.down, ForceMode.VelocityChange);
        audioSource = GetComponent<AudioSource>();
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

        if (!isFirstImpact)
            return;

        // Squared magnitude performs better cuz no square root is required :)
        var isMovingFastEnoughToKill = lastVelocity > deadlyVelocityThreshold;
        if (!isMovingFastEnoughToKill)
            return;

        isFirstImpact = false;

        var instance = Instantiate(impactExplosion, transform.position, Quaternion.identity);
        instance.Init();
        instance.Explode(player);
    }
}