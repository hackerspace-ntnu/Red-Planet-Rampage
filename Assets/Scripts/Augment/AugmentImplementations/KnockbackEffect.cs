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
            if (collider.gameObject.TryGetComponent<PlayerManager>(out PlayerManager manager))
            {
                var rigidbody = manager.gameObject.GetComponent<Rigidbody>();
                rigidbody.AddExplosionForce(pushPower, transform.position, radius, 0f, ForceMode.Impulse);
            }
        }
    }

}
