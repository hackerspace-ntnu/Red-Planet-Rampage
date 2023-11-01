using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubberSniper : GunExtension
{
    [SerializeField]
    private FloppyExtensionJiggleMesh jigglePhysics;
    [SerializeField]
    private AudioSource audioSource;

    private GunController gunController;
    void Awake()
    {
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
        {
            Debug.Log("Fire not attached to gun parent!");
            return;
        }
        jigglePhysics.player = gunController.player;
        gunController.onFire += Fire;
    }

    private void Fire(GunStats stats)
    {
        audioSource.Play();
        jigglePhysics.AnimatePushback();
    }
}
