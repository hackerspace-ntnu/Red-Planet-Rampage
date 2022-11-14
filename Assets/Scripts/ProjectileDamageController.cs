using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileDamageController : MonoBehaviour
{
    private HashSet<HealthController> hitHealthControllers = new HashSet<HealthController>();
    private void Start()
    {
        GetComponentInParent<ProjectileController>().OnHitboxCollision += DamageHitbox;
    }

    private void DamageHitbox(HitboxController controller, ref ProjectileState state, GunStats stats)
    {
        
        if( controller.health == null || !hitHealthControllers.Contains(controller.health))
        {
            hitHealthControllers.Add(controller.health);
            controller.DamageCollider(stats, (int)stats.projectileDamage.Value());
        }
    }
}
