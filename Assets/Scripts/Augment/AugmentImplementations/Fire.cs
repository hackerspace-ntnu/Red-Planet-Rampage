using CollectionExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Fire : GunExtension
{
    [SerializeField]
    private GameObject fire;

    private GunController gunController;
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip[] lighterSounds;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
        {
            Debug.Log("Fire not attached to gun parent!");
            return;
        }
        gunController.onInitializeGun += AddFireToProjectile;
        gunController.onFireEnd += PlayShotAudio; 
    }


    private void AddFireToProjectile(GunStats gunstats)
    {
        GameObject fireObject = Instantiate(fire, gunController.projectile.transform);
        fireObject.SetActive(false);
    }

    private void PlayShotAudio(GunStats stats)
    {
        if (!gunController)
            return;
        audioSource.clip = lighterSounds.RandomElement();
        audioSource.Play();
    }
}
