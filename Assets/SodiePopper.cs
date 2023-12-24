using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SodiePopper : GunBody
{
    // Start is called before the first frame update

    Vector3 currentOffset = Vector3.zero;

    [SerializeField]
    float offsetMagnitude = 0.1f;

    // Prevents the players movement from being too dominant in the shaking
    [SerializeField]
    float playermovementRelativeAdjustment = 0.4f;
    Transform playerTransform;
    Vector3 playerLastPos;
    Vector3 playerLastLastPos;


    Vector3 lastPos = Vector3.zero;
    Vector3 lastLastPos = Vector3.zero;

    Vector3 lastMeassurementPos = Vector3.zero;

    [SerializeField]
    float shakeCorrectionReloadThreshold = 0.003f;

    [SerializeField]
    float reload_delay = 0.1f;

    [SerializeField]
    float reload_ammount = 0.2f;

    float lastReload = 0f;

    [SerializeField]
    float offsetScaling = 0.1f;
    [SerializeField]
    float velocityDamping = 0.1f;
    [SerializeField]
    float springStrength = 0.1f;

    [SerializeField]
    Transform meassurementPoint = null;

    [SerializeField]
    Transform offsetTarget = null;

    [SerializeField]
    private Animator anim;

    public override void Start()
    {
        gunController = transform.parent?.GetComponent<GunController>();
        lastMeassurementPos = meassurementPoint.position;

        lastPos = lastLastPos = meassurementPoint.position;

        if (!gunController)
            return;
        gunController.HasRecoil = false;

        if(!gunController.player)
            return;
        playerLastPos = playerLastLastPos = gunController.player.transform.position;
    }

    private void Update()
    {
        if( gunController != null)
        {
            anim.SetFloat("Ammo", (float)gunController.stats.Ammo/(float)gunController.stats.magazineSize);
        }
    }

    private void FixedUpdate()
    {
        Vector3 positionDiff = meassurementPoint.position - lastMeassurementPos;

        lastMeassurementPos = meassurementPoint.position;

        Vector3 offsetVelocity = (lastPos - lastLastPos) / Time.fixedDeltaTime;

        offsetVelocity -= offsetVelocity * velocityDamping * Time.fixedDeltaTime;

        offsetVelocity += (meassurementPoint.position - lastPos) * springStrength * Time.fixedDeltaTime;

        if (playerTransform)
        {
            Vector3 playerSpeedDiff = (playerLastLastPos - 2 * playerLastPos + playerTransform.position)/Time.fixedDeltaTime;
            offsetVelocity += playerSpeedDiff * playermovementRelativeAdjustment;
            
            playerLastLastPos = playerLastPos;
            playerLastPos = playerTransform.position;
        }

        lastLastPos = lastPos;
        
        lastPos += offsetVelocity * Time.fixedDeltaTime;

        if( (lastPos - meassurementPoint.position).magnitude > offsetMagnitude)
        {
            Vector3 diff = lastPos - meassurementPoint.position;

            if(diff.magnitude >= shakeCorrectionReloadThreshold && Time.fixedTime - lastReload > reload_delay)
            {
                lastReload = Time.fixedTime;
                
                gunController?.Reload(reload_ammount);
            }

            lastPos = meassurementPoint.position + diff.normalized * offsetMagnitude;

        }
        offsetTarget.position = lastPos;

    }
}
