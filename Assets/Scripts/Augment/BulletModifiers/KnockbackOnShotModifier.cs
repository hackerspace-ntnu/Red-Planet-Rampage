using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class KnockbackOnShotModifier: MonoBehaviour, ProjectileModifier
{
    [SerializeField]
    private GameObject knockbackSourceObject;

    [SerializeField]
    private float pushPower;

    [SerializeField]
    private float radius;

    private PlayerManager source;

    private Item barrelPrefab;

    public void Attach(ProjectileController projectile)
    {
        projectile.OnProjectileInit += KnockAwayOnShot;
        source = projectile.player;
    }

    public void Detach(ProjectileController projectile)
    {
        projectile.OnProjectileInit -= KnockAwayOnShot;
    }

    public void KnockAwayOnShot(ref ProjectileState state, GunStats stats)
    {

        var knockObject = Instantiate(knockbackSourceObject, state.position, state.rotation, null);    
        knockObject.gameObject.GetComponent<KnockbackEffect>().KnockAwayTargets(pushPower, radius);
        Destroy(knockObject, 0.2f);
    }
}
