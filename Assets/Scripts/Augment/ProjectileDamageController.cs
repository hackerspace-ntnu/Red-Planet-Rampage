using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO use this
public struct DamageInfo
{
    public PlayerManager sourcePlayer;

    public float damage;

    public Vector3 position;

    public Vector3 force;
    public DamageInfo(PlayerManager source, float damage)
    {
        this.sourcePlayer = source;
        this.damage = damage;

        // Todo, re-implement this with actual damage position and force
        this.position = Vector3.zero;
        this.force  = Vector3.zero;
    }
}

public class ProjectileDamageController : MonoBehaviour, ProjectileModifier
{
    public PlayerManager player;
    public void Attach(ProjectileController projectile)
    {
        projectile.OnHitboxCollision += DamageHitbox;
    }
    public void Detach(ProjectileController projectile)
    {
        projectile.OnHitboxCollision -= DamageHitbox;
    }
 
    private void DamageHitbox(HitboxController controller, ref ProjectileState state)
    {
        DamageInfo info = new DamageInfo(player, state.damage);
        if (controller.health == null || !state.hitHealthControllers.Contains(controller.health))
        {
            state.hitHealthControllers.Add(controller.health);
            controller.DamageCollider(info);
        }
    }
}
