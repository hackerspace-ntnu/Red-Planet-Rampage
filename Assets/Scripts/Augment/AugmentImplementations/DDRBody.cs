using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DDRBody : GunBody
{
    [SerializeField]
    private SkinnedMeshRenderer meshRenderer;

    private Material ddrMaterial;

    private const int screenMaterialIndex = 2;

    private float arrowHeigth = 0;

    private float arrowRotationDegrees = 0;

    public override void Start()
    {
        meshRenderer.materials[screenMaterialIndex] = Instantiate(meshRenderer.materials[screenMaterialIndex]);
        ddrMaterial = meshRenderer.materials[screenMaterialIndex];

        // TODO: Function instead of logic in fixedUpdate, invoked by sound clip play events
    }

    protected override void Reload(GunStats stats)
    {
        // TODO: lerp flash
        ddrMaterial.SetFloat("_FlashFactor", 5);
        base.Reload(stats);
        ddrMaterial.SetFloat("_FlashFactor", 0);
    }

    private void FixedUpdate()
    {
        // TODO: LeanTween.value event
        arrowHeigth += 0.01f;
        if (arrowHeigth > 3.5f)
        {
            arrowHeigth = 0;
            arrowRotationDegrees = Mathf.Round(Random.value * 4) * 90;
            ddrMaterial.SetFloat("_ArrowRotationDegrees", arrowRotationDegrees);

        }
        ddrMaterial.SetFloat("_ArrowYPos", arrowHeigth);
    }

}
