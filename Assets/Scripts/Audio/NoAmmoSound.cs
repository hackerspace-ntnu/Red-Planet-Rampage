using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoAmmoSound : MonoBehaviour
{
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioGroup noAmmo;
    private GunController gunController;
    void Start()
    {
        gunController = transform.parent.GetComponent<GunController>();
        if (gunController && gunController.Player && gunController.Player is not AIManager)
            gunController.onFireNoAmmo += NoAmmoAudio;
    }

    private void NoAmmoAudio(GunStats _)
    {
        noAmmo.Play(audioSource);
    }

    private void OnDestroy()
    {
        if (gunController && gunController.Player && gunController.Player is not AIManager)
            gunController.onFireNoAmmo -= NoAmmoAudio;
    }
}
