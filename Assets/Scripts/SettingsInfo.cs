using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsInfo : MonoBehaviour
{
    public static SettingsInfo Singleton { get; private set; }
    
    public float sensMultiplier { get; set; }

    private PlayerInputManagerController playerInputManagerController;

    void Start()
    {
        #region Singleton boilerplate

        if (Singleton != null)
        {
            if (Singleton != this)
            {
                Debug.LogWarning($"There's more than one {Singleton.GetType()} in the scene!");
                Destroy(gameObject);
            }

            return;
        }

        Singleton = this;

        #endregion Singleton boilerplate

        playerInputManagerController = PlayerInputManagerController.Singleton;
    }

    public void SetChangesToLocalAllInputs()
    {
        foreach (InputManager playerInput in playerInputManagerController.LocalPlayerInputs)
        {
            playerInput.adjustScaleMulti = sensMultiplier;
        }
    }

    public void SetChangesToInput(InputManager playerInput)
    {
        playerInput.adjustScaleMulti = sensMultiplier;
    }
}
