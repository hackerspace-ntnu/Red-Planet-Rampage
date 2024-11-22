using System.Collections;
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

    private void OnEnable()
    {
        if (!transform.parent)
            return;
        Initialize();
    }

    public void Initialize()
    {
        hitbox = transform.parent.GetComponent<HitboxController>();
        if (hitbox && hitbox.health)
            hitbox.health.onDeath += OnDeath;
        if (durationSeconds > 0)
            StartCoroutine(DelayedDisable());
    }

    private IEnumerator DelayedDisable()
    {
        yield return new WaitForSeconds(durationSeconds);
        gameObject.SetActive(false);
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

    private void OnDisable()
    {
        if (hitbox && hitbox.health)
            hitbox.health.onDeath -= OnDeath;
    }
}
