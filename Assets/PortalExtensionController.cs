using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;


public class PortalExtensionController : GunExtension
{

    [SerializeField]
    private Transform output;

    [SerializeField]
    private GameObject portalMoverPrefab;

    private Transform portalMover;

    private GunController controller;
    //private bool isViewPort;

    //[SerializeField]
    //private GameObject portalIn, portalInPlayerView;

    private Transform playerView;
    private InputManager inputManager;

    [SerializeField]
    private LayerMask validPortalSurfaceColliders, validAimColliders;

    private void Start()
    {
        controller = GetComponentInParent<GunController>();
        //inputManager = GetComponentInParent<InputManager>();
    
        if(controller != null)
        {
            controller.AimCorrectionEnabled = false;
            inputManager = GetComponentInParent<PlayerManager>().inputManager;
            playerView = inputManager.transform;
        }


        //if (isViewPort)
        //{
        //    portalIn.SetActive(false);
        //    portalInPlayerView.SetActive(true);
        //}
        
    }


    private void OnDestroy()
    {
        if(controller)
            controller.AimCorrectionEnabled = true;
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
        var portalMoverObject = Instantiate(portalMoverPrefab);
        output = portalMoverObject.GetComponent<PortalExtensionOutput>().Output;
        portalMoverObject.transform.rotation = transform.rotation;
        portalMoverObject.transform.position = transform.position - output.localPosition;
        portalMover = portalMoverObject.transform;
        outputs = new Transform[] { output };
        return new Transform[] { output };
    }


}
