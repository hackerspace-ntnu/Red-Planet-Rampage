using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DDRBody : GunBody
{
    [SerializeField]
    private SkinnedMeshRenderer meshRenderer;

    private Material ddrMaterial;

    private const int screenMaterialIndex = 2;

    private float arrowHeigth = 0;

    private int arrowDirectionQuadrant = 0;

    private const float screenHeigth = 3.5f;
    private const float errorMarginInput = 0.1f;

    private const float targetPoint = 3;
    private const float rangeMeh = 0.5f;
    private const float rangeGood = 0.25f;
    private const float rangeGreat = 0.1f;
    private const float rangeExcellent = 0.05f;

    [SerializeField]
    private float secondPerBeat = 1;


    public override void Start()
    {
        meshRenderer.materials[screenMaterialIndex] = Instantiate(meshRenderer.materials[screenMaterialIndex]);
        ddrMaterial = meshRenderer.materials[screenMaterialIndex];

        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
        {
            Debug.Log("DanceBody not attached to gun parent!");
            return;
        }

        if (gunController.player)
        {
            gunController.player.inputManager.onMovePerformed += ArrowSelect;
            ResetArrow();
            LeanTween.value(gameObject, SetArrowHeigth, 0, screenHeigth, secondPerBeat)
                .setRepeat(-1)
                .setOnComplete(() => ResetArrow());
        }
        
    }

    protected override void Reload(GunStats stats)
    {
        float danceAccuracy = 0;
        if (Mathf.Abs(arrowHeigth - targetPoint) <= rangeExcellent)
            danceAccuracy = 1;
        else if(Mathf.Abs(arrowHeigth - targetPoint) <= rangeGreat)
            danceAccuracy = 0.75f;
        else if (Mathf.Abs(arrowHeigth - targetPoint) <= rangeGood)
            danceAccuracy = 0.5f;
        else if (Mathf.Abs(arrowHeigth - targetPoint) <= rangeMeh)
            danceAccuracy = 0.25f;

        if (danceAccuracy == 0)
            return;

        gunController.Reload(reloadEfficiencyPercentage*danceAccuracy);
    }

    private void ArrowSelect(InputAction.CallbackContext ctx)
    {
        switch (arrowDirectionQuadrant)
        {
            case 0:
                if (!(gunController.player.inputManager.moveInput.y > 1 - errorMarginInput))
                    return;
                break;
            case 1:
                if (!(gunController.player.inputManager.moveInput.x > 1 - errorMarginInput))
                    return;
                break;
            case 2:
                if (!(gunController.player.inputManager.moveInput.y < -1 + errorMarginInput))
                    return;
                break;
            case 3:
                if (!(gunController.player.inputManager.moveInput.x < -1 + errorMarginInput))
                    return;
                break;
        }

        Reload(gunController.stats);
    }

    private void SetArrowHeigth(float heigth)
    {
        ddrMaterial.SetFloat("_ArrowYPos", heigth);
        arrowHeigth = heigth;
    }

    private void SetFlashFactor(float factor)
    {
        ddrMaterial.SetFloat("_FlashFactor", factor);
    }

    private float ArrowDirectionToDegrees(int direction)
    {
        // Default orientation (0 degrees) = 12 O' clock
        return direction*90;
    }

    private void ResetArrow()
    {
        arrowHeigth = 0;
        arrowDirectionQuadrant = Random.Range(0,4);
        ddrMaterial.SetFloat("_ArrowRotationDegrees", ArrowDirectionToDegrees(arrowDirectionQuadrant));
    }

    private void OnDestroy()
    {
        if (gunController)
            gunController.player.inputManager.onMovePerformed -= ArrowSelect;
    }
}
