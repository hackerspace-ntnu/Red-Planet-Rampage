using UnityEngine;

public struct DamageInfo
{
    public PlayerManager sourcePlayer;

    public float damage;

    public Vector3 position;

    public Vector3 force;
    public DamageInfo(PlayerManager source, float damage, Vector3 position, Vector3 force)
    {
        this.sourcePlayer = source;
        this.damage = damage;

        // Todo, re-implement this with actual damage position and force
        this.position = position;
        this.force = force;
    }
}

public class ProjectileDamageController : MonoBehaviour, ProjectileModifier
{
    public PlayerManager player;

    public void Attach(ProjectileController projectile)
    {
        player = projectile.player;
        projectile.OnHitboxCollision += DamageHitbox;
    }

    public void Detach(ProjectileController projectile)
    {
        projectile.OnHitboxCollision -= DamageHitbox;
    }

    private void DamageHitbox(HitboxController controller, ref ProjectileState state)
    {
        var info = new DamageInfo(player, state.damage, state.position, state.direction);
        if (controller.health && !state.hitHealthControllers.Contains(controller.health))
        {
            state.hitHealthControllers.Add(controller.health);
            controller.DamageCollider(info);
        }
    }
}
