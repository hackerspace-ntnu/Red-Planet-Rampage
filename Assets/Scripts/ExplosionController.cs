using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ExplosionController : MonoBehaviour
{
    // Might need modification or futher testing when added to a rocket/granade
    [SerializeField] private float damage;

    [SerializeField] private AnimationCurve damageCurve;

    [SerializeField] private float radius;
    public float Radius => radius;

    [SerializeField] private float knockbackForce = 2000;

    [SerializeField] private float knockbackLiftFactor = .5f;

    [SerializeField] private LayerMask hitBoxLayers;

    private VisualEffect visualEffect;
    private AudioSource soundEffect;

    // Makes sure a player doesn't take damage for each hitbox
    private HashSet<HealthController> hitHealthControllers = new HashSet<HealthController>();


    private void Start()
    {
        if (!visualEffect) Init();
    }

    public void Init()
    {
        visualEffect = GetComponent<VisualEffect>();
        visualEffect.enabled = false;
        soundEffect = GetComponent<AudioSource>();
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    public void Explode(PlayerManager sourcePlayer)
    {
        visualEffect.enabled = true;
        visualEffect.SendEvent(VisualEffectAsset.PlayEventID);
        soundEffect?.Play();
        var targets = Physics.OverlapSphere(transform.position, radius, hitBoxLayers);
        foreach (var target in targets)
        {
            DealDamage(target, sourcePlayer);
        }
        Destroy(gameObject, 4);
    }

    private void DealDamage(Collider target, PlayerManager sourcePlayer)
    {
        if (!target.TryGetComponent<HitboxController>(out var hitbox))
            return;
        bool hasHealth = hitbox.health;
        if (hasHealth && !hitHealthControllers.Contains(hitbox.health))
        {
            hitHealthControllers.Add(hitbox.health);
            var scaledDamage = damage * damageCurve.Evaluate(Vector3.Distance(target.transform.position, transform.position) / radius);
            hitbox.DamageCollider(new DamageInfo(sourcePlayer, scaledDamage, target.transform.position, (target.transform.position - transform.position).normalized));
        }

        if (hasHealth && hitbox.health.TryGetComponent<Rigidbody>(out var rigidbody))
        {
            rigidbody.AddExplosionForce(knockbackForce, transform.position, radius * 1.2f, knockbackLiftFactor);
        }
    }
}
