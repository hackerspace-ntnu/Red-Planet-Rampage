using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerSelectAnimationManager : MonoBehaviour
{

    [SerializeField]
    private Animator animator;

    private Material eyeMaterial;

    public void Start()
    {
        // Find eye material
        SkinnedMeshRenderer eyeRenderer = transform.GetChild(0).GetComponent<SkinnedMeshRenderer>();
        eyeMaterial = eyeRenderer.materials[1];
        resetEyePosition();
        animator.SetTrigger("cardPeek");
    }

    /// <summary>
    /// Resets eye position to default. Looking straight ahead.
    /// </summary>
    public void resetEyePosition()
    {
        eyeMaterial.SetFloat("_LeftUpperLid", 0.12f);
        eyeMaterial.SetFloat("_LeftLowerLid", 0.2f);
        eyeMaterial.SetFloat("_RightUpperLid", 0.12f);
        eyeMaterial.SetFloat("_RightLowerLid", 0.2f);

        eyeMaterial.SetVector("_LeftEye", new Vector4(0, 0));
        eyeMaterial.SetVector("_RightEye", new Vector4(0, 0));

        eyeMaterial.SetFloat("_Angery", 0.2f);
    }

    /// <summary>
    /// Moves the eyes to the players left.
    /// </summary>
    /// <param name="position"></param>
    public void lookLeft(float position)
    {
        eyeMaterial.SetVector("_RightEye", new Vector4(position, 0.0f));
        eyeMaterial.SetVector("_LeftEye", new Vector4(position, 0.0f));

    }

    /// <summary>
    /// Moves the eyes to the players right.
    /// </summary>
    /// <param name="seconds"></param>
    public void lookRight(float seconds)
    {
        eyeMaterial.SetVector("_RightEye", new Vector4(0.0f, 0.6f));
        eyeMaterial.SetVector("_LeftEye", new Vector4(0.0f, 0.6f));
    }
}
