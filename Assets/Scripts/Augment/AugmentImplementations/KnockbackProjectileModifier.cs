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
    }

    public void Detach(ProjectileController projectile)
    {
        projectile.OnColliderHit -= KnockAwayTarget;
    }

    public void KnockAwayTarget(Collider collider, ref ProjectileState state)
    {
        var stuck = Instantiate(stuckObject, state.position, state.rotation, null);
        stuck.transform.ParentUnscaled(collider.transform);
        stuck.gameObject.GetComponent<KnockbackEffect>().KnockAwayTargets(pushPower, knockbackRadius, stuck.transform.position);
        Destroy(stuck, 0.1f);
    }
}
