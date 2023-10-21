using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JiggleBone : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve wobbleInterpolationCurve;
    [SerializeField]
    private float jiggleCoefficient = 10f;
    [SerializeField]
    private float maxPositionDelta = 1f;
    [SerializeField]
    private float maxRotationDegrees = 20f;

    private Vector3 previousPosition;
    private Quaternion previousRotation;
    void Awake()
    {
        previousPosition = transform.position;
        previousRotation = transform.rotation;
    }

    private IEnumerator InterpolateTransform(Vector3 position, Vector3 positionTarget, Quaternion rotation, Quaternion rotationTarget, float time)
    {
        var timePassed = 0f;
        while (timePassed < time)
        {
            timePassed += Time.deltaTime;
            float curveTime = wobbleInterpolationCurve.Evaluate(time / timePassed);
            var positionSlerp = Vector3.Slerp(position, transform.position, curveTime);
            var rotationSlerp = Quaternion.Slerp(rotation, transform.rotation, curveTime);

            transform.position = Vector3.MoveTowards(positionTarget, positionSlerp, maxPositionDelta);
            transform.rotation = Quaternion.RotateTowards(rotationTarget, rotationSlerp, maxRotationDegrees);


            previousPosition = transform.position;
            previousRotation = transform.rotation;
            yield return null;
        }
    }


    private void LateUpdate()
    {
        var currentPosition = transform.position;
        var currentRotation = transform.rotation;
        StartCoroutine(InterpolateTransform(previousPosition, currentPosition, previousRotation, currentRotation, jiggleCoefficient*Time.deltaTime));
    }

    /*
     //Mesh has just been animated
        animatedBoneWorldPosition = transform.position;
        animatedBoneWorldRotation = transform.rotation;
        goalPosition = Vector3.Slerp(oldBoneWorldPosition, transform.position, Time.deltaTime * bounceFactor);
        goalRotation = Quaternion.Slerp(oldBoneWorldRotation, transform.rotation, Time.deltaTime * wobbleFactor);
 
        transform.rotation = Quaternion.RotateTowards(animatedBoneWorldRotation, goalRotation, maxRotationDegrees);
        transform.position = Vector3.MoveTowards(animatedBoneWorldPosition, goalPosition, maxTranslation);
 
        oldBoneWorldPosition = transform.position;
        oldBoneWorldRotation = transform.rotation;
        */
}
