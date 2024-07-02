using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public class ExplosionController : MonoBehaviour
{
    // Might need modification or futher testing when added to a rocket/granade
    [SerializeField] private float damage;

    [SerializeField] private AnimationCurve damageCurve;

    [SerializeField] private float radius;
    public float Radius
    {
        get => radius;
        set
        {
            radius = value;
        }
    }

    [SerializeField] private float knockbackForce = 2000;

    [SerializeField] private float knockbackLiftFactor = .5f;

    [SerializeField] private LayerMask hitBoxLayers;

    [SerializeField] private AudioGroup soundEffect;

    private List<VisualEffect> visualEffects = new();
    private AudioSource audioSource;

    // Makes sure a player doesn't take damage for each hitbox
    private HashSet<HealthController> hitHealthControllers = new HashSet<HealthController>();


    private void Start()
    {
        if (visualEffects.Count < 1) Init();
    }

    public void Init()
    {
        visualEffects = GetComponentsInChildren<VisualEffect>().ToList();
        visualEffects.ForEach(vfx => {
            vfx.enabled = false;
            vfx.SetFloat("Size", radius / 6f);
        });
        audioSource = GetComponent<AudioSource>();
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    public List<(RaycastHit hit, float damage)> Explode(PlayerManager sourcePlayer)
    {
        visualEffects.ForEach(vfx => 
        {
            vfx.enabled = true;
            vfx.SendEvent(VisualEffectAsset.PlayEventID);
        });
        soundEffect.Play(audioSource);
        var targets = Physics.SphereCastAll(transform.position, radius, Vector3.up, 0.01f, hitBoxLayers);
        var hits = new List<(RaycastHit, float)>(targets.Length);
        foreach (var target in targets)
        {
            DealDamage(target.collider, sourcePlayer, out var shouldBeReturned, out var scaledDamage);
            if (shouldBeReturned)
                hits.Add((target, scaledDamage));
        }
        Destroy(gameObject, 4);
        return hits;
    }

    private void DealDamage(Collider target, PlayerManager sourcePlayer, out bool shouldBeReturned, out float scaledDamage)
    {
        scaledDamage = 0;
        if (!target.TryGetComponent<HitboxController>(out var hitbox))
        {
            shouldBeReturned = true;
            return;
        }

        bool hasHealth = hitbox.health;
        bool hasNotBeenRegisteredYet = !hitHealthControllers.Contains(hitbox.health);
        shouldBeReturned = !hasHealth || hasNotBeenRegisteredYet;

        if (hasHealth && hasNotBeenRegisteredYet)
        {
            hitHealthControllers.Add(hitbox.health);
            scaledDamage = damage * damageCurve.Evaluate(Vector3.Distance(target.transform.position, transform.position) / radius);
            hitbox.DamageCollider(new DamageInfo(sourcePlayer, scaledDamage, target.transform.position, (target.transform.position - transform.position).normalized, DamageType.Explosion));
        }

        if (hasHealth && hitbox.health.TryGetComponent<Rigidbody>(out var rigidbody))
        {
            rigidbody.AddExplosionForce(knockbackForce, transform.position, radius * 1.2f, knockbackLiftFactor);
        }
    }
}
