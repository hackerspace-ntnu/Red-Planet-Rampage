using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class JiggleMesh : MonoBehaviour
{
    [SerializeField]
    protected int jiggleMaterialIndex;
    [SerializeField]
    protected float jiggleFalloff = 0.9f;
    [SerializeField]
    protected float elasticity = 4f;
    [SerializeField]
    protected float movementSensitivity = 1f;

    protected Vector3 previousDiff;
    protected Vector3 previousTarget;
    protected Vector3 oldPosition;

    protected MeshRenderer meshRenderer;
    protected Material jiggleMaterial;

    void Start()
    {
        // Intantitate JiggleMaterial
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.materials[jiggleMaterialIndex] = Instantiate(meshRenderer.materials[jiggleMaterialIndex]);
        jiggleMaterial = meshRenderer.materials[jiggleMaterialIndex];
        // Set initial value
        oldPosition = transform.position;
    }

    void Update()
    {
        Vector3 target = Vector3.Slerp(previousTarget, previousDiff, Time.deltaTime * elasticity);
        jiggleMaterial.SetVector("_Distance", target);
        previousTarget = target + Quaternion.Inverse(transform.rotation) * -(oldPosition - transform.position) * movementSensitivity;
        previousDiff -= target * jiggleFalloff;
        previousDiff /= 2;
        oldPosition = transform.position;
    }
}
