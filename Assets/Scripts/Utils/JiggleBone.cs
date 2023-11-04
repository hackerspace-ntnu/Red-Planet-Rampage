using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JiggleBone : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve wobbleInterpolationCurve;
    [SerializeField]
    private float jiggleCoefficient = 20f;
    [SerializeField]
    private float maxPositionDelta = 2f;
    [SerializeField]
    private float maxRotationDegrees = 100f;

    private Vector3 previousPosition;
    private Vector3 targetPosition;
    private Vector3 animationPosition;
    private Quaternion previousRotation;
    private Quaternion targetRotation;
    private Quaternion animationRotation;

    void Awake()
    {
        previousPosition = transform.position;
        previousRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        animationPosition = transform.position;
        animationRotation = transform.rotation;
        targetPosition = Vector3.Slerp(previousPosition, transform.position, Time.deltaTime * jiggleCoefficient);
        targetRotation = Quaternion.Slerp(previousRotation, transform.rotation, Time.deltaTime * jiggleCoefficient);

        transform.position = Vector3.MoveTowards(animationPosition, targetPosition, maxPositionDelta);
        transform.rotation = Quaternion.RotateTowards(animationRotation, targetRotation, maxRotationDegrees);

        previousPosition = transform.position;
        previousRotation = transform.rotation;
    }
}
