using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WobbleMesh : MonoBehaviour
{
    [SerializeField]
    private Material jiggleMaterial;
    [SerializeField]
    protected int jiggleMaterialIndex = 2;
    [SerializeField]
    private float wobbleOriginOffset = 0.1f;
    [SerializeField]
    private float elasticity = 4f;
    [SerializeField]
    private float distanceScale = 1f;

    private Vector3 previousPosition;
    private Vector3 momentum = Vector3.zero;

    protected MeshRenderer meshRenderer;

    private void Start()
    {
        // Intantitate JiggleMaterial
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.materials[jiggleMaterialIndex] = Instantiate(meshRenderer.materials[jiggleMaterialIndex]);
        jiggleMaterial = meshRenderer.materials[jiggleMaterialIndex];
        jiggleMaterial.SetFloat("_BendStartOffset", wobbleOriginOffset);
        previousPosition = transform.position;
    }
    private void Update()
    {
        Vector3 target = Vector3.Slerp(previousPosition, transform.position - momentum, Time.deltaTime * elasticity);
        momentum = (target - transform.position);
        var distance = target - transform.position;
        jiggleMaterial.SetVector("_Distance", Quaternion.Inverse(transform.rotation) * -distance * distanceScale);
        previousPosition = target;
    }
}
