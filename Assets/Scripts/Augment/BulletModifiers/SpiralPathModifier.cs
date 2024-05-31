using Mirror;
using RandomExtensions;
using UnityEngine;

public class SpiralPathModifier : NetworkBehaviour, ProjectileModifier
{

    //Modifier for adding a spiral movement to any projectile
    public AnimationCurve radialLerp;

    public float spiralRadius, spiralWavelength, spiralLerpDist;

    [SerializeField]
    private bool randomAngle = false;

    // TODO synchronize
    private System.Random random = new System.Random();

    public void AddSpiralDisplacement(float distance, ref ProjectileState state)
    {
        float oldRadius = radialLerp.Evaluate(state.distanceTraveled / spiralLerpDist) * spiralRadius;
        float newRadius = radialLerp.Evaluate((state.distanceTraveled + distance) / spiralLerpDist) * spiralRadius;

        Vector3 oldVector = new Vector3(
            Mathf.Sin((float)state.additionalProperties["spiralOffset"] + 2 * Mathf.PI * state.distanceTraveled / spiralWavelength),
            Mathf.Cos((float)state.additionalProperties["spiralOffset"] + 2 * Mathf.PI * state.distanceTraveled / spiralWavelength)) * oldRadius;
        Vector3 newVector = new Vector3(
            Mathf.Sin((float)state.additionalProperties["spiralOffset"] + 2 * Mathf.PI * (state.distanceTraveled + distance) / spiralWavelength),
            Mathf.Cos((float)state.additionalProperties["spiralOffset"] + 2 * Mathf.PI * (state.distanceTraveled + distance) / spiralWavelength)) * newRadius;

        state.position += state.rotation * (newVector - oldVector);

    }

    public Priority GetPriority()
    {
        return Priority.EXTENSION;
    }

    public void SetProjectileAngle(ref ProjectileState state, GunStats stats)
    {
        state.additionalProperties["spiralOffset"] = randomAngle ? random.Range(0, 2 * Mathf.PI) : 0f;
    }

    public void Attach(ProjectileController projectile)
    {
        projectile.UpdateProjectileMovement += AddSpiralDisplacement;
        projectile.OnProjectileInit += SetProjectileAngle;
    }

    public void Detach(ProjectileController projectile)
    {
        projectile.UpdateProjectileMovement -= AddSpiralDisplacement;
        projectile.OnProjectileInit -= SetProjectileAngle;
    }
}
