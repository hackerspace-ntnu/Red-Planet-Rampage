using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FiringSound : MonoBehaviour
{
    [SerializeField]
    private AudioGroup firingSound;

    private AudioSource audioSource;
    private ProjectileController projectile;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        projectile = GetComponent<ProjectileController>();
        projectile.OnProjectileInit += PlaySound;
    }

    private void OnDestroy()
    {
        projectile.OnProjectileInit -= PlaySound;
    }

    private void PlaySound(ref ProjectileState state, GunStats stats)
    {
        firingSound.Play(audioSource);
    }
}
