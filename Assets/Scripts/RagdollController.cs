using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    [SerializeField]
    private List<Collider> collidersToDisable;

    [SerializeField]
    private List<Collider> collidersToEnable;

    [SerializeField]
    private List<Rigidbody> rigidbodiesToDisable;

    [SerializeField]
    private List<Rigidbody> rigidbodiesToEnable;

    [SerializeField]
    private Animator animatorToDisable;

    [SerializeField]
    private Rigidbody rigidbodyToPush;

    [SerializeField]
    private Transform cameraParent;

    [SerializeField]
    private LayerMask ragdollMask;

    private OrbitCamera orbitCamera;

    private void Start()
    {
        orbitCamera = GetComponent<OrbitCamera>();
    }

    public void EnableRagdoll(Vector3 knockbackForce)
    {
        animatorToDisable.enabled = false;

        foreach (var collider in collidersToDisable)
        {
            collider.enabled = false;
        }
        foreach (var collider in collidersToEnable)
        {
            collider.enabled = true;
            collider.isTrigger = false;
        }
        foreach (var rigidbody in rigidbodiesToDisable)
        {
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
            rigidbody.velocity = Vector3.zero;
        }
        foreach (var rigidbody in rigidbodiesToEnable)
        {
            rigidbody.isKinematic = false;
            rigidbody.useGravity = true;
        }


        rigidbodyToPush.AddForce(knockbackForce, ForceMode.Impulse);
    }

    public void AnimateCamera(Camera camera, Transform input)
    {
        // TODO push this stuff inside
        camera.cullingMask = ragdollMask;
        orbitCamera.Camera = camera.transform;
        orbitCamera.InputManager = input.GetComponent<InputManager>();
        orbitCamera.StartTracking(cameraParent);
        StartCoroutine(WaitAndDisableCamera(camera));
    }

    private IEnumerator WaitAndDisableCamera(Camera camera)
    {
        yield return new WaitForSeconds(7f);
        orbitCamera.StopTracking();
        // TODO put this inside orbit?
        camera.enabled = false;
    }
}
