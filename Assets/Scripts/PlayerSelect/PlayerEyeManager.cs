using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerEyeManager : MonoBehaviour
{

    private Material eyeMaterial;

    public void Start()
    {
        // Find eye material
        SkinnedMeshRenderer eyeRenderer = transform.GetChild(0).GetComponent<SkinnedMeshRenderer>();
        eyeMaterial = eyeRenderer.materials[1];
        resetEyePosition();
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
    /// <param name="targetPosition"></param>
    public void lookLeft(float targetPosition)
    {
        eyeMaterial.SetVector("_RightEye", new Vector4(targetPosition, 0.0f));
        eyeMaterial.SetVector("_LeftEye", new Vector4(targetPosition, 0.0f));

    }

    /// <summary>
    /// Moves the eyes to the players right.
    /// </summary>
    /// <param name="targetPosition"></param>
    public void lookRight(float targetPosition)
    {
        eyeMaterial.SetVector("_RightEye", new Vector4(-targetPosition, 0.0f));
        eyeMaterial.SetVector("_LeftEye", new Vector4(-targetPosition, 0.0f));
    }

    /// <summary>
    /// Sets the tilt of players eyelid. Positive value = anger, negative value = sad times.
    /// </summary>
    /// <param name="angery"></param>
    public void setAngery(float angery)
    {
        eyeMaterial.SetFloat("_Angery", angery);
    }
}
