using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    protected PlayerInput playerInput;

    public delegate void InputEvent(InputAction.CallbackContext ctx);

    /// <summary>
    /// Subscribe to these delegates to listen to input events.
    /// </summary>
    public InputEvent onSelect;
    public InputEvent onCancel;
    public InputEvent onMovePerformed;
    public InputEvent onMoveCanceled;

    /// <summary>
    /// This vector is set to the current move input.
    /// Use this instead of onMovePerformed/Canceled if you just want access to a direction.
    /// </summary>
    public Vector2 moveInput { get; private set; } = Vector2.zero;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        AddListeners();
    }

    void OnDestroy()
    {
        RemoveListeners();
    }

    private void AddListeners()
    {
        playerInput.actions["Select"].performed += ctx => onSelect(ctx);
        playerInput.actions["Cancel"].performed += ctx => onCancel(ctx);
        playerInput.actions["Move"].performed += ctx => onMovePerformed(ctx);
        playerInput.actions["Move"].canceled += ctx => onMoveCanceled(ctx);
        // Update moveInput
        onMovePerformed += MoveInputPerformed;
        onMoveCanceled += MoveInputCanceled;

        AddExtraListeners();
    }

    private void RemoveListeners()
    {
        playerInput.actions["Select"].performed -= ctx => onSelect(ctx);
        playerInput.actions["Cancel"].performed -= ctx => onCancel(ctx);
        playerInput.actions["Move"].performed -= ctx => onMovePerformed(ctx);
        playerInput.actions["Move"].canceled -= ctx => onMoveCanceled(ctx);
        // Update moveInput
        onMovePerformed -= MoveInputPerformed;
        onMoveCanceled -= MoveInputCanceled;

        RemoveExtraListeners();
    }

    /// <summary>
    /// Inherit and override this method to add listeners for other inputs.
    /// </summary>
    protected virtual void AddExtraListeners()
    {

    }

    /// <summary>
    /// Remember to override this as well to remove listeners on destroy.
    /// </summary>
    protected virtual void RemoveExtraListeners()
    {

    }

    private void MoveInputPerformed(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
        Debug.Log(moveInput);
    }

    private void MoveInputCanceled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }
}
