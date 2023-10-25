using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockbackEffect : MonoBehaviour
{
    public void KnockAwayTargets(float pushPower, float radius, Vector3 origin)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.TryGetComponent<PlayerManager>(out PlayerManager manager))
            {
                var rigidbody = manager.gameObject.GetComponent<Rigidbody>();
                rigidbody.AddExplosionForce(pushPower, origin, radius, 0f, ForceMode.Impulse);
            }
        }
    }

    public void KnockAwayTargetsDirectional(float pushPower, Vector3 normal, PlayerManager source, float radius)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        Vector3 playerPush = normal * -1;

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.TryGetComponent<PlayerManager>(out PlayerManager manager))
            {
                var rigidbody = manager.gameObject.GetComponent<Rigidbody>();

                if (manager.Equals(source))
                {
                    rigidbody.AddForce(playerPush * pushPower, ForceMode.Impulse);
                }
                else
                {
                    rigidbody.AddForce(normal * pushPower, ForceMode.Impulse);
                }
            }
        }
    }
}
