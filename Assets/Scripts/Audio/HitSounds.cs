using UnityEngine;

public class HitSounds : MonoBehaviour
{
    [SerializeField]
    private AudioGroup hitSounds;

    [SerializeField]
    private AudioGroup criticalHitSounds;

    [SerializeField]
    private AudioGroup extraHitSounds;

    [SerializeField]
    private AudioSource audioSource;

    private void Start()
    {
        var health = GetComponent<HealthController>();
        health.onDamageTaken += OnDamageTaken;
        if (!audioSource)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnDestroy()
    {
        var health = GetComponent<HealthController>();
        if (health)
            health.onDamageTaken -= OnDamageTaken;
    }

    private void OnDamageTaken(HealthController healthController, float damage, DamageInfo info)
    {
        if (!audioSource)
            return;

        if (criticalHitSounds && info.isCritical)
        {
            criticalHitSounds.Play(audioSource);
        }
        else
        {
            if (extraHitSounds && Random.Range(0, 10000) <= 5)
            {
                extraHitSounds.Play(audioSource);
            }
            else
            {
                hitSounds.Play(audioSource);
            }
        }
    }

}
