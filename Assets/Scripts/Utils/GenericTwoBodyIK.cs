using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericTwoBodyIK : MonoBehaviour
{
    /// <summary>
    /// Manually sets the rotation of all 3 given transforms that will be animated by IK.
    /// Transfoms should have a capsule collider to define length, else localScale magnitude will be used (which is less accurate)
    /// 
    /// Note: Due to scaling of world distances compared to localscale, 
    /// one might have to tweak the localScaleOffset to get proper bending, usually in the range of [0.5] or smaller
    /// </summary>
    /// <param name="root">First bone</param>
    /// <param name="joint">Second bone, will extend towards pole when squished</param>
    /// <param name="end">Third and final bone, will always point to target</param>
    /// <param name="pole">The half-sphere direction which the second bone points from</param>
    /// <param name="target">The goal point of the end bone</param>
    /// <param name="rootOffset">Optional extra rotation of root around its IK forward axis</param>
    /// <param name="jointOffset">optinal extra rotation of joint around its IK forward axis</param>
    /// <param name="localScaleOffset">Optional relative scaling for achieving distances comparable </param>
    public static void AnimateTransforms(Transform root, Transform joint, Transform end, Transform pole, Transform target, float rootOffset = 0, float jointOffset = 0, float localScaleOffset = 1)
    {
        float jointLength = joint.TryGetComponent(out CapsuleCollider jointCollider) ? jointCollider.height : joint.localPosition.magnitude;
        float rootLength = root.TryGetComponent(out CapsuleCollider rootCollider) ? rootCollider.height : root.localPosition.magnitude;
        float rootLengthToTarget = Vector3.Distance(root.position, target.position) * localScaleOffset;
        Vector3 rotationAxis = Vector3.Cross(target.position - root.position, pole.position - root.position);

        // First bone
        var offsetRotationAxis = Quaternion.AngleAxis(rootOffset, joint.position - root.position) * rotationAxis;

        root.rotation = Quaternion.LookRotation(target.position - root.position, offsetRotationAxis);
        root.rotation *= Quaternion.Inverse(Quaternion.FromToRotation(Vector3.forward, joint.localPosition));
        root.rotation = Quaternion.AngleAxis(-CosAngle(jointLength, rootLengthToTarget, rootLength), -rotationAxis) * root.rotation;

        // Second bone
        offsetRotationAxis = Quaternion.AngleAxis(jointOffset, end.position - joint.position) * rotationAxis;

        joint.rotation = Quaternion.LookRotation(target.position - joint.position, offsetRotationAxis);
        joint.rotation *= Quaternion.Inverse(Quaternion.FromToRotation(Vector3.forward, end.localPosition));

    }

    private static float CosAngle(float edge1, float edge2, float opposingEdge)
    {
        float angle = Mathf.Acos((-(opposingEdge * opposingEdge) + (edge1 * edge1) + (edge2 * edge2)) / (2 * edge1 * edge2)) * Mathf.Rad2Deg;

        if (float.IsNaN(angle))
            return 1;

        return angle;
    }
}
