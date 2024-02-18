using System;
using UnityEngine;

/// <summary>
/// Creates an explosion on hit
/// </summary>
public class ExplosionModifier : MonoBehaviour, ProjectileModifier
{
    [SerializeField]
    private ExplosionController explosion;

    private PlayerManager player;
    private ProjectileController projectile;

    public void Attach(ProjectileController projectile)
    {
        this.projectile = projectile;
        projectile.OnColliderHit += Explode;
        player = projectile.player;
    }

    public void Detach(ProjectileController projectile)
    {
        projectile.OnColliderHit -= Explode;
    }

    private void Explode(Collider other, ref ProjectileState state)
    {
        var instance = Instantiate(explosion, state.position, Quaternion.identity);
        instance.Init();
        var targets = instance.Explode(player);

        // Trigger on hit, excluding this effect and using the scaled damage from the explosion
        var originalDamage = state.damage;
        projectile.OnColliderHit -= Explode;
        foreach (var (target, damage) in targets)
        {
            state.damage = damage;
            projectile.OnColliderHit?.Invoke(target, ref state);
        }
        projectile.OnColliderHit += Explode;
        state.damage = originalDamage;
    }
}

