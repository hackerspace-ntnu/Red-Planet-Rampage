using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeBody : GunBody
{
    [SerializeField]
    private Rope rope;
    private Rigidbody playerBody;

    [SerializeField]
    private float pullForce = 10f;
    [SerializeField]
    private float ropeLength = 60f;

    public override void Start()
    {
        base.Start();
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController || !gunController.Player)
        {
            rope.enabled = false;
            return;
        }
        rope.Target = gunController.Player.GunHolder;
        playerBody = gunController.Player.GetComponent<Rigidbody>();
    }

    private void PullingWire()
    {
        playerBody.AddForce(-(playerBody.position - rope.CurrentAnchor).normalized * pullForce, ForceMode.Acceleration);
        if (rope.RopeLength > ropeLength + 4f)
            rope.ResetRope(gunController.Player.GunHolder.position);
    }


    private void FixedUpdate()
    {
        if (!rope || !rope.enabled)
            return;
        if (rope.RopeLength > ropeLength)
            PullingWire();
            
    }

}
