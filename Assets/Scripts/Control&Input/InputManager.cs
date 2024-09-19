using UnityEngine;
using UnityEngine.InputSystem;

public delegate void InputEvent(InputAction.CallbackContext ctx);

internal class ListenerBackup
{
    // General
    public InputEvent onMovePerformed;
    public InputEvent onMoveCanceled;
    // Menu-related
    public InputEvent onSelect;
    public InputEvent onCancel;
    public InputEvent onExit;
    public InputEvent onLeftTab;
    public InputEvent onRightTab;
    public InputEvent onAnyKey;
    // FPS-related
    public InputEvent onInteract;
    public InputEvent onFirePerformed;
    public InputEvent onFireCanceled;
    public InputEvent onCrouchPerformed;
    public InputEvent onCrouchCanceled;
    public InputEvent onZoomPerformed;
    public InputEvent onZoomCanceled;
    public InputEvent onLookPerformed;
    public InputEvent onLookCanceled;
}

public class InputManager : MonoBehaviour
{
    public PlayerInput playerInput;

    /// <summary>
    /// Subscribe to these delegates to listen to input events.
    /// </summary>
    // General
    public InputEvent onMovePerformed;
    public InputEvent onMoveCanceled;
    // Menu-related
    public InputEvent onSelect;
    public InputEvent onCancel;
    public InputEvent onExit;
    public InputEvent onLeftTab;
    public InputEvent onRightTab;
    public InputEvent onAnyKey;
    // FPS-related
    public InputEvent onInteract;
    public InputEvent onFirePerformed;
    public InputEvent onFireCanceled;
    public InputEvent onCrouchPerformed;
    public InputEvent onCrouchCanceled;
    public InputEvent onZoomPerformed;
    public InputEvent onZoomCanceled;
    private InputEvent onLookPerformed;
    private InputEvent onLookCanceled;

    private ListenerBackup backup;

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
    private float mouseLookScale = 0.02f;
    [SerializeField]
    private float gamepadLookScale = 0.75f;
    [SerializeField]
    private Camera playerCamera;
    public Camera PlayerCamera => playerCamera;

    private bool isMouseAndKeyboard = false;
    public bool IsMouseAndKeyboard => isMouseAndKeyboard;

