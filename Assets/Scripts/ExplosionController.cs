using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class ExplosionController : MonoBehaviour
{
    // Might need modification or futher testing when added to a rocket/granade
    [SerializeField] private int damage;

    private VisualEffect visualEffect;

    [SerializeField] private float radius;

    [SerializeField] private LayerMask hitBoxLayers;

    // Makes sure a player doesnt take damage for each hitbox
    private HashSet<HealthController> hitHealthControllers = new HashSet<HealthController>();


    private void Start()
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
            DealDamage(collider,sourcePlayer);
        }
        
    }

    private void DealDamage(Collider collider,PlayerManager sourcePlayer)
    {
        HitboxController controller = collider.GetComponent<HitboxController>();
        if (!controller.health || !hitHealthControllers.Contains(controller.health))
        {
            hitHealthControllers.Add(controller.health);
            float distanceScaling = math.pow((Vector3.Distance(collider.transform.position, transform.position) / radius), 2);
            float damageFinal = (1 - distanceScaling) * damage;
            controller.DamageCollider(new DamageInfo(sourcePlayer,damageFinal));
        }
    }
}
