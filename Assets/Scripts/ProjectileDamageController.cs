using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileDamageController : MonoBehaviour
{
    private HashSet<HealthController> hitHealthControllers = new HashSet<HealthController>();
    void Start()
    {
        GetComponent<ProjectileController>().OnColliderHit += DamageHitbox;
    }

    private void DamageHitbox(ProjectileController projectile, Collider collider, GunBaseStats stats)
    {
        var controller = collider.GetComponent<HitboxController>();
        if( controller.health == null || !hitHealthControllers.Contains(controller.health))
        {
            hitHealthControllers.Add(controller.health);
            collider.GetComponent<HitboxController>()?.damageCollider(stats, (int)stats.bulletDamage);
        }
    }
}
