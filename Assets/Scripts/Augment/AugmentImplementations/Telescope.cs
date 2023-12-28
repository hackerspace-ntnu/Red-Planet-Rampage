using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Telescope : GunExtension
{
    [SerializeField]
    private float enhancedZoomFov = 20f;
    
    void Start()
    {
        var gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
        {
            Debug.Log("Telescope not attached to gun parent!");
            return;
        }

        if (gunController.Player)
        {
            gunController.Player.GetComponent<PlayerMovement>().ZoomFov = enhancedZoomFov;
        }
    }
}
