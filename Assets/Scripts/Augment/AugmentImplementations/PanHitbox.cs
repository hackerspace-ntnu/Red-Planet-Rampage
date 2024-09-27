using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanHitbox : HitboxController
{
    [SerializeField]
    private HealthController panHealth;

    public override void DamageCollider(DamageInfo info)
    {
        panHealth?.DealDamage(new DamageInfo(info.sourcePlayer, 0f, info.position, info.force, info.damageType));
        if (info.damageType != DamageType.Explosion)
            return;
        if (!health || !health.enabled)
            return;
        // Only deal 1 damage from explosions if pan is closer to explosion than its owner
        health?.DealDamage(new DamageInfo(info.sourcePlayer, 1f, info.position, info.force, info.damageType));
    }
}
