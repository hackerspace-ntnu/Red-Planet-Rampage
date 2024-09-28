using UnityEngine;

public class ContinuousDamage : MonoBehaviour
{
    [SerializeField]
    private float damage;

    [SerializeField]
    private float damageRate;

    [SerializeField]
    private float durationSeconds = -1;

    private float lastDamageTime = -100;

    [HideInInspector]
    public PlayerManager source;

    private HitboxController hitbox;

    private void Start()
    {
        hitbox = transform.parent.GetComponent<HitboxController>();
        if (hitbox && hitbox.health)
            hitbox.health.onDeath += OnDeath;
        if (durationSeconds > 0) 
        {
            Destroy(gameObject, durationSeconds);
        }
    }

    private void OnDeath(HealthController healthController, float damage, DamageInfo info)
    {
        enabled = false;
    }

    private void Update()
    {
        if (hitbox && Time.time - lastDamageTime > 1 / damageRate)
        {
            hitbox.DamageCollider(new DamageInfo(source, damage, transform.position, -hitbox.transform.forward, DamageType.Continuous, 1));
            lastDamageTime = Time.time;
        }
    }

    private void OnDestroy()
    {
        if (hitbox && hitbox.health)
            hitbox.health.onDeath -= OnDeath;
    }
}
