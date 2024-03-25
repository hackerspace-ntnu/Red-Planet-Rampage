using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeBody : GunBody
{
    [SerializeField]
    private Rope rope;

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
    }

}
