using UnityEngine;

public class RicochetModifier : MonoBehaviour, ProjectileModifier
{
    public void ricochetProjectile(RaycastHit other, ref ProjectileState state)
    {
        state.direction = Vector3.Reflect(state.direction, other.normal);
    }

    public void Attach(ProjectileController projectile)
    {
        projectile.OnColliderHit += ricochetProjectile;
    }
    public void Detach(ProjectileController projectile)
    {
        projectile.OnColliderHit -= ricochetProjectile;
    }
}
