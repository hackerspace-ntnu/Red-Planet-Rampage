using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIK : MonoBehaviour
{
    [Header("Left Arm")]
    [SerializeField]
    private Transform leftArmRoot;
    [SerializeField]
    private float leftArmRootOffset = -70f;
    [SerializeField]
    private Transform leftArmElbow;
    [SerializeField]
    private float leftArmElbowOffset = -1.6f;
    [SerializeField]
    private Transform leftArmHand;
    // Which pole (direction) elbow bends towards
    [SerializeField]
    private Transform leftHandIKPole;
    public Transform LeftHandIKTarget { get; set; }
    


    [Header("Right Arm")]
    [SerializeField]
    private Transform rightArmRoot;
    [SerializeField]
    private float rightArmRootOffset = 70;
    [SerializeField]
    private Transform rightArmElbow;
    [SerializeField]
    private float rightArmElbowOffset = 1.6f;
    [SerializeField]
    private Transform rightArmHand;
    // Which pole (direction) elbow bends towards
    [SerializeField]
    private Transform rightHandIKPole;
    public Transform RightHandIKTarget { get; set; }

    /// <summary>
    /// Manualy sets the rotation of all 3 transforms that will be animated by IK
    /// </summary>
    /// <param name="root">First bone</param>
    /// <param name="joint">Second bone, will extend towards pole when squished</param>
    /// <param name="end">Third and final bone, will always point to target</param>
    /// <param name="pole">The half-sphere direction which the second bone points from</param>
    /// <param name="target">The goal point of the end bone</param>
    private void AnimateTransforms(Transform root, Transform joint, Transform end, Transform pole, Transform target, float rootOffset, float jointOffset)
    {
        float rootLength = root.localPosition.magnitude;
        float endLength = end.localPosition.magnitude;
        float rootLengthToTarget = Vector3.Distance(root.position, target.position);
        Vector3 rotationAxis = Vector3.Cross(target.position - root.position, pole.position - root.position);

        // First bone
        var offsetRotationAxis = Quaternion.AngleAxis(rootOffset, joint.position - root.position) * rotationAxis;

        root.rotation = Quaternion.LookRotation(target.position - root.position, offsetRotationAxis);
        root.rotation *= Quaternion.Inverse(Quaternion.FromToRotation(Vector3.forward, joint.localPosition));
        root.rotation *= Quaternion.AngleAxis(-CosAngle(rootLength, rootLengthToTarget, endLength), -rotationAxis);

        // Second bone
        offsetRotationAxis = Quaternion.AngleAxis(jointOffset, end.position - joint.position) * rotationAxis;

        joint.rotation = Quaternion.LookRotation(target.position - joint.position, offsetRotationAxis);
        joint.rotation *= Quaternion.Inverse(Quaternion.FromToRotation(Vector3.forward, end.localPosition));
    }

    private float CosAngle(float edge1, float edge2, float opposingEdge)
    {
        float angle = Mathf.Acos((-(opposingEdge * opposingEdge) + (edge1 * edge1) + (edge2 * edge2)) / (2 * edge1 * edge2)) * Mathf.Rad2Deg;

        if (float.IsNaN(angle))
            return 1;

        return angle;
    }

    private void LateUpdate()
    {
        if (LeftHandIKTarget)
            AnimateTransforms(leftArmRoot, leftArmElbow, leftArmHand, leftHandIKPole, LeftHandIKTarget, leftArmRootOffset, leftArmElbowOffset);
        if (RightHandIKTarget)
            AnimateTransforms(rightArmRoot, rightArmElbow, rightArmHand, rightHandIKPole, RightHandIKTarget, rightArmRootOffset, rightArmElbowOffset);
    }
}
