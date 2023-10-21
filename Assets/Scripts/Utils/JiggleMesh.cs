using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JiggleMesh : MonoBehaviour
{
    [SerializeField]
    private Material JiggleMat;

    [SerializeField]
    private float elasticity = 4f;

    private Vector3 previousDiff; 

    private Vector3 previousPosition;

    private void Start()
    {
        previousPosition = transform.position;
        previousDiff = Vector3.zero;
    }
    private void LateUpdate()
    {
        Vector3 target = Vector3.Slerp(previousPosition, previousDiff - transform.position, Time.fixedDeltaTime * elasticity);
        var distance = target - transform.position;
        JiggleMat.SetVector("_Distance", target);
        previousPosition = target;
        previousDiff -= distance*0.99f;
        previousDiff /= 2;
    }
}
