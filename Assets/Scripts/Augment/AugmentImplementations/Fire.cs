using CollectionExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class Fire : GunExtension
{
    [SerializeField]
    private GameObject fire;

    private GunController gunController;
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip[] lighterSounds;

    [SerializeField]
    private VisualEffect fireTrail;
    [SerializeField]
    private LessJallaVFXPositionEncoder trailPositions;

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
        gunController.projectile.UpdateProjectileMovement += ApplyTrails;
    }

    void Start()
    {
        fireTrail.SetGraphicsBuffer("StartEndPositions", trailPositions.StartEndPositionsBuffer);
    }

    private void ApplyTrails(float distance, ref ProjectileState state)
    {
        if (!state.active)
            return;
        // TODO: Draw longer lines along path instead of many small lines
        trailPositions.AddLine(state.oldPosition, state.position);
        trailPositions.PopulateBuffer();
        fireTrail.SetInt("SpawnCount", 1);
        fireTrail.SendEvent("OnPlay");
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
