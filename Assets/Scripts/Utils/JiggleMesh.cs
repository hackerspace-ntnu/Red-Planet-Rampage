using UnityEngine;

public class JiggleMesh : MonoBehaviour
{
    [SerializeField]
    private Material jiggleMat;

    [SerializeField]
    private float elasticity = 4f;

    private Vector3 previousPosition;
    private Vector3 oldPosition;
    private Vector3 oldDeltaPosition;
    private Vector3 animationTarget;
    private Vector3 reflectedAnimationTarget;
    private Vector3 deltaAnimationTarget;

    private void Start()
    {
        previousPosition = transform.position;
        oldPosition = transform.position;
        animationTarget = transform.position;
    }
    private void LateUpdate()
    {
        // Momentum
        Vector3 deltaPosition = (oldPosition - transform.position) * 100;// + (animationTarget + transform.up));
        deltaPosition = new Vector3(Mathf.Clamp(deltaPosition.x, -50, 50), Mathf.Clamp(deltaPosition.y, -50, 50), Mathf.Clamp(deltaPosition.z, -50, 50));
        deltaPosition = Vector3.Slerp(oldDeltaPosition, deltaPosition, Time.deltaTime * 1f);
        //Debug.DrawRay(transform.position, deltaPosition, Color.white);

        // Adjust animationTarget based on momentum loss
        if (oldDeltaPosition.magnitude > deltaPosition.magnitude)
        {
            deltaAnimationTarget = Vector3.Slerp(deltaAnimationTarget, transform.up, 0.01f*Time.deltaTime);
            reflectedAnimationTarget = Vector3.Slerp(reflectedAnimationTarget, transform.up, 0.01f * Time.deltaTime);
            animationTarget = Vector3.Slerp(deltaAnimationTarget, reflectedAnimationTarget, (1+Mathf.Sin(deltaPosition.magnitude*10))*0.5f);
        }
        else
        {
            deltaAnimationTarget = deltaPosition;
            animationTarget = deltaAnimationTarget.normalized;
            reflectedAnimationTarget = Vector3.Reflect(animationTarget, Vector3.Cross(Vector3.Cross(deltaPosition, previousPosition), transform.up)).normalized;
        }
        //Debug.DrawRay(Vector3.zero, animationTarget+transform.position, Color.blue);
        //Debug.DrawRay(transform.position, reflectedAnimationTarget, Color.green);

        // Start rotating towards momentum Target
        Vector3 animatedPosition = Vector3.Slerp(previousPosition, animationTarget, Time.deltaTime * elasticity);
        //Debug.DrawRay(transform.position, animatedPosition, Color.red);

        // Pass state variables
        jiggleMat.SetVector("_Distance", Quaternion.Inverse(transform.rotation) * animatedPosition.normalized);
        previousPosition = animatedPosition;
        oldPosition = transform.position;
        oldDeltaPosition = deltaPosition;
    }
}
