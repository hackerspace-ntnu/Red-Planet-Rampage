using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO use this
public struct DamageInfo
{
    public PlayerManager sourcePlayer;

    public GunStats stats;
    public ProjectileState projectileState;

    public bool isCritical;

    public DamageInfo(PlayerManager source, GunStats stats, ProjectileState projectileState)
    {
        this.sourcePlayer = source;
        this.stats = stats;
        this.projectileState = projectileState;
        this.isCritical = false;
    }
}

public class ProjectileDamageController : MonoBehaviour
{
    public PlayerManager player;

    private HashSet<HealthController> hitHealthControllers = new HashSet<HealthController>();
    private void Awake()
    {
        GetComponentInParent<ProjectileController>().OnHitboxCollision += DamageHitbox;
    }

    private void DamageHitbox(HitboxController controller, ref ProjectileState state, GunStats stats)
    {
        DamageInfo info = new DamageInfo(player, stats, state);
        if (controller.health == null || !hitHealthControllers.Contains(controller.health))
        {
            hitHealthControllers.Add(controller.health);
            controller.DamageCollider(info);
        }
    }
}
