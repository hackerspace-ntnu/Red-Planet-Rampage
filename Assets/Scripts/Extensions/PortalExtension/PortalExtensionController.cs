using UnityEngine;
using Mirror;

public class PortalExtensionController : GunExtension
{

    [SerializeField]
    private Transform output;

    [SerializeField]
    private GameObject portalMoverPrefab;

    private Transform portalMover;

    private GunController gunController;

    private Transform playerView;
    private InputManager inputManager;

    [SerializeField]
    private LayerMask validPortalSurfaceColliders, validAimColliders;

    private void Start()
    {
        if (!gunController)
            gunController = GetComponentInParent<GunController>();

        if (gunController != null)
        {
            gunController.AimCorrectionEnabled = false;
            inputManager = GetComponentInParent<PlayerManager>().inputManager;
            if (inputManager)
                playerView = inputManager.transform;
        }
    }

    private void OnDestroy()
    {
        if (gunController)
            gunController.AimCorrectionEnabled = true;
        if (portalMover)
            Destroy(portalMover.gameObject);
    }


    private void MovePortal()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerView.position, playerView.forward, out hit, 200, this.validPortalSurfaceColliders))
        {
            portalMover.rotation = Quaternion.LookRotation(-hit.normal, Vector3.up);
            portalMover.position = hit.point + 0.9f * hit.normal;
            CmdMovePortal(portalMover.position, portalMover.rotation);
        }
    }

    [Command]
    private void CmdMovePortal(Vector3 position, Quaternion rotation)
    {
        RpcMovePortal(position, rotation);
    }

    [ClientRpc]
    private void RpcMovePortal(Vector3 position, Quaternion rotation)
    {
        if (inputManager)
            return;

        portalMover.rotation = rotation;
        portalMover.position = position;
    }

    private void AimPortal()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerView.position + 1f * playerView.forward, playerView.forward, out hit, 200, this.validAimColliders))
        {
            portalMover.rotation = Quaternion.LookRotation(portalMover.position - hit.point, Vector3.up);
            CmdAimPortal(portalMover.rotation);
        }
    }

    [Command]
    private void CmdAimPortal(Quaternion rotation)
    {
        RpcAimPortal(rotation);
    }

    [ClientRpc]
    private void RpcAimPortal(Quaternion rotation)
    {
        if (inputManager)
            return;

        portalMover.rotation = rotation;
    }

    private void Update()
    {
        if (!playerView)
            return;
        if (inputManager.ZoomActive)
        {
            MovePortal();
        }
        else
        {
            AimPortal();
        }
    }

    public override Transform[] AttachToTransforms(Transform[] transforms)
    {
        gunController = GetComponentInParent<GunController>();
        var portalMoverObject = Instantiate(portalMoverPrefab);
        output = portalMoverObject.GetComponent<PortalExtensionOutput>().Output;
        portalMoverObject.transform.rotation = transform.rotation;
        portalMoverObject.transform.position = transform.position - output.localPosition;
        portalMover = portalMoverObject.transform;
        outputs = new Transform[] { output };
        var barrel = gunController.GetComponentInChildren<GunBarrel>();
        if (barrel && barrel.MuzzleFlash)
            barrel.MuzzleFlash.enabled = false;
        if (!gunController || !gunController.Player)
            foreach (Transform child in portalMoverObject.transform)
                child.gameObject.SetActive(false);
        return new Transform[] { output };
    }
}
