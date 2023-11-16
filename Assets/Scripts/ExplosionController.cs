using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ExplosionController : MonoBehaviour
{
    // Might need modification or futher testing when added to a rocket/granade
    [SerializeField] private float damage;

    [SerializeField] private AnimationCurve damageCurve;

    [SerializeField] private float radius;

    [SerializeField] private float knockbackForce = 2000;

    [SerializeField] private float knockbackLiftFactor = .5f;

    [SerializeField] private LayerMask hitBoxLayers;

    private VisualEffect visualEffect;

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
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    public void Explode(PlayerManager sourcePlayer)
    {
        visualEffect.enabled = true;
        visualEffect.SendEvent("OnPlay");
        Collider[] colliderList = Physics.OverlapSphere(transform.position, radius, hitBoxLayers);
        foreach (Collider collider in colliderList)
        {
            DealDamage(collider, sourcePlayer);
        }
        Destroy(gameObject, 4);
    }

    private void DealDamage(Collider collider, PlayerManager sourcePlayer)
    {
        HitboxController controller = collider.GetComponent<HitboxController>();
        bool hasHealth = controller.health;
        if (hasHealth && !hitHealthControllers.Contains(controller.health))
        {
            hitHealthControllers.Add(controller.health);
            float scaledDamage = damage * damageCurve.Evaluate(Vector3.Distance(collider.transform.position, transform.position) / radius);
            controller.DamageCollider(new DamageInfo(sourcePlayer, scaledDamage));
        }

        if (hasHealth && controller.health.TryGetComponent<Rigidbody>(out var rigidbody))
        {
            rigidbody.AddExplosionForce(knockbackForce, transform.position, radius * 1.2f, knockbackLiftFactor);
        }
    }
}
