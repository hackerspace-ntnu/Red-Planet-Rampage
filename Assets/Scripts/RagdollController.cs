using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    private Transform camera;

    private Transform inputTransform;

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
        camera.cullingMask = ragdollMask;
        this.camera = camera.transform;
        inputTransform = input;
        StartCoroutine(WaitAndDisableCamera(camera));
    }

    private IEnumerator WaitAndDisableCamera(Camera camera)
    {
        yield return new WaitForSeconds(7f);
        camera.enabled = false;
    }

    private void Update()
    {
        if (!camera)
            return;
        camera.position = cameraParent.position + new Vector3(0, 2, -5f);
        camera.LookAt(cameraParent);
    }

    private void OnDestroy()
    {
        if(!camera)
            return;
        camera.position = Vector3.zero;
        camera.localPosition = Vector3.zero;
        camera.rotation = inputTransform.rotation;
    }
}
