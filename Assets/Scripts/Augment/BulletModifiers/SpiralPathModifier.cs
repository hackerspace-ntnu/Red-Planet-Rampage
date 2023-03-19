using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiralPathModifier : MonoBehaviour, ProjectileModifier
{

    //Modifier for adding a spiral movement to any projectile
    public AnimationCurve radialLerp;

    public float spiralRadius, spiralWavelength, spiralLerpDist;

    private float pathTraveled = 0f;

    private float offset = 0f;

    [SerializeField]
    private bool randomAngle = false;

    [SerializeField]
    private Priority priority = Priority.ARBITRARY;

    public void ModifyProjectile(ref ProjectileController projectile)
    {
        projectile.UpdateProjectileMovement += addSpiralDisplacement;
        if (randomAngle)
            offset = Random.Range(0, 2 * Mathf.PI);
    }

    public void addSpiralDisplacement(float distance, ref ProjectileState state, GunStats stats)
    {
        float oldRadius = radialLerp.Evaluate(pathTraveled / spiralLerpDist) * spiralRadius;
        float newRadius = radialLerp.Evaluate((pathTraveled + distance) / spiralLerpDist) * spiralRadius;

        Vector3 oldVector = new Vector3(
            Mathf.Sin(offset + 2 * Mathf.PI * pathTraveled / spiralWavelength), 
            Mathf.Cos(offset + 2 * Mathf.PI * pathTraveled / spiralWavelength)) * oldRadius;
        Vector3 newVector = new Vector3(
            Mathf.Sin(offset + 2 * Mathf.PI * (pathTraveled + distance) / spiralWavelength), 
            Mathf.Cos(offset + 2 * Mathf.PI * (pathTraveled + distance) / spiralWavelength)) * newRadius;

        pathTraveled += distance;

        state.position += state.rotation * (newVector - oldVector);
 
    }

    public Priority GetPriority()
    {
        return priority;
    }
}
