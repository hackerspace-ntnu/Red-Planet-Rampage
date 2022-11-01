using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FPSInputManager : InputManager
{
    public InputEvent onFire;
    private InputEvent onLookPerformed;
    private InputEvent onLookCanceled;

    [SerializeField]
    private float mouseLookScale = 0.0001f;

    private bool isMouseAndKeyboard = false;

    /// <summary>
    /// This vector is set to the current look input.
    /// Use this instead of onLookPerformed/Canceled if you just want access to a direction.
    /// </summary>
    public Vector2 lookInput { get; private set; } = Vector2.zero;

    protected override void AddExtraListeners()
    {
        // Update lookInput
        onLookPerformed += LookInputPerformed;
        onLookCanceled += LookInputCanceled;

        cleanupCalls.Add(FixListeners("Fire", true, onFire));
        cleanupCalls.Add(FixListeners("Look", true, onLookPerformed));
        cleanupCalls.Add(FixListeners("Look", false, onLookCanceled));

        // Imprison mouse
        if (playerInput.currentControlScheme == "MouseAndKeyboard")
        {
            isMouseAndKeyboard = true;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    protected override void RemoveExtraListeners()
    {
        // Update lookInput
        onLookPerformed -= LookInputPerformed;
        onLookCanceled -= LookInputCanceled;

        // Free the mouse
        if (isMouseAndKeyboard)
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void LookInputPerformed(InputAction.CallbackContext ctx)
    {
        if (isMouseAndKeyboard)
        {
            lookInput = ctx.ReadValue<Vector2>() * mouseLookScale;
        }
        else
        {
            lookInput = ctx.ReadValue<Vector2>();
        }
    }

    private void LookInputCanceled(InputAction.CallbackContext ctx)
    {
        lookInput = Vector2.zero;
    }
}