using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JiggleMeshSimple : MonoBehaviour
{
    [SerializeField]
    private Material jiggleMat;
    [SerializeField]
    private Vector3 jiggleForwardDirection = Vector3.up;
    [SerializeField]
    private float elasticity = 4f;
    [SerializeField]
    private float movementSensitivity = 1f;

    private Vector3 previousDiff;
    private Vector3 previousTarget;
    private Vector3 oldPosition;

    void Start()
    {
        previousTarget = transform.position;
        previousDiff = transform.position;
        oldPosition = transform.position;
    }

    void Update()
    {
        Vector3 target = Vector3.Slerp(previousTarget, previousDiff - jiggleForwardDirection, Time.deltaTime * elasticity);
        var distance = target - jiggleForwardDirection;
        jiggleMat.SetVector("_Distance", target);
        previousTarget = target + (oldPosition - transform.position) * movementSensitivity;
        previousDiff -= distance * 0.90f;
        previousDiff /= 2;
        oldPosition = transform.position;
    }
}
