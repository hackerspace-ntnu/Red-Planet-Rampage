using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IcosphereEffectController : MonoBehaviour
{
    [SerializeField]
    private Transform orientation;

    [SerializeField]
    private MeshRenderer meshRenderer;
    private Material material;

    private void Start()
    {
        meshRenderer.materials[0] = Instantiate(meshRenderer.materials[0]);
        material = meshRenderer.materials[0];
    }

    private void Update()
    {
        material.SetVector("_Dir", orientation.right);
    }
}
