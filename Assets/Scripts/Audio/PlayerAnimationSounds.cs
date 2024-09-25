using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationSounds : MonoBehaviour
{
    [SerializeField]
    private PlayerMovement movement;
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioGroup walkSoundsDefault;
    [SerializeField]
    private AudioGroup walkSoundsWood;
    [SerializeField]
    private AudioGroup walkSoundsMetal;

    public void PlayWalkSound()
    {
        if (movement.Ground != null && movement.Body.velocity.magnitude > 1f && !audioSource.isPlaying)
            switch (movement.Ground.tag)
            {
                case "wood":
                    walkSoundsWood.Play(audioSource);
                    break;
                case "metal":
                    walkSoundsMetal.Play(audioSource);
                    break;
                default:
                    walkSoundsDefault.Play(audioSource);
                    break;
            }
    }
}
