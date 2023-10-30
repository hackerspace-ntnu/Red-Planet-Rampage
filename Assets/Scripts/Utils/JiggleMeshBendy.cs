using UnityEngine;

public class JiggleMesh : MonoBehaviour
{
    [SerializeField]
    private int jiggleMaterialIndex;
    [SerializeField]
    private float elasticity = 4f;

    private Vector3 previousPosition;
    private Vector3 oldPosition;
    private Vector3 oldDeltaPosition;
    private Vector3 animationTarget;
    private Vector3 reflectedAnimationTarget;
    private Vector3 deltaAnimationTarget;
    private MeshRenderer meshRenderer;
    private Material jiggleMaterial;

    private void Start()
    {
        // Intantitate JiggleMaterial
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.materials[jiggleMaterialIndex] = Instantiate(meshRenderer.materials[jiggleMaterialIndex]);
        jiggleMaterial = meshRenderer.materials[jiggleMaterialIndex];
        // Set initial values
        previousPosition = transform.position;
        oldPosition = transform.position;
        animationTarget = transform.position;
    }
    private void Update()
    {
        // Momentum
        Vector3 deltaPosition = (oldPosition - transform.position) * 100;
        deltaPosition = new Vector3(Mathf.Clamp(deltaPosition.x, -50, 50), Mathf.Clamp(deltaPosition.y, -50, 50), Mathf.Clamp(deltaPosition.z, -50, 50));
        deltaPosition = Vector3.Slerp(oldDeltaPosition, deltaPosition, Time.deltaTime * 1f);
        Debug.DrawRay(transform.position, deltaPosition, Color.white);

        // Adjust animationTarget based on momentum loss
        if (oldDeltaPosition.magnitude > deltaPosition.magnitude)
        {
            deltaAnimationTarget = Vector3.Slerp(deltaAnimationTarget, transform.up, Time.deltaTime);
            reflectedAnimationTarget = Vector3.Slerp(reflectedAnimationTarget, transform.up,  Time.deltaTime);
            animationTarget = Vector3.Slerp(deltaAnimationTarget, reflectedAnimationTarget, (1+Mathf.Sin(deltaPosition.magnitude*10))*0.5f);
        }
        else
        {
            deltaAnimationTarget = oldPosition;
            animationTarget = deltaAnimationTarget.normalized;
            reflectedAnimationTarget = Vector3.Reflect(animationTarget, Vector3.Cross(Vector3.Cross(deltaPosition, previousPosition), transform.up)).normalized;
        }

        // Start rotating towards AnimationTarget
        Vector3 animatedPosition = Vector3.Slerp(previousPosition, animationTarget, Time.deltaTime * elasticity);

        // Pass state variables
        jiggleMaterial.SetVector("_Distance", Quaternion.Inverse(transform.rotation) * -animatedPosition.normalized);
        previousPosition = animatedPosition;
        oldPosition = transform.position;
        oldDeltaPosition = deltaPosition;
    }
}
