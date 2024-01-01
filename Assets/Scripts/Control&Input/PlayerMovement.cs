using System.Collections;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

enum PlayerState
{
    IN_AIR,
    GROUNDED,
    DEAD
}

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PlayerMovement : MonoBehaviour
{
    private InputManager inputManager;
    private Rigidbody body;
    private Collider hitbox;
    private Camera playerCamera;

    [SerializeField]
    private LayerMask ignoreMask;

    [SerializeField]
    private float lookSpeed = 3;

    [SerializeField]
    public float LookSpeedZoom = 0.75f;

    [Header("Drag")]
    [SerializeField]
    private float groundDrag = 6f;

    [SerializeField]
    private float airDrag = 2f;

    [SerializeField]
    private float maxVelocityBeforeExtraDamping = 20f;

    [SerializeField]
    private float extraDamping = 25f;

    private float dragForce = 0;

    [Header("Strafe")]
    [SerializeField]
    private float strafeForceGrounded = 60;

    [SerializeField]
    private float strafeForceCrouched = 30;

    [SerializeField]
    private float strafeForceInAir = 16;

    private float strafeForce;

    [Header("Jumping")]
    [SerializeField]
    private float jumpForce = 10;

    [SerializeField]
    private float leapForce = 12.5f;

    [SerializeField]
    private float leapTimeout = 0.25f;

    [SerializeField]
    private float dashHeightMultiplier = 0.5f;

    [SerializeField]
    private float dashForwardMultiplier = 1.5f;

    [SerializeField]
    private float dashDamping = 4f;

    [SerializeField]
    private float minDashVelocity = 8f;

    private bool isDashing = false;

    [Header("State")]
    [SerializeField]
    private float crouchedHeightOffset = 0.3f;
    [SerializeField]
    private float crouchedHeightGunOffset = 0.28f;

    [SerializeField]
    private float airThreshold = 0.4f;

    [SerializeField]
    private float slopeAngleThreshold = 50;

    [SerializeField, ReadOnly]
    private PlayerState state = PlayerState.GROUNDED;

    [SerializeField]
    private Animator animator;

    private GameObject gunHolder;

    private const float MarsGravity = 3.7f;

    private float localCameraHeight;
    private float localGunHolderHeight;
    private float localGunHolderX;

    [SerializeField]
    public float ZoomFov = 30f;
    private float startingFov;

    private Vector2 aimAngle = Vector2.zero;

    private delegate void MovementEvent();
    private MovementEvent onLanding;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        hitbox = GetComponent<BoxCollider>();
        strafeForce = strafeForceGrounded;
    }

    /// <summary>
    /// Function for setting a playerInput and adding movement related listeners to it.
    /// </summary>
    /// <param name="player"></param>
    public void SetPlayerInput(InputManager player)
    {
        inputManager = player;
        var playerManager = GetComponent<PlayerManager>();
        gunHolder = playerManager.GunHolder.gameObject;
        inputManager.onSelect += OnJump;
        inputManager.onCrouchPerformed += OnCrouch;
        inputManager.onCrouchCanceled += OnCrouch;
        inputManager.onZoomPerformed += OnZoom;
        inputManager.onZoomCanceled += OnZoomCanceled;
        localCameraHeight = inputManager.transform.localPosition.y;
        localGunHolderX = gunHolder.transform.localPosition.x;
        localGunHolderHeight = gunHolder.transform.localPosition.y;
        playerCamera = inputManager.GetComponent<Camera>();
        startingFov = playerCamera.fieldOfView;

        if (MatchController.Singleton)
            MatchController.Singleton.onRoundEnd += ResetZoom;
    }

    public void SetInitialRotation(float radians)
    {
        aimAngle = new Vector2(radians, aimAngle.y);
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (!(state == PlayerState.GROUNDED))
            return;

        // Leap/dash jump
        if (animator.GetBool("Crouching"))
        {
            body.AddForce(Vector3.up * leapForce * (isDashing ? dashHeightMultiplier : 1f), ForceMode.VelocityChange);
            Vector3 forwardDirection = new Vector3(inputManager.transform.forward.x, 0, inputManager.transform.forward.z);
            body.AddForce(forwardDirection * leapForce * (isDashing ? dashForwardMultiplier : 1f), ForceMode.VelocityChange);
            animator.SetTrigger("Leap");
            onLanding += EnableDash;
            return;
        }
        // Normal jump
        body.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
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

    private void ResetZoom()
    {
        inputManager.ZoomActive = false;
        inputManager.onZoomPerformed -= OnZoom;
        inputManager.onZoomCanceled -= OnZoomCanceled;
        CancelZoom();
    }

    private void OnCrouch(InputAction.CallbackContext ctx)
    {
        if (LeanTween.isTweening(inputManager.gameObject))
        {
            LeanTween.cancel(inputManager.gameObject);
            inputManager.transform.localPosition = new Vector3(inputManager.transform.localPosition.x, localCameraHeight, inputManager.transform.localPosition.z);
            gunHolder.transform.localPosition = new Vector3(gunHolder.transform.localPosition.x, localGunHolderHeight, gunHolder.transform.localPosition.z);
        }

        if (ctx.performed)
        {
            if (IsInAir())
            {
                onLanding += StartCrouch;
                return;
            }
            StartCrouch();
        }

        if (ctx.canceled)
        {
            animator.SetBool("Crouching", false);
            strafeForce = strafeForceGrounded;
            inputManager.gameObject.LeanMoveLocalY(localCameraHeight, 0.2f);
            gunHolder.LeanMoveLocalY(localGunHolderHeight, 0.2f);
            isDashing = false;
            onLanding -= StartCrouch;
        }

    }

    private void StartCrouch()
    {
        animator.SetBool("Crouching", true);
        strafeForce = strafeForceCrouched;
        inputManager.gameObject.LeanMoveLocalY(localCameraHeight - crouchedHeightOffset, 0.2f);
        gunHolder.LeanMoveLocalY(localGunHolderHeight - crouchedHeightGunOffset, 0.2f);
    }

    private void EnableDash()
    {
        StartCoroutine(JumpTimeout(leapTimeout));
    }

    private IEnumerator JumpTimeout(float time)
    {
        yield return new WaitForSeconds(time);
        isDashing = IsInAir();
        onLanding -= EnableDash;
    }

    private bool IsInAir()
    {
        // Cast a box to detect (partial) ground. See OnDrawGizmos for what I think is the extent of the box cast.
        // No, this does not work if the cast start at the bottom.
        return !Physics.BoxCast(hitbox.bounds.center, 0.5f * Vector3.one, Vector3.down, Quaternion.identity, 0.5f + airThreshold, ignoreMask); ;
    }

    private Vector3 GroundNormal()
    {
        // Cast a box to detect (partial) ground. See OnDrawGizmos for what I think is the extent of the box cast.
        // No, this does not work if the cast start at the bottom.
        if (Physics.BoxCast(hitbox.bounds.center, 0.5f * Vector3.one, Vector3.down, out var hit, Quaternion.identity,
                0.5f + airThreshold, ignoreMask))
        {
            var angle = Vector3.Angle(hit.normal, Vector3.up);
            var isAngleWithinThreshold = angle < slopeAngleThreshold && angle > 0;
            return isAngleWithinThreshold ? hit.normal : Vector3.up;
        }
        return Vector3.up;
    }

    private void UpdatePosition(Vector3 input)
    {
        // Modify input to addforce with relation to current rotation.
        input = transform.forward * input.z + transform.right * input.x;
        switch (state)
        {
            case PlayerState.IN_AIR:
                {
                    dragForce = airDrag;
                    body.AddForce(input * strafeForceInAir * Time.deltaTime, ForceMode.VelocityChange);
                    body.AddForce(Vector3.down * MarsGravity * Time.deltaTime, ForceMode.Acceleration);
                    if (IsInAir())
                        break;
                    state = PlayerState.GROUNDED;
                    onLanding?.Invoke();
                    break;
                }
            case PlayerState.GROUNDED:
                {
                    dragForce = groundDrag;
                    // Walk along ground normal (adjusted if on heavy slope).
                    var groundNormal = GroundNormal();
                    var direction = Vector3.ProjectOnPlane(input, groundNormal);
                    body.AddForce(direction * strafeForce * Time.deltaTime, ForceMode.VelocityChange);
                    body.AddForce(direction * strafeForce * Time.deltaTime, ForceMode.Impulse);
                    if (IsInAir()) state = PlayerState.IN_AIR;
                    break;
                }
            default:
                {
                    Debug.Log("Player in unhandled state (!)");
                    break;
                }
        }
        var yDrag = body.velocity.y < 0 ? 0f : body.velocity.y;
        body.AddForce(-dragForce * body.mass * new Vector3(body.velocity.x, yDrag, body.velocity.z), ForceMode.Force);
    }

    private void UpdateRotation()
    {
        var lookSpeedFactor = inputManager.ZoomActive ? LookSpeedZoom : lookSpeed;
        aimAngle += inputManager.lookInput * lookSpeedFactor * Time.deltaTime;
        // Constrain aiming angle vertically and wrap horizontally.
        // + and - Mathf.Deg2Rad is offsetting with 1 degree in radians,
        // which is neccesary to avoid IK shortest path slerping that causes aniamtions to break at exactly the halfway points.
        // This is way more computationaly efficient than creating edgecase checks in IK with practically no gameplay impact
        aimAngle.y = Mathf.Clamp(aimAngle.y, -Mathf.PI / 2 + Mathf.Deg2Rad, Mathf.PI / 2 - Mathf.Deg2Rad);
        aimAngle.x = (aimAngle.x + Mathf.PI) % (2 * Mathf.PI) - Mathf.PI;
        // Rotate rigidbody.
        body.MoveRotation(Quaternion.AngleAxis(aimAngle.x * Mathf.Rad2Deg, Vector3.up));
        // Rotate look separately. Camera is attached to FPSInputManager.
        inputManager.transform.localRotation = Quaternion.AngleAxis(aimAngle.y * Mathf.Rad2Deg, Vector3.left);
    }

    private void UpdateAnimatorParameters()
    {
        animator.SetFloat("Forward", Vector3.Dot(body.velocity, transform.forward) / maxVelocityBeforeExtraDamping);
        animator.SetFloat("Right", Vector3.Dot(body.velocity, transform.right) / maxVelocityBeforeExtraDamping);
    }

    void OnDrawGizmos()
    {
        if (!hitbox) return;
        var extents = new Vector3(1, 1.5f + airThreshold, 1);
        var center = hitbox.bounds.center + (0.25f + 0.5f * airThreshold) * Vector3.down;
        Gizmos.DrawWireCube(center, extents);
    }

    void FixedUpdate()
    {
        var positionInput = new Vector3(inputManager.moveInput.x, 0, inputManager.moveInput.y);
        UpdatePosition(positionInput);
        // Limit velocity when not grounded
        if (state == PlayerState.GROUNDED)
            return;

        if (isDashing)
        {
            var directionalForces = new Vector3(body.velocity.x, 0, body.velocity.z);
            if (directionalForces.magnitude < minDashVelocity)
                isDashing = false;
        }
        // Add extra drag when player velocity is too high
        var maxVelocityReached = Mathf.Abs(body.velocity.x) > maxVelocityBeforeExtraDamping || Mathf.Abs(body.velocity.z) > maxVelocityBeforeExtraDamping;
        if (maxVelocityReached)
            body.AddForce(-(isDashing ? dashDamping : extraDamping) * body.mass * new Vector3(body.velocity.x, 0, body.velocity.z), ForceMode.Force);
    }

    void Update()
    {
        UpdateRotation();
        UpdateAnimatorParameters();
    }

    private void OnDestroy()
    {
        ResetZoom();
        inputManager.onSelect -= OnJump;
        inputManager.onCrouchPerformed -= OnCrouch;
        inputManager.onCrouchCanceled -= OnCrouch;
        inputManager.onZoomPerformed -= OnZoom;
        inputManager.onZoomCanceled -= OnZoomCanceled;
        var playerManager = GetComponent<PlayerManager>();
        inputManager.onZoomPerformed -= playerManager.GunController.OnZoom;
        inputManager.onZoomCanceled -= playerManager.GunController.OnZoomCanceled;

        if (MatchController.Singleton)
            MatchController.Singleton.onRoundEnd -= ResetZoom;
    }
}
