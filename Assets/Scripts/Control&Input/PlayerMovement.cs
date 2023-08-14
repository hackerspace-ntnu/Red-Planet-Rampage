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

    [SerializeField]
    private LayerMask ignoreMask;

    [SerializeField]
    private float lookSpeed = 3;

    [SerializeField]
    private float maxVelocity = 10;

    private float strafeForce;

    [SerializeField]
    private float strafeForceGrounded = 60;

    [SerializeField]
    private float strafeForceCrouched = 30;

    [SerializeField]
    private float strafeForceInAir = 16;

    [SerializeField]
    private float drag = 6f;

    [SerializeField]
    private float airDrag = 2f;

    [SerializeField]
    private float jumpForce = 10;

    [SerializeField]
    private float jumpForceLeap = 12.5f;

    [SerializeField]
    private float jumpTimeout = 0.5f;

    [SerializeField]
    private float jumpTimeoutLeap = 0.875f;

    private const float marsGravity = 3.72076f;

    private bool canJump = true;

    [SerializeField]
    private float crouchedCameraHeightOffset = 0.3f;

    private float localHeightInputManager;

    [SerializeField]
    private float airThreshold = 0.4f;

    [SerializeField, ReadOnly]
    private PlayerState state = PlayerState.GROUNDED;

    [SerializeField]
    private Animator animator;

    private Vector2 aimAngle = Vector2.zero;

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
        inputManager.onSelect += OnJump;
        inputManager.onCrouchPerformed += SetCrouch;
        inputManager.onCrouchCanceled += SetCrouch;
        localHeightInputManager = inputManager.transform.localPosition.y;
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (!(canJump && state == PlayerState.GROUNDED))
            return;

        // Leap jump
        if (animator.GetBool("Crouching"))
        {
            body.AddForce(Vector3.up * jumpForceLeap, ForceMode.VelocityChange);
            Vector3 forwardDirection = new Vector3(inputManager.transform.forward.x, 0, inputManager.transform.forward.z);
            body.AddForce(forwardDirection * jumpForceLeap, ForceMode.VelocityChange);
            animator.SetTrigger("Leap");
            StartCoroutine(JumpTimeout(jumpTimeoutLeap));
            return;
        }
        
        // Normal jump
        body.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        StartCoroutine(JumpTimeout(jumpTimeout));
    }

    private void SetCrouch(InputAction.CallbackContext ctx)
    {
        if (LeanTween.isTweening(inputManager.gameObject))
        {
            LeanTween.cancel(inputManager.gameObject);
            inputManager.transform.localPosition = new Vector3(inputManager.transform.localPosition.x, localHeightInputManager, inputManager.transform.localPosition.z);
        }


        if (ctx.performed)
        {
            animator.SetBool("Crouching", true);
            strafeForce = strafeForceCrouched;
            if (!IsInAir())
                inputManager.gameObject.LeanMoveLocalY(localHeightInputManager - crouchedCameraHeightOffset, 0.2f);
        }
            
        if (ctx.canceled)
        {
            animator.SetBool("Crouching", false);
            strafeForce = strafeForceGrounded;
            if (!IsInAir())
                inputManager.gameObject.LeanMoveLocalY(localHeightInputManager + crouchedCameraHeightOffset, 0.2f);
        }
            
    }

    private IEnumerator JumpTimeout(float time)
    {
        canJump = false;
        yield return new WaitForSeconds(time);
        canJump = true;
    }


    private bool IsInAir()
    {
        // Cast a box to detect (partial) ground. See OnDrawGizmos for what I think is the extent of the box cast.
        // No, this does not work if the cast start at the bottom.
        return !Physics.BoxCast(hitbox.bounds.center, 0.5f * Vector3.one, Vector3.down, Quaternion.identity, 0.5f + airThreshold, ignoreMask); ;
    }

    private void UpdatePosition(Vector3 input)
    {
        // Modify input to addforce with relation to current rotation.
        input = transform.forward * input.z + transform.right * input.x;
        switch (state)
        {
            case PlayerState.IN_AIR:
                {
                    // Strafe slightly with less drag.
                    body.drag = airDrag;
                    body.AddForce(input * strafeForceInAir, ForceMode.VelocityChange);
                    body.AddForce(Vector3.down * marsGravity, ForceMode.Acceleration);
                    if (!IsInAir()) state = PlayerState.GROUNDED;
                    break;
                }
            case PlayerState.GROUNDED:
                {
                    // Strafe normally with heavy drag.
                    body.drag = drag;
                    body.AddForce(input * strafeForce, ForceMode.VelocityChange);
                    if (IsInAir()) state = PlayerState.IN_AIR;
                    break;
                }
            default:
                {
                    Debug.Log("Player in unhandled state (!)");
                    break;
                }
        }
    }

    private void UpdateRotation()
    {
        aimAngle += inputManager.lookInput * lookSpeed * Time.deltaTime;
        // Constrain aiming angle vertically and wrap horizontally.
        aimAngle.y = Mathf.Clamp(aimAngle.y, -Mathf.PI / 2, Mathf.PI / 2);
        aimAngle.x = (aimAngle.x + Mathf.PI) % (2 * Mathf.PI) - Mathf.PI;
        // Rotate rigidbody.
        body.MoveRotation(Quaternion.AngleAxis(aimAngle.x * Mathf.Rad2Deg, Vector3.up));
        // Rotate look separately. Camera is attached to FPSInputManager.
        inputManager.transform.localRotation = Quaternion.AngleAxis(aimAngle.y * Mathf.Rad2Deg, Vector3.left);
    }

    private void UpdateAnimatorParameters()
    {
        animator.SetFloat("Forward", Vector3.Dot(body.velocity, transform.forward) / maxVelocity);
        animator.SetFloat("Right", Vector3.Dot(body.velocity, transform.right) / maxVelocity);
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
        UpdatePosition(positionInput * Time.deltaTime);
        // Limit velocity when not grounded
        if (state == PlayerState.GROUNDED)
            return;
        body.velocity = new Vector3(Mathf.Clamp(body.velocity.x, -maxVelocity, maxVelocity), body.velocity.y, Mathf.Clamp(body.velocity.z, -maxVelocity, maxVelocity));
    }

    void Update()
    {
        UpdateRotation();
        UpdateAnimatorParameters();
    }
}
