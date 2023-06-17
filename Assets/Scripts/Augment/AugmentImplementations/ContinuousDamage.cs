using UnityEngine;

public class ContinuousDamage : MonoBehaviour
{
    [SerializeField]
    private float damage;

    [SerializeField]
    private float damageRate;

    private float lastDamageTime = -100;

    [HideInInspector]
    public PlayerManager source;

    private HitboxController hitbox;

    void Start()
    {
        hitbox = transform.parent.GetComponent<HitboxController>();
        if (hitbox && hitbox.health)
        {
            hitbox.health.onDeath += OnDeath;
        }
    }

    private void OnDeath(HealthController healthController, float damage, DamageInfo info)
    {
        this.enabled = false;
    }

    private void Update()
    {
        if (hitbox && Time.time - lastDamageTime > 1 / damageRate)
        {
            hitbox.DamageCollider(new DamageInfo(source, damage));
            lastDamageTime = Time.time;
        }
    }
}
