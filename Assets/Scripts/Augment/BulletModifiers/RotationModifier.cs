using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rotates a bullet with specified amount of degrees on it's trajectory
/// </summary>
public class RotationModifier : ProjectileModifier
{
    [SerializeField]
    Vector3 rotationPerUpdate = Vector3.zero;
    [SerializeField]
    private bool randomStartAngleX;
    [SerializeField]
    private bool randomStartAngleY;
    [SerializeField]
    private bool randomStartAngleZ;

    public void Rotate(float distance, ref ProjectileState state, GunStats stats)
    {
        if (distance > 0) {
            state.rotation *= Quaternion.Euler(rotationPerUpdate * distance);
        }
        else
        {
            state.rotation = Quaternion.Euler(randomStartAngleX ? Random.Range(0,360) : 0, randomStartAngleY ? Random.Range(0, 360) : 0, randomStartAngleZ ? Random.Range(0, 360) : 0);
        }
        
    }

    void Awake()
    {
        projectile.UpdateProjectileMovement += Rotate;
    }

}
