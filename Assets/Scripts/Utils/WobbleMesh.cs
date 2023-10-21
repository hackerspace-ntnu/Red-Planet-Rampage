using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WobbleMesh : MonoBehaviour
{
    [SerializeField]
    private Material JiggleMat;

    [SerializeField]
    private float elasticity = 4f;

    private Vector3 previousPosition;

    private void Start()
    {
        previousPosition = transform.position;
    }
    private void LateUpdate()
    {
        Vector3 target = Vector3.Slerp(previousPosition, transform.position, Time.fixedDeltaTime * elasticity);
        var distance = target - transform.position;
        JiggleMat.SetVector("_Distance", distance);
        previousPosition = target;
    }
}
