using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WobbleMesh : MonoBehaviour
{
    [SerializeField]
    private Material jiggleMaterial;

    [SerializeField]
    private float elasticity = 4f;
    [SerializeField]
    [Range(0,1)]
    private float slerpSpeed = 0.1f;
    [SerializeField]
    private float maxBendDistance = 1f;

    private Vector3 previousPosition;
    private Vector3 momentum = Vector3.zero;

    private void Start()
    {
        previousPosition = transform.position;
    }
    private void Update()
    {
        Vector3 target = Vector3.Slerp(previousPosition, transform.position + momentum, Time.deltaTime * elasticity);
        momentum = (target - transform.position) * slerpSpeed;
        var distance = target - transform.position;
        distance = new Vector3(Mathf.Clamp(distance.x, -maxBendDistance, maxBendDistance), Mathf.Clamp(distance.y, -maxBendDistance, maxBendDistance), Mathf.Clamp(distance.z, -maxBendDistance/2, maxBendDistance/2));
        jiggleMaterial.SetVector("_Distance", Quaternion.Inverse(transform.rotation) * distance);
        previousPosition = target;
    }
}
