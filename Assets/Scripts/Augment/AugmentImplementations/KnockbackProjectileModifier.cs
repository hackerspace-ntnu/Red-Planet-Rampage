using System.Collections;
using System.Collections.Generic;
using TransformExtensions;
using Unity.VisualScripting;
using UnityEngine;

public class KnockbackProjectileModifier : MonoBehaviour, ProjectileModifier
{
    [SerializeField]
    private float pushPower = 50f;

    [SerializeField]
    private GameObject stuckObject;

    [SerializeField]
    private float knockbackRadius;

    private PlayerManager source;

    public void Attach(ProjectileController projectile)
    {
        projectile.OnColliderHit += KnockAwayTarget;
        source = projectile.player;
    }

    public void Detach(ProjectileController projectile)
    {
        projectile.OnColliderHit -= KnockAwayTarget;
    }

    public void KnockAwayTarget(Collider collider, ref ProjectileState state)
    {
        var stuck = Instantiate(stuckObject, state.position, state.rotation, null);
        stuck.transform.ParentUnscaled(collider.transform);
        //if (collider.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rigidbody))
        //{
        //    rigidbody.AddForce(Vector3.up * pushPower, ForceMode.Impulse);
        //}
        source.gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * pushPower, ForceMode.Impulse);
        Destroy(stuck, 0.1f);
    }
}
