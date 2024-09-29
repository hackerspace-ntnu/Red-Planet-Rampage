using UnityEngine;

public class HitboxController : MonoBehaviour
{
    public bool isCritical = false;

    public HealthController health;

    public virtual void DamageCollider(DamageInfo info)
    {
        if (health == null || !health.enabled)
            return;

        if (isCritical && info.criticalHitMultiplier > 1.01f)
        {
            info.isCritical = true;
            info.damage *= info.criticalHitMultiplier;
        }

        health.DealDamage(info);

        if (info.sourcePlayer != null)
            if (info.sourcePlayer.HUDController != null)
                info.sourcePlayer.HUDController.DamageAnimation(info);
    }
}
