using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ExplosionController : MonoBehaviour
{
    // Might need modification or futher testing when added to a rocket/granade
    [SerializeField] private float damage;

    [SerializeField] private AnimationCurve damageCurve;

    private VisualEffect visualEffect;

    [SerializeField] private float radius;

    [SerializeField] private LayerMask hitBoxLayers;

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

    // Function that runs turns on the visual effect and calculates damage
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
        if (!controller.health || !hitHealthControllers.Contains(controller.health))
        {
            hitHealthControllers.Add(controller.health);
            float scaledDamage = damage * damageCurve.Evaluate(Vector3.Distance(collider.transform.position, transform.position) / radius);
            controller.DamageCollider(new DamageInfo(sourcePlayer, scaledDamage));
        }
    }
}
