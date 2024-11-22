using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DynamiteBarrel : MonoBehaviour
{
    // Start is called before the first frame update
    private GunController gunController;
    void Start()
    {
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController || !gunController.Player)
            return;

        gunController.Player.inputManager.onZoomPerformed += OnZoom;
    }


    private void OnZoom(InputAction.CallbackContext ctx)
    {
        
    }
}
