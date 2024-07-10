using System.Collections;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using VectorExtensions;

internal enum GroundState
{
    InAir,
    Grounded,
}

internal enum JumpState
{
    Normal,
    Leap,
    LeapHop
}

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PlayerMovement : MonoBehaviour
{
    private InputManager inputManager;
    protected Rigidbody body;
    public Rigidbody Body => body;
    protected Collider hitbox;
    private Camera playerCamera;

    [FormerlySerializedAs("ignoreMask")]
    [SerializeField]
    protected LayerMask groundCheckMask;

    [SerializeField]
    private float lookSpeed = 3;

    [SerializeField]
    public float LookSpeedZoom = 0.75f;

    private float sensScale;

    [SerializeField]
    [Tooltip("Reduction in look speed for mice when zoomed")]
    private float mouseZoomSpeedFactor = 0.1f;

    [Header("Drag")]
    [SerializeField]
    protected float groundDrag = 6f;

    [SerializeField]
    protected float airDrag = 2f;

    [SerializeField]
    protected float maxVelocityBeforeExtraDamping = 20f;

    [SerializeField]
    protected float extraDamping = 25f;

    protected float dragForce = 0;

    [Header("Strafe")]
    [SerializeField]
    protected float strafeForceGrounded = 60;

    [SerializeField]
    protected float strafeForceCrouched = 30;

    [SerializeField]
    protected float strafeForceInAir = 16;

    protected float strafeForce;

    [Header("Jumping")]
    [SerializeField]
    protected float jumpForce = 10;

    [SerializeField]
    protected float leapForce = 12.5f;

    [SerializeField]
    protected float leapTimeout = 0.25f;

    [SerializeField]
    protected float dashHeightMultiplier = 0.5f;

    [SerializeField]
    protected float dashForwardMultiplier = 1.5f;

    [SerializeField]
    protected float dashDamping = 4f;

    [SerializeField]
    protected float minDashVelocity = 8f;

    [Header("State")]
    [SerializeField]
    private float crouchedHeightOffset = 0.3f;
    [SerializeField]
    private float crouchedHeightGunOffset = 0.28f;

    [SerializeField]
    private float airThreshold = 0.17f;

    [SerializeField]
    protected float slopeAngleThreshold = 50;

    [SerializeField, ReadOnly]
    private GroundState state = GroundState.Grounded;
    public bool StateIsAir => state == GroundState.InAir;

    [SerializeField, ReadOnly]
    private JumpState jumpState = JumpState.Normal;

    private bool isCrouching = false;

    [SerializeField]
    protected Animator animator;
    public Animator Animator => animator;

    private GameObject gunHolder;

    private const float MarsGravity = 3.7f;

    private float localCameraHeight;
    private float localGunHolderHeight;
    private float localGunHolderX;

    [SerializeField]
    public float ZoomFov = 30f;
    private float startingFov;

    private Vector2 aimAngle = Vector2.zero;
    public Vector2 AimAngle => aimAngle;

    public delegate void MovementEventBody(Rigidbody body);
    public MovementEventBody OnMove;
    public MovementEventBody OnJumpPerformed;
    public MovementEventBody OnLeapPerformed;

    private int gunCrouchPerformedTween;
    private int cameraCrouchPerformedTween;
    private int gunCrouchCanceledTween;
    private int cameraCrouchCanceledTween;

    public bool CanMove = true;
    public bool CanLook = true;

    [Header("Step climb")]
    [SerializeField]
    private GameObject bottomCaster;
    [SerializeField]
    private GameObject topCaster;
    [SerializeField]
    private float stepHeight = 0.5f;
    [SerializeField]
    private Transform playerRoot;
    [SerializeField]
    protected LayerMask steppingIgnoreMask;


    protected virtual void Start()
    {
        body = GetComponent<Rigidbody>();
        hitbox = GetComponent<BoxCollider>();
        strafeForce = strafeForceGrounded;
        topCaster.transform.localPosition = new Vector3(0f, stepHeight, 0f);
    }

    /// <summary>
    /// Function for setting a playerInput and adding movement related listeners to it.
    /// </summary>
    /// <param name="player"></param>
    public void SetPlayerInput(InputManager player)
    {
        ReassignPlayerInput(player);
        var playerManager = GetComponent<PlayerManager>();
        gunHolder = playerManager.GunHolder.gameObject;
        localCameraHeight = inputManager.transform.localPosition.y;
        localGunHolderX = gunHolder.transform.localPosition.x;
        localGunHolderHeight = gunHolder.transform.localPosition.y;
        playerCamera = inputManager.PlayerCamera;
        SetFOVFromSettings();
        sensScale = SettingsInfo.Singleton.settings.sensScale;

        if (MatchController.Singleton)
            MatchController.Singleton.onRoundEnd += ResetZoom;
    }

    public void ReassignPlayerInput(InputManager input)
    {
        inputManager = input;
        inputManager.onSelect += OnJump;
        inputManager.onCrouchPerformed += OnCrouch;
        inputManager.onCrouchCanceled += OnCrouch;
        inputManager.onZoomPerformed += OnZoom;
        inputManager.onZoomCanceled += OnZoomCanceled;
    }

    public void SetFOVFromSettings()
    {
        if (playerCamera != null)
        {
            startingFov = SettingsInfo.Singleton.settings.playerFOV;
            playerCamera.fieldOfView = SettingsInfo.Singleton.settings.playerFOV;
            ZoomFov = SettingsInfo.Singleton.settings.zoomFOV;
        }
    }

    public void SetInitialRotation(float radians)
    {
        aimAngle = new Vector2(radians, aimAngle.y);
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (state is not GroundState.Grounded)
            return;

        if (isCrouching)
        {
            jumpState = jumpState switch
            {
                JumpState.Normal => JumpState.Leap,
                JumpState.Leap => JumpState.LeapHop,
                JumpState.LeapHop => JumpState.LeapHop,
                _ => JumpState.Leap
            };
        }
        else
        {
            jumpState = JumpState.Normal;
        }

        var forwardDirection = new Vector3(inputManager.transform.forward.x, 0, inputManager.transform.forward.z);
        switch (jumpState)
        {
            case JumpState.Leap:
                {
                    body.AddForce(Vector3.up * leapForce, ForceMode.VelocityChange);
                    body.AddForce(forwardDirection * leapForce, ForceMode.VelocityChange);
                    StartCoroutine(LeapTimeout());
                    animator.SetTrigger("Leap");
                    OnLeapPerformed?.Invoke(body);
                    break;
                }
            case JumpState.LeapHop:
                {
                    body.AddForce(Vector3.up * leapForce * dashHeightMultiplier, ForceMode.VelocityChange);
                    body.AddForce(forwardDirection * leapForce * dashForwardMultiplier, ForceMode.VelocityChange);
                    StartCoroutine(LeapTimeout());
                    animator.SetTrigger("Leap");
                    OnLeapPerformed?.Invoke(body);
                    break;
                }
            case JumpState.Normal:
            default:
                {
                    body.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
                    break;
                }
        }
        OnJumpPerformed?.Invoke(body);
    }

    private void OnZoom(InputAction.CallbackContext ctx)
    {
        LeanTween.value(gameObject, (fov) => playerCamera.fieldOfView = fov, playerCamera.fieldOfView, ZoomFov, 0.2f).setEaseInOutCubic();
        gunHolder.LeanMoveLocalX(0f, 0.2f);
    }

    private void OnZoomCanceled(InputAction.CallbackContext ctx)
    {
        CancelZoom();
    }

    private void CancelZoom()
    {
        LeanTween.value(gameObject, (fov) => playerCamera.fieldOfView = fov, playerCamera.fieldOfView, startingFov, 0.2f).setEaseInOutCubic();
        gunHolder.LeanMoveLocalX(localGunHolderX, 0.2f);
    }

    public void ResetZoom()
    {
        inputManager.ZoomActive = false;
        inputManager.onZoomPerformed -= OnZoom;
        inputManager.onZoomCanceled -= OnZoomCanceled;
        CancelZoom();
    }

    private void OnCrouch(InputAction.CallbackContext ctx)
    {
        if (LeanTween.isTweening(cameraCrouchCanceledTween))
        {
            LeanTween.cancel(cameraCrouchCanceledTween);
            LeanTween.cancel(gunCrouchCanceledTween);
            inputManager.transform.localPosition = new Vector3(inputManager.transform.localPosition.x, localCameraHeight, inputManager.transform.localPosition.z);
            gunHolder.transform.localPosition = new Vector3(gunHolder.transform.localPosition.x, localGunHolderHeight, gunHolder.transform.localPosition.z);
        }
        if (LeanTween.isTweening(cameraCrouchPerformedTween))
        {
            LeanTween.cancel(cameraCrouchPerformedTween);
            LeanTween.cancel(gunCrouchPerformedTween);
            inputManager.transform.localPosition = new Vector3(inputManager.transform.localPosition.x, localCameraHeight, inputManager.transform.localPosition.z);
            gunHolder.transform.localPosition = new Vector3(gunHolder.transform.localPosition.x, localGunHolderHeight, gunHolder.transform.localPosition.z);
        }

        if (inputManager.CrouchActive)
            StartCrouching();
        else
            StopCrouching();
    }

    private void StartCrouching()
    {
        isCrouching = true;
        if (state is GroundState.InAir)
            return;
        animator.SetBool("Crouching", true);
        strafeForce = strafeForceCrouched;
        cameraCrouchPerformedTween = inputManager.gameObject.LeanMoveLocalY(localCameraHeight - crouchedHeightOffset, 0.2f).id;
        gunCrouchCanceledTween = gunHolder.LeanMoveLocalY(localGunHolderHeight - crouchedHeightGunOffset, 0.2f).id;
    }

    private void StopCrouching()
    {
        isCrouching = false;
        jumpState = JumpState.Normal;
        animator.SetBool("Crouching", false);
        strafeForce = strafeForceGrounded;
        cameraCrouchCanceledTween = inputManager.gameObject.LeanMoveLocalY(localCameraHeight, 0.2f).id;
        gunCrouchCanceledTween = gunHolder.LeanMoveLocalY(localGunHolderHeight, 0.2f).id;
    }

    /// <summary>
    /// Abort leap(dash)ing if the player becomes grounded again after a short while.
    /// This prevents players from abusing this mechanic to boost themselves up ramps.
    /// </summary>
    private IEnumerator LeapTimeout()
    {
        yield return new WaitForSeconds(leapTimeout);
        if (state is not GroundState.InAir)
            jumpState = JumpState.Normal;
    }

    /// <summary>
    /// Cast a box to detect (partial) ground. See OnDrawGizmos for what I think is the extent of the box cast.
    /// No, this does not work if the cast start at the bottom.
    /// </summary>
    /// <returns>Whether or not the player is in the air</returns>
    private bool IsInAir()
    {
        return !Physics.BoxCast(hitbox.bounds.center, new Vector3(.5f, .5f, .2f), Vector3.down, Quaternion.identity, 0.5f + airThreshold, groundCheckMask); ;
    }


    /// <summary>
    /// Cast a box to detect (partial) ground. See OnDrawGizmos for what I think is the extent of the box cast.
    /// No, this does not work if the cast starts at the bottom.
    /// </summary>
    /// <returns>The normal vector of the ground</returns>
    private Vector3 GroundNormal()
    {
        if (Physics.BoxCast(hitbox.bounds.center, 0.5f * Vector3.one, Vector3.down, out var hit, Quaternion.identity,
                0.5f + airThreshold, groundCheckMask))
        {
            var angle = Vector3.Angle(hit.normal, Vector3.up);
            var isAngleWithinThreshold = angle < slopeAngleThreshold && angle > 0;
            return isAngleWithinThreshold ? hit.normal : Vector3.up;
        }
        return Vector3.up;
    }

    protected void UpdatePosition(Vector3 input)
    {
        if (!CanMove)
            input = Vector3.zero;
        // Modify input to addforce with relation to current rotation.
        input = transform.forward * input.z + transform.right * input.x;

        // Handle state changes
        var isInAir = IsInAir();
        switch (state)
        {
            case GroundState.InAir:
                {
                    if (!isInAir)
                    {
                        state = GroundState.Grounded;
                        // Handle held crouch vs previous jump state
                        if (isCrouching)
                            StartCrouching();
                        else
                            jumpState = JumpState.Normal;
                    }
                    break;
                }
            case GroundState.Grounded:
            default:
                {
                    if (isInAir)
                    {
                        state = GroundState.InAir;
                    }
                    break;
                }
        }

        // Apply movement input
        switch (state)
        {
            case GroundState.InAir:
                {
                    dragForce = airDrag;
                    body.AddForce(input * (strafeForceInAir * Time.deltaTime), ForceMode.VelocityChange);
                    body.AddForce(Vector3.down * (MarsGravity * Time.deltaTime), ForceMode.Acceleration);
                    break;
                }
            case GroundState.Grounded:
            default:
                {
                    dragForce = groundDrag;
                    // Walk along ground normal (adjusted if on heavy slope).
                    var groundNormal = GroundNormal();
                    var direction = Vector3.ProjectOnPlane(input, groundNormal);
                    body.AddForce(direction * (strafeForce * Time.deltaTime), ForceMode.VelocityChange);
                    // Add an extra punch on ground strafing, to prevent excessive acceleration time.
                    body.AddForce(direction * (strafeForce * Time.deltaTime), ForceMode.Impulse);
                    break;
                }
        }

        var yDrag = body.velocity.y < 0 ? 0f : body.velocity.y;
        body.AddForce(-dragForce * body.mass * new Vector3(body.velocity.x, yDrag, body.velocity.z), ForceMode.Force);
    }

    private void UpdateRotation()
    {
        if (!CanLook)
            return;
        var lookSpeedFactor = inputManager.ZoomActive
            ? inputManager.IsMouseAndKeyboard ? LookSpeedZoom * mouseZoomSpeedFactor: LookSpeedZoom
            : lookSpeed;
        var lookInput = inputManager.IsMouseAndKeyboard
            ? inputManager.lookInput
            : inputManager.lookInput * Time.deltaTime;
        aimAngle += lookInput * lookSpeedFactor * sensScale;
        aimAngle = aimAngle.ClampedLookAngles();
        // Rotate rigidbody.
        body.MoveRotation(Quaternion.AngleAxis(aimAngle.x * Mathf.Rad2Deg, Vector3.up));
        // Rotate look separately. Camera is attached to FPSInputManager.
        inputManager.transform.localRotation = Quaternion.AngleAxis(aimAngle.y * Mathf.Rad2Deg, Vector3.left);
    }

    protected void UpdateAnimatorParameters()
    {
        animator.SetFloat("Forward", Vector3.Dot(body.velocity, transform.forward) / maxVelocityBeforeExtraDamping);
        animator.SetFloat("Right", Vector3.Dot(body.velocity, transform.right) / maxVelocityBeforeExtraDamping);
    }

    private void OnDrawGizmos()
    {
        if (!hitbox) return;
        var extents = new Vector3(1, 1.5f + airThreshold, 1);
        var center = hitbox.bounds.center + (0.25f + 0.5f * airThreshold) * Vector3.down;
        Gizmos.DrawWireCube(center, extents);
    }

    /// <summary>
    /// More accurate ground detection for use in stepping up edges. Uses raycast to check for ground directly beneath the player.
    /// </summary>
    /// <returns>true if player is touching the ground, false if not touching ground </returns>
    private bool FindSteppingGround()
    {
        RaycastHit hitGround;
        if (Physics.Raycast(playerRoot.position, Vector3.down, out hitGround, 0.01f))
        {
            if (hitGround.normal.y > 0.0001f && state == GroundState.Grounded)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Check for an edge to step up using a short raycast, also checks for a ledge if the short raycast doesn't hit anything.
    /// </summary>
    private void FindStep()
    {
        Vector3 rayDirection = new Vector3(body.velocity.x, 0, body.velocity.z);
        if (Physics.Raycast(bottomCaster.transform.position, rayDirection, out var hitBottom, 0.5f, steppingIgnoreMask))
        {
            OnStepDetected(hitBottom, rayDirection);
            return;
        }
        if (Physics.Raycast(bottomCaster.transform.position + rayDirection.normalized * 0.5f, Vector3.up, out var hitBottomSurface, stepHeight, steppingIgnoreMask))
        {
            OnStepDetected(hitBottomSurface, rayDirection);
        }
    }

    /// <summary>
    /// Called when a edge or ledge is detected, checks for free area on top of edge and moves the player ontop if there is space.
    /// </summary>
    /// <param name="raycastHit"></param>
    /// <param name="rayDirection"></param>
    private void OnStepDetected(RaycastHit raycastHit, Vector3 rayDirection)
    {
        if (!Physics.Raycast(topCaster.transform.position, rayDirection, 1f, steppingIgnoreMask))
        {
            if (raycastHit.normal.y < 0.0001f && raycastHit.collider.name != "Terrain")
            {
                Physics.Raycast(topCaster.transform.position + rayDirection.normalized * 0.5f, Vector3.down, out var hitTopSurface, stepHeight, steppingIgnoreMask);
                body.position += new Vector3(0f, stepHeight - hitTopSurface.distance + 0.05f, 0f) + body.velocity.normalized * 0.4f;
            }
        }
    }

    protected virtual void FixedUpdate()
    {
        if (FindSteppingGround() && body.velocity.magnitude > 0.08f)
        {
            FindStep();
        }

        var positionInput = new Vector3(inputManager.moveInput.x, 0, inputManager.moveInput.y);
        UpdatePosition(positionInput);

        // Limit velocity when not grounded
        if (state == GroundState.Grounded)
            return;

        if (jumpState == JumpState.LeapHop)
        {
            var directionalForces = new Vector3(body.velocity.x, 0, body.velocity.z);
            if (directionalForces.magnitude < minDashVelocity)
                jumpState = JumpState.Normal;
        }

        // Add extra drag when player velocity is too high
        var maxVelocityReached = Mathf.Abs(body.velocity.x) > maxVelocityBeforeExtraDamping || Mathf.Abs(body.velocity.z) > maxVelocityBeforeExtraDamping;
        if (maxVelocityReached)
        {
            var dampingFactor = jumpState == JumpState.LeapHop ? dashDamping : extraDamping;
            body.AddForce(-dampingFactor * body.mass * new Vector3(body.velocity.x, 0, body.velocity.z), ForceMode.Force);
        }
    }

    private void Update()
    {
        UpdateRotation();
        UpdateAnimatorParameters();
        OnMove?.Invoke(body);
    }

    private void OnDestroy()
    {
        if (!inputManager)
            return;
        ResetZoom();
        inputManager.onSelect -= OnJump;
        inputManager.onCrouchPerformed -= OnCrouch;
        inputManager.onCrouchCanceled -= OnCrouch;
        inputManager.onZoomPerformed -= OnZoom;
        inputManager.onZoomCanceled -= OnZoomCanceled;
        var playerManager = GetComponent<PlayerManager>();
        if (playerManager?.GunController)
        {
            inputManager.onZoomPerformed -= playerManager.GunController.OnZoom;
            inputManager.onZoomCanceled -= playerManager.GunController.OnZoomCanceled;
        }

        if (MatchController.Singleton)
            MatchController.Singleton.onRoundEnd -= ResetZoom;
    }
}
