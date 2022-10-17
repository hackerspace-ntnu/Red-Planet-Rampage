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
    private PlayerInput playerInput;
    private FPSInputManager fpsInput;
    private Rigidbody body;
    private Collider hitbox;

    [SerializeField]
    private float strafeForce = 20;

    [SerializeField]
    private float inAirStrafeForce = 10;

    [SerializeField]
    private float jumpForce = 50;

    [SerializeField]
    private float airThreshold = 0.4f;

    [SerializeField]
    private float dampening = 0.04f;

    [SerializeField, ReadOnly]
    private PlayerState state = PlayerState.GROUNDED;

    void Start()
    {
        playerInput = FindObjectOfType<PlayerInput>();
        fpsInput = playerInput.gameObject.GetComponent<FPSInputManager>();
        fpsInput.onSelect += OnJump;
        fpsInput.onMoveCanceled += OnMoveCanceled;

        body = GetComponent<Rigidbody>();
        hitbox = GetComponent<BoxCollider>();
    }

    void OnJump(InputAction.CallbackContext ctx)
    {
        if (state == PlayerState.GROUNDED)
        {
            body.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();
        if (state == PlayerState.GROUNDED)
        {
            // Rapidly decelerate if movement stops when grounded.
            var velocity = new Vector3(body.velocity.x, 0, body.velocity.z);
            body.AddForce(-velocity * strafeForce * dampening, ForceMode.VelocityChange);
        }
    }

    bool IsInAir()
    {
        // Cast a box to detect (partial) ground. See OnDrawGizmos for what I think is the extent of the box cast.
        // No, this does not work if the cast start at the bottom.
        return !Physics.BoxCast(hitbox.bounds.center, 0.5f * Vector3.one, Vector3.down, Quaternion.identity, 0.5f + airThreshold);
    }

    void OnDrawGizmos()
    {
        var extents = new Vector3(1, 1.5f + airThreshold, 1);
        var center = hitbox.bounds.center + (0.25f + 0.5f * airThreshold) * Vector3.down;
        Gizmos.DrawWireCube(center, extents);
    }

    void FixedUpdate()
    {
        var input = new Vector3(fpsInput.moveInput.x, 0, fpsInput.moveInput.y) * Time.deltaTime;
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
}
