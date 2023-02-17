using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rotates a bullet with specified amount of degrees on it's trajectory
/// </summary>
public class RotatorModifier : ProjectileModifier
{
    [SerializeField]
    private float rotationX = 0f;
    [SerializeField]
    private float rotationY = 0f;
    [SerializeField]
    private float rotationZ = 0f;

    private void Rotate(float distance, ref ProjectileState state, GunStats stats)
    {
        state.rotation *= Quaternion.Euler(rotationX*distance, rotationY*distance, rotationZ*distance);
    }

    void Start()
    {
        // TODO: Implement local rotation in BulletController, which is currently not implemented apparently
        projectile.UpdateProjectileMovement += Rotate;
    }

}
