using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunExtension : GunModifier
{

    public override int priority { get => 2; }

    [SerializeField]
    private GameObject model;

    // The projectile is shot from each of these outputs, might want to change this iw we want varying outputs, like alternate shooting
    [SerializeField]
    public Transform[] outputs;

    public void AttachToTransforms(Transform[] transforms)
    {
        foreach(var t in transforms[1..])
        {
            Instantiate(model, t.position, t.rotation, transform);
        }
    }
}