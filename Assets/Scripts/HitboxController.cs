using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxController : MonoBehaviour
{
    public bool isCritical = false;

    public HealthController health;

    public void DamageCollider(DamageInfo info)
    {
        if (!health.enabled)
            return;
        health?.DealDamage(info);

        if (info.sourcePlayer != null)
            if (info.sourcePlayer.HUDController != null)
                info.sourcePlayer.HUDController.DamageAnimation();
    }
}
