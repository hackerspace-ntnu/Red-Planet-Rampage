using UnityEngine;

public class VoicePlayer : MonoBehaviour
{
    public Voice Voice;

    [SerializeField]
    private float delayBetweenShortLines = 2f;

    [SerializeField]
    private float delayBetweenLongLines = 5f;

    [SerializeField]
    private AudioSource audioSource;

    private HealthController health;
    private PlayerManager playerManager;

    private float lastFireLineTime = -100;
    private float lastKillLineTime = -100;
    private float lastPanShotTime = -100;

    private bool isDead = false;

    private void Start()
    {
        health = GetComponent<HealthController>();
        health.onDamageTaken += OnDamageTaken;
        health.onDeath += OnDeath;
        playerManager = GetComponent<PlayerManager>();
        playerManager.onKill += OnKill;
    }

    private void OnDestroy()
    {
        health.onDamageTaken -= OnDamageTaken;
        health.onDeath -= OnDeath;
        playerManager.onKill -= OnKill;
    }

    public void Turn2D()
    {
        Voice = Voice.To2D();
    }

    private void OnDamageTaken(HealthController healthController, float damage, DamageInfo info)
    {
        if (info.damageType is DamageType.Continuous && Mathf.Abs(Time.time - lastFireLineTime) > delayBetweenShortLines)
        {
            Voice.FireLines.Play(audioSource);
            lastFireLineTime = Time.time;
        }
    }

    private void OnDeath(HealthController healthController, float damage, DamageInfo info)
    {
        if (isDead)
            return;

        isDead = true;

        if (info.damageType is DamageType.Falling)
        {
            Voice.FallLines.PlayExclusively(audioSource);
        }
        else
        {
            Voice.DeathLines.PlayExclusively(audioSource);
        }
    }

    private void OnKill(PlayerManager victim, PlayerManager killer, DamageInfo info)
    {
        if (Mathf.Abs(Time.time - lastKillLineTime) < delayBetweenLongLines)
            return;
        Voice.KillLines.Play(audioSource);
        lastKillLineTime = Time.time;
    }

    public void PlayPanShot()
    {
        if (Mathf.Abs(Time.time - lastPanShotTime) < delayBetweenShortLines)
            return;
        Voice.PanshotLines.Play(audioSource);
        lastPanShotTime = Time.time;
    }
}
