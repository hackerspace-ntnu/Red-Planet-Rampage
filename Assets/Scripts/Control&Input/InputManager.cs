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
    // General
    public InputEvent onMovePerformed;
    public InputEvent onMoveCanceled;
    // Menu-related
    public InputEvent onSelect;
    public InputEvent onCancel;
    public InputEvent onLeftTab;
    public InputEvent onRightTab;
    // FPS-related
    public InputEvent onFirePerformed;
    public InputEvent onFireCanceled;
    public InputEvent onCrouchPerformed;
    public InputEvent onCrouchCanceled;
    private InputEvent onLookPerformed;
    private InputEvent onLookCanceled;

    /// <summary>
    /// This vector is set to the current move input.
    /// Use this instead of onMovePerformed/Canceled if you just want access to a direction.
    /// </summary>
    public Vector2 moveInput { get; private set; } = Vector2.zero;

    /// <summary>
    /// This vector is set to the current look input.
    /// Use this instead of onLookPerformed/Canceled if you just want access to a direction.
    /// </summary>
    public Vector2 lookInput { get; private set; } = Vector2.zero;

    [SerializeField]
    private float mouseLookScale = 0.1f;
    [SerializeField]
    private float gamepadLookScale = 0.75f;

    private bool isMouseAndKeyboard = false;
    public bool IsMouseAndKeyboard => IsMouseAndKeyboard;

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

    public void AddListeners()
    {
        // Update moveInput
        onMovePerformed += MoveInputPerformed;
        onMoveCanceled += MoveInputCanceled;
        // Update lookInput
        onLookPerformed += LookInputPerformed;
        onLookCanceled += LookInputCanceled;
        // Subscribe delegates to inputs
        playerInput.actions["Select"].performed += Select;
        playerInput.actions["Cancel"].performed += Cancel;
        playerInput.actions["Move"].performed += Move;
        playerInput.actions["Move"].canceled += Move;
        playerInput.actions["LeftTab"].performed += LeftTab;
        playerInput.actions["RightTab"].performed += RightTab;
        playerInput.actions["Fire"].performed += Fire;
        playerInput.actions["Fire"].canceled += Fire;
        playerInput.actions["Crouch"].performed += Crouch;
        playerInput.actions["Crouch"].canceled += Crouch;
        playerInput.actions["Look"].performed += Look;
        playerInput.actions["Look"].canceled += Look;

        // Imprison mouse
        if (playerInput.currentControlScheme == "MouseAndKeyboard")
        {
            isMouseAndKeyboard = true;
            Cursor.lockState = CursorLockMode.Locked;
        }

        AddExtraListeners();
    }

    public void RemoveListeners()
    {
        RemoveAllListeners();
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

    private void LookInputPerformed(InputAction.CallbackContext ctx)
    {
        if (isMouseAndKeyboard)
        {
            lookInput = ctx.ReadValue<Vector2>() * mouseLookScale;
        }
        else
        {
            lookInput = ctx.ReadValue<Vector2>() * gamepadLookScale;
        }
    }

    private void LookInputCanceled(InputAction.CallbackContext ctx)
    {
        lookInput = Vector2.zero;
    }

    private void RemoveAllListeners()
    {
        // Abusing that empty delegate bodies are defined as null to remove all invocation lists.
        onSelect = null;
        onCancel = null;
        onMovePerformed = null;
        onMoveCanceled = null;
        onLeftTab = null;
        onRightTab = null;
        onFirePerformed = null;
        onFireCanceled = null;
        onCrouchPerformed = null;
        onCrouchCanceled = null;
        onLookPerformed = null;
        onLookCanceled = null;

        // Free the mouse
        if (isMouseAndKeyboard)
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    #region OnEvent Functions
    private void Select(InputAction.CallbackContext ctx)
    {
        onSelect?.Invoke(ctx);
    }

    private void Cancel(InputAction.CallbackContext ctx)
    {
        onCancel?.Invoke(ctx);
    }

    private void LeftTab(InputAction.CallbackContext ctx)
    {
        onLeftTab?.Invoke(ctx);
    }

    private void RightTab(InputAction.CallbackContext ctx)
    {
        onRightTab?.Invoke(ctx);
    }

    private void Move(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) { onMovePerformed?.Invoke(ctx); return; }
        onMoveCanceled?.Invoke(ctx);
    }

    private void Fire(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) { onFirePerformed?.Invoke(ctx); return; }
        onFireCanceled?.Invoke(ctx);
    }

    private void Crouch(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) { onCrouchPerformed?.Invoke(ctx); return; }
        onCrouchCanceled?.Invoke(ctx);
    }

    private void Look(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) { onLookPerformed?.Invoke(ctx); return; }
        onLookCanceled?.Invoke(ctx);
    }
    #endregion
}
