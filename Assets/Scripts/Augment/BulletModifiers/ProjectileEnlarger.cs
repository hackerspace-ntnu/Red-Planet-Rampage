using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class ProjectileEnlarger : MonoBehaviour, ProjectileModifier
{
    public float multiplier = 3f;
    
    public void Scale(ref ProjectileState state, GunStats stats)
    {
        float relativeDistance = state.distanceTraveled / state.maxDistance;
        state.size = state.size * multiplier * (1-1/(1+relativeDistance));
        Debug.Log(relativeDistance);
        
    }
    public void Attach(ProjectileController projectile)
    {
        projectile.OnProjectileInit += Scale;
    }

    public void Detach(ProjectileController projectile)
    {
        projectile.OnProjectileInit -= Scale;
    }
}
