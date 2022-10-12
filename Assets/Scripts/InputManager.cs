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

    // Start is called before the first frame update
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        AddListeners();
        AddExtraListeners();
    }

    void OnDestroy()
    {
        RemoveListeners();
        RemoveExtraListeners();
    }

    private void AddListeners()
    {
        playerInput.actions["Select"].performed += onSelect.Invoke;
        playerInput.actions["Cancel"].performed += onCancel.Invoke;
        playerInput.actions["Move"].performed += onMovePerformed.Invoke;
        playerInput.actions["Move"].canceled += onMoveCanceled.Invoke;
        // Update moveInput
        onMovePerformed += MoveInputPerformed;
        onMoveCanceled += MoveInputCanceled;
    }

    private void RemoveListeners()
    {
        playerInput.actions["Select"].performed -= onSelect.Invoke;
        playerInput.actions["Cancel"].performed -= onCancel.Invoke;
        playerInput.actions["Move"].performed -= onMovePerformed.Invoke;
        playerInput.actions["Move"].canceled -= onMoveCanceled.Invoke;
        onMovePerformed -= MoveInputPerformed;
        onMoveCanceled -= MoveInputCanceled;
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
    }

    private void MoveInputCanceled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }
}
