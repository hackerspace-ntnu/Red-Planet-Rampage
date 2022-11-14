using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxController : MonoBehaviour
{
    public bool isCritical = false;

    public HealthController health;

    public void DamageCollider(GunStats stats, int damage)
    {
        health?.dealDamage((int)(damage * (isCritical ? 1 : stats.criticalMultiplier.Value()))); 
    }
}
