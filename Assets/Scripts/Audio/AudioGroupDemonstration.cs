using UnityEngine;
using System;
using CollectionExtensions;

public class AudioGroupDemonstration : MonoBehaviour
{
    [SerializeField]
    private AudioSource emitterA;
    [SerializeField]
    private AudioSource emitterB;
    [SerializeField]
    private AudioSource emitterC;
    AudioSource[] emitters = new AudioSource[3];

    // For demonstration purposes, let's say this script emulates a weapon with two different attacks
    // With that, we also have two separate "types" of sounds we want to emit, based on which attack we use
    [SerializeField]
    private AudioGroup sounds;
    [SerializeField]
    private AudioGroup sharpSounds;

    [SerializeField, Uneditable]
    private AudioGroup activeSoundGroup;

    private void Awake()
    {
        emitters[0] = emitterA;
        emitters[1] = emitterB;
        emitters[2] = emitterC;
        activeSoundGroup = sounds;
    }
    private void Update()
    {
        // Right click to switch weapon mode
        if (Input.GetMouseButtonDown(1))
        {
            if (activeSoundGroup == sounds)
                activeSoundGroup = sharpSounds;
            else
                activeSoundGroup = sounds;
        }

        if (Input.GetMouseButtonDown(0))
        {
            activeSoundGroup.Play(emitterA);
        }
        else if (Input.GetMouseButtonDown(3))
        {
            activeSoundGroup.Play(emitterB);
        }
        else if (Input.GetMouseButtonDown(4))
        {
            activeSoundGroup.Play(emitterC);
        }
        else if (Input.GetMouseButtonDown(2))
        {
            activeSoundGroup.Play(emitters.RandomElement());
        }
    }
}
