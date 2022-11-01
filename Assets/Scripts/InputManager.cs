using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public PlayerInput playerInput;

    public delegate void InputEvent(InputAction.CallbackContext ctx);

    /// <summary>
    /// Subscribe to these delegates to listen to input events.
    /// </summary>
    public InputEvent onSelect;
    public InputEvent onCancel;
    public InputEvent onMovePerformed;
    public InputEvent onMoveCanceled;

    protected List<Action> cleanupCalls = new List<Action>();

    /// <summary>
    /// This vector is set to the current move input.
    /// Use this instead of onMovePerformed/Canceled if you just want access to a direction.
    /// </summary>
    public Vector2 moveInput { get; private set; } = Vector2.zero;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        AddListeners();
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        RemoveListeners();
    }

    /// <summary>
    /// A helper function for subscribing a "InputEvent" delegate to a specific InputAction from an ActionMap
    /// 
    /// The delegate will be invoked by an anonymous internal function whenever specified inputAction is triggered.
    /// Warning! InputEvent delegate will not be invoked if it has an empty body, so minimum 1 function has to be assigned before this function is called.
    /// </summary>
    /// <param name="inputAction">The name of an inputAction you want the delegate to be invoked by</param>
    /// <param name="performed">Is the InputAction you want to add delegate to invoked when action is performed or canceled?</param>
    /// <param name="inputEvent">The inputEvent delegate you want to be invoked</param>
    /// <returns>Function to remove added listener, typically called within OnDestroy</returns>
    protected Action FixListeners(string inputAction, bool performed, InputEvent inputEvent)
    {
        Action<InputAction.CallbackContext> function = ctx => inputEvent?.Invoke(ctx);
        if (performed)
        {
            playerInput.actions[inputAction].performed += function;
            return () => playerInput.actions[inputAction].performed -= function;
        }
        else
        {
            playerInput.actions[inputAction].canceled += function;
            return () => playerInput.actions[inputAction].canceled -= function;
        }
    }

    public void AddListeners()
    {
        // Update moveInput
        onMovePerformed += MoveInputPerformed;
        onMoveCanceled += MoveInputCanceled;

        cleanupCalls.Add(FixListeners("Select", true, onSelect));
        cleanupCalls.Add(FixListeners("Cancel", true, onCancel));
        cleanupCalls.Add(FixListeners("Move", true, onMovePerformed));
        cleanupCalls.Add(FixListeners("Move", false, onMoveCanceled));

        AddExtraListeners();
    }

    public void RemoveListeners()
    {
        cleanupCalls.ForEach(callback => callback());
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
    }

    private void MoveInputCanceled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }

}
