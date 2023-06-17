using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FiringSound : MonoBehaviour
{
    [SerializeField]
    private AudioGroup firingSound;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        var projectile = GetComponent<ProjectileController>();
        projectile.OnProjectileInit += (ref ProjectileState state, GunStats stats) => PlaySound(firingSound);
    }
    private void PlaySound(AudioGroup sound)
    {
        sound.Play(audioSource);
    }
}
