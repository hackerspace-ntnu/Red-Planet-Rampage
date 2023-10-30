using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class JiggleMeshSimple : MonoBehaviour
{
    [SerializeField]
    private int jiggleMaterialIndex;
    [SerializeField]
    private Vector3 jiggleForwardDirection = Vector3.up;
    [SerializeField]
    private float elasticity = 4f;
    [SerializeField]
    private float movementSensitivity = 1f;

    private Vector3 previousDiff;
    private Vector3 previousTarget;
    private Vector3 oldPosition;

    private MeshRenderer meshRenderer;
    private Material jiggleMaterial;

    void Start()
    {
        // Intantitate JiggleMaterial
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.materials[jiggleMaterialIndex] = Instantiate(meshRenderer.materials[jiggleMaterialIndex]);
        jiggleMaterial = meshRenderer.materials[jiggleMaterialIndex];
        // Set initial values
        previousTarget = transform.position;
        previousDiff = transform.position;
        oldPosition = transform.position;
    }

    void Update()
    {
        Vector3 target = Vector3.Slerp(previousTarget, previousDiff - jiggleForwardDirection, Time.deltaTime * elasticity);
        var distance = target - jiggleForwardDirection;
        jiggleMaterial.SetVector("_Distance", target);
        previousTarget = target + (oldPosition - transform.position) * movementSensitivity;
        previousDiff -= distance * 0.90f;
        previousDiff /= 2;
        oldPosition = transform.position;
    }
}
