using UnityEngine;

public class RicochetModifier : MonoBehaviour, ProjectileModifier
{
    public void RicochetProjectile(RaycastHit other, ref ProjectileState state)
    {
        state.direction = Vector3.Reflect(state.direction, other.normal);
    }

    public void Attach(ProjectileController projectile)
    {
        projectile.OnColliderHit += RicochetProjectile;
    }
    public void Detach(ProjectileController projectile)
    {
        projectile.OnColliderHit -= RicochetProjectile;
    }
}
