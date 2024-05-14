using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;
using UnityEngine.VFX;

public class PortalExtensionController : GunExtension
{

    [SerializeField]
    private Transform output;

    [SerializeField]
    private GameObject portalMoverPrefab;

    private Transform portalMover;

    private GunController controller;

    private Transform playerView;
    private InputManager inputManager;

    [SerializeField]
    private LayerMask validPortalSurfaceColliders, validAimColliders;

    private void Start()
    {
        if (!controller)
            controller = GetComponentInParent<GunController>();
    
        if(controller != null)
        {
            controller.AimCorrectionEnabled = false;
            inputManager = GetComponentInParent<PlayerManager>().inputManager;
            playerView = inputManager.transform;
        }
    }


    private void OnDestroy()
    {
        if(controller)
            controller.AimCorrectionEnabled = true;
        if (portalMover)
            Destroy(portalMover.gameObject);
    }


    private void MoveToAim()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerView.position, playerView.forward, out hit, 200, this.validPortalSurfaceColliders))
        {
            portalMover.rotation = Quaternion.LookRotation(-hit.normal, Vector3.up);
            portalMover.position = hit.point + 0.9f * hit.normal;
        }
    }
    private void AimAt()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerView.position + 1f * playerView.forward, playerView.forward, out hit, 200, this.validAimColliders))
        {
            portalMover.rotation = Quaternion.LookRotation(portalMover.position - hit.point, Vector3.up);
        }
    }
    private void Update()
    {
        if (!playerView)
            return;
        if (inputManager.ZoomActive)
        {
            MoveToAim();
        }
        else
        {
            AimAt();
        }
       
    }
    public override Transform[] AttachToTransforms(Transform[] transforms)
    {
        controller = GetComponentInParent<GunController>();
        var portalMoverObject = Instantiate(portalMoverPrefab);
        output = portalMoverObject.GetComponent<PortalExtensionOutput>().Output;
        portalMoverObject.transform.rotation = transform.rotation;
        portalMoverObject.transform.position = transform.position - output.localPosition;
        portalMover = portalMoverObject.transform;
        outputs = new Transform[] { output };
        var barrel = controller.GetComponentInChildren<GunBarrel>();
        if (barrel && barrel.MuzzleFlash)
            barrel.MuzzleFlash.enabled = false;
        if (!controller || !controller.Player)
            foreach (Transform child in portalMoverObject.transform)
                child.gameObject.SetActive(false);
        return new Transform[] { output };
    }


}
