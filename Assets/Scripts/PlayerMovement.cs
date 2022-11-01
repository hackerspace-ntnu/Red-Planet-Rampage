using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

enum PlayerState
{
    IN_AIR,
    GROUNDED,
    DEAD
}

public class PlayerMovement : MonoBehaviour
{
    private FPSInputManager fpsInput;
    private Rigidbody body;
    private Collider hitbox;

    [SerializeField]
    private float lookSpeed = 3;

    [SerializeField]
    private float strafeForce = 20;

    [SerializeField]
    private float inAirStrafeForce = 10;

    [SerializeField]
    private float jumpForce = 50;

    [SerializeField]
    private float jumpTimeout = 0.5f;

    private bool canJump = true;

    [SerializeField]
    private float airThreshold = 0.4f;

    [SerializeField]
    private float dampening = 0.04f;

    [SerializeField, ReadOnly]
    private PlayerState state = PlayerState.GROUNDED;

    private Vector2 aimAngle = Vector2.zero;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        hitbox = GetComponent<BoxCollider>();
    }

    private void OnDestroy()
    {
        //Remove listeners
        if (fpsInput)
        {
            fpsInput.onSelect -= OnJump;
            fpsInput.onMoveCanceled -= OnMoveCanceled;
        }
    }

    /// <summary>
    /// Function for setting a playerInput and adding movement related listeners to it.
    /// </summary>
    /// <param name="player"></param>
    public void SetPlayerInput(FPSInputManager player)
    {
        fpsInput = player;
        fpsInput.onSelect += OnJump;
        fpsInput.onMoveCanceled += OnMoveCanceled;
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (canJump && state == PlayerState.GROUNDED)
        {
            body.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            StartCoroutine(JumpTimeout());
        }
    }

    private IEnumerator JumpTimeout()
    {
        canJump = false;
        yield return new WaitForSeconds(jumpTimeout);
        canJump = true;
    }


    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        if (state == PlayerState.GROUNDED)
        {
            // Rapidly decelerate if movement stops when grounded.
            var velocity = new Vector3(body.velocity.x, 0, body.velocity.z);
            body.AddForce(-velocity * strafeForce * dampening, ForceMode.VelocityChange);
        }
    }

    private bool IsInAir()
    {
        // Cast a box to detect (partial) ground. See OnDrawGizmos for what I think is the extent of the box cast.
        // No, this does not work if the cast start at the bottom.
        return !Physics.BoxCast(hitbox.bounds.center, 0.5f * Vector3.one, Vector3.down, Quaternion.identity, 0.5f + airThreshold);
    }

    private void UpdatePosition(Vector3 input)
    {
        // Modify input to addforce with relation to current rotation.
        input = transform.forward * input.z + transform.right * input.x;
        switch (state)
        {
            case PlayerState.IN_AIR:
                {
                    // Strafe slightly.
                    body.AddForce(input * inAirStrafeForce, ForceMode.VelocityChange);
                    if (!IsInAir()) state = PlayerState.GROUNDED;
                    break;
                }
            case PlayerState.GROUNDED:
                {
                    // Strafe normally.
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
        aimAngle += fpsInput.lookInput * lookSpeed * Time.deltaTime;
        // Constrain aiming angle vertically and wrap horizontally.
        aimAngle.y = Mathf.Clamp(aimAngle.y, -Mathf.PI / 2, Mathf.PI / 2);
        aimAngle.x = (aimAngle.x + Mathf.PI) % (2 * Mathf.PI) - Mathf.PI;
        // Rotate rigidbody.
        body.MoveRotation(Quaternion.AngleAxis(aimAngle.x * Mathf.Rad2Deg, Vector3.up));
        // Rotate look separately. Camera is attached to FPSInputManager.
        fpsInput.transform.localRotation = Quaternion.AngleAxis(aimAngle.y * Mathf.Rad2Deg, Vector3.left);
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
        var positionInput = new Vector3(fpsInput.moveInput.x, 0, fpsInput.moveInput.y);
        UpdatePosition(positionInput * Time.deltaTime);
    }

    void Update()
    {
        UpdateRotation();
    }
}
