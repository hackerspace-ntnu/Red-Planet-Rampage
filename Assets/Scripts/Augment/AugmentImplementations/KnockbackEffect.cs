using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockbackEffect : MonoBehaviour
{
    public void KnockAwayTargets(float pushPower, float radius)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rigidbody))
            {
                rigidbody.AddForce(Vector3.MoveTowards(transform.position, rigidbody.position, 5f) * pushPower, ForceMode.Impulse);
            }
        }
    }

}
