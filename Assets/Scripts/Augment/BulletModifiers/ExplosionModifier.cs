using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rotates a bullet with specified amount of degrees on it's trajectory
/// </summary>
public class ExplosionModifier : MonoBehaviour, ProjectileModifier
{
    [SerializeField]
    private ExplosionController explosion;

    private PlayerManager player;

    public void Attach(ProjectileController projectile)
    {
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
        instance.Explode(player);
    }
}