    public bool ZoomActive = false;
    public bool CrouchActive = false;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        DontDestroyOnLoad(gameObject);
        AddListeners();
    }

    void OnDestroy()
    {
        RemoveListeners();
    }

    public void SetMouseAndKeyboard()
    {
        isMouseAndKeyboard = true;
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
        playerInput.actions["Join"].performed += AnyKey;
        playerInput.actions["Select"].performed += Select;
        playerInput.actions["Cancel"].performed += Cancel;
        playerInput.actions["Exit"].performed += Exit;
        playerInput.actions["Move"].performed += Move;
        playerInput.actions["Move"].canceled += Move;
        playerInput.actions["LeftTab"].performed += LeftTab;
        playerInput.actions["RightTab"].performed += RightTab;
        playerInput.actions["Interact"].performed += Interact;
        playerInput.actions["Fire"].performed += Fire;
        playerInput.actions["Fire"].canceled += Fire;
        playerInput.actions["Crouch"].performed += Crouch;
        playerInput.actions["Crouch"].canceled += Crouch;
        playerInput.actions["CrouchToggle"].performed += CrouchToggle;
        playerInput.actions["CrouchToggle"].canceled += CrouchToggle;
        playerInput.actions["Zoom"].performed += Zoom;
        playerInput.actions["Zoom"].canceled += Zoom;
        playerInput.actions["ZoomToggle"].performed += ZoomToggle;
        playerInput.actions["ZoomToggle"].canceled += ZoomToggle;
        playerInput.actions["Look"].performed += Look;
        playerInput.actions["Look"].canceled += Look;

        AddExtraListeners();
    }

    public void RemoveListeners()
    {
        RemoveAllListeners();
        RemoveExtraListeners();
        moveInput = Vector2.zero;
        lookInput = Vector2.zero;
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
        onExit = null;
        onMovePerformed = null;
        onMoveCanceled = null;
        onLeftTab = null;
        onRightTab = null;
        onFirePerformed = null;
        onFireCanceled = null;
        onCrouchPerformed = null;
        onCrouchCanceled = null;
        onZoomPerformed = null;
        onZoomCanceled = null;
        onLookPerformed = null;
        onLookCanceled = null;
        onInteract = null;
    }

    /// <summary>
    /// Store a backup of each delegate's invocation list.
    /// Restore when desired.
    /// </summary>
    public void BackupListeners()
    {
        backup = new()
        {
            onSelect = onSelect,
            onCancel = onCancel,
            onExit = onExit,
            onMovePerformed = onMovePerformed,
            onMoveCanceled = onMoveCanceled,
            onLeftTab = onLeftTab,
            onRightTab = onRightTab,
            onFirePerformed = onFirePerformed,
            onFireCanceled = onFireCanceled,
            onCrouchPerformed = onCrouchPerformed,
            onCrouchCanceled = onCrouchCanceled,
            onZoomPerformed = onZoomPerformed,
            onZoomCanceled = onZoomCanceled,
            onLookPerformed = onLookPerformed,
            onLookCanceled = onLookCanceled,
            onInteract = onInteract,
        };
    }

    /// <summary>
    /// Restore the invocation lists of each delegate.
    /// </summary>
    public void RestoreListeners()
    {
        onSelect = backup.onSelect;
        onCancel = backup.onCancel;
        onExit = backup.onExit;
        onMovePerformed = backup.onMovePerformed;
        onMoveCanceled = backup.onMoveCanceled;
        onLeftTab = backup.onLeftTab;
        onRightTab = backup.onRightTab;
        onFirePerformed = backup.onFirePerformed;
        onFireCanceled = backup.onFireCanceled;
        onCrouchPerformed = backup.onCrouchPerformed;
        onCrouchCanceled = backup.onCrouchCanceled;
        onZoomPerformed = backup.onZoomPerformed;
        onZoomCanceled = backup.onZoomCanceled;
        onLookPerformed = backup.onLookPerformed;
        onLookCanceled = backup.onLookCanceled;
        onInteract = backup.onInteract;
    }

    #region OnEvent Functions
    private void AnyKey(InputAction.CallbackContext ctx)
    {
        onAnyKey?.Invoke(ctx);
    }

    private void Select(InputAction.CallbackContext ctx)
    {
        onSelect?.Invoke(ctx);
    }

    private void Cancel(InputAction.CallbackContext ctx)
    {
        onCancel?.Invoke(ctx);
    }

    private void Exit(InputAction.CallbackContext ctx)
    {
        onExit?.Invoke(ctx);
    }

    private void LeftTab(InputAction.CallbackContext ctx)
    {
        onLeftTab?.Invoke(ctx);
    }

    private void RightTab(InputAction.CallbackContext ctx)
    {
        onRightTab?.Invoke(ctx);
    }

    private void Interact(InputAction.CallbackContext ctx)
    {
        onInteract?.Invoke(ctx);
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
        if (ctx.performed)
        {
            CrouchActive = true;
            onCrouchPerformed?.Invoke(ctx);
            return;
        }
        CrouchActive = false;
        onCrouchCanceled?.Invoke(ctx);
    }

    private void CrouchToggle(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            CrouchActive = !CrouchActive;
            if (!CrouchActive) { onCrouchCanceled?.Invoke(ctx); return; }
            onCrouchPerformed?.Invoke(ctx);
        }
    }

    private void Zoom(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            ZoomActive = true;
            onZoomPerformed?.Invoke(ctx);
            return;
        }
        ZoomActive = false;
        onZoomCanceled?.Invoke(ctx);
        return;
    }

    private void ZoomToggle(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            ZoomActive = !ZoomActive;
            if (!ZoomActive) { onZoomCanceled?.Invoke(ctx); return; }
            onZoomPerformed?.Invoke(ctx);
        }
    }

    private void Look(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) { onLookPerformed?.Invoke(ctx); return; }
        onLookCanceled?.Invoke(ctx);
    }
    #endregion
}
