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

    public void EnableRagdoll(Vector3 knockbackForce)
    {
        ToggleRagdoll(true);

        rigidbodyToPush.AddForce(knockbackForce, ForceMode.Impulse);
    }

    public void DisableRagdoll()
    {
        ToggleRagdoll(false);
    }

    public void ToggleRagdoll(bool shouldEnable = true)
    {
        animatorToDisable.enabled = !shouldEnable;

        foreach (var collider in collidersToDisable)
        {
            collider.enabled = !shouldEnable;
        }
        foreach (var collider in collidersToEnable)
        {
            collider.enabled = shouldEnable;
            collider.isTrigger = !shouldEnable;
        }
        foreach (var rigidbody in rigidbodiesToDisable)
        {
            rigidbody.isKinematic = shouldEnable;
            rigidbody.useGravity = !shouldEnable;
            if (shouldEnable)
                rigidbody.AddForce(-rigidbody.GetAccumulatedForce());
        }
        foreach (var rigidbody in rigidbodiesToEnable)
        {
            rigidbody.isKinematic = !shouldEnable;
            rigidbody.useGravity = shouldEnable;
            if (!shouldEnable)
                rigidbody.AddForce(-rigidbody.GetAccumulatedForce());
        }
    }
}
