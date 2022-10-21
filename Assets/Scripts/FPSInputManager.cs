using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FPSInputManager : InputManager
{
    public InputEvent onFire;
    public InputEvent onLookPerformed;
    public InputEvent onLookCanceled;

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
        cleanupCalls.Add(FixListeners("Look", true, onFire));
        cleanupCalls.Add(FixListeners("Look", false, onFire));
    }

    protected override void RemoveExtraListeners()
    {
        // Update lookInput
        onLookPerformed -= LookInputPerformed;
        onLookCanceled -= LookInputCanceled;
    }

    private void LookInputPerformed(InputAction.CallbackContext ctx)
    {
        Debug.Log("Look performed");
        lookInput = ctx.ReadValue<Vector2>();
    }

    private void LookInputCanceled(InputAction.CallbackContext ctx)
    {
        lookInput = Vector2.zero;
    }
}