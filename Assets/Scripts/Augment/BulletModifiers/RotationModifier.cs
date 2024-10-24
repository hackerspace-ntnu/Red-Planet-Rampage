using RandomExtensions;
using UnityEngine;

/// <summary>
/// Rotates a bullet with specified amount of degrees on it's trajectory
/// </summary>
public class RotationModifier : MonoBehaviour, ProjectileModifier
{
    [SerializeField]
    Vector3 rotationPerUpdate = Vector3.zero;
    [SerializeField]
    private bool randomStartAngleX;
    [SerializeField]
    private bool randomStartAngleY;
    [SerializeField]
    private bool randomStartAngleZ;

    [SerializeField]
    private Priority priority = Priority.ARBITRARY;

    private System.Random random = new System.Random();

    public Priority GetPriority()
    {
        return priority;
    }

    public void Rotate(float distance, ref ProjectileState state)
    {
        if (distance > 0)
        {
            state.rotation *= Quaternion.Euler(rotationPerUpdate * distance);
        }
        else
        {
            state.rotation = Quaternion.Euler(randomStartAngleX ? random.Range(0, 360f) : 0, randomStartAngleY ? random.Range(0, 360f) : 0, randomStartAngleZ ? random.Range(0, 360f) : 0);
        }
    }

    public void Attach(ProjectileController projectile)
    {
        projectile.UpdateProjectileMovement += Rotate;
    }

    public void Detach(ProjectileController projectile)
    {
        projectile.UpdateProjectileMovement -= Rotate;
    }
}
