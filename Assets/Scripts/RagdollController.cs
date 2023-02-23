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

    public void EnableRagdoll()
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
    }
}
