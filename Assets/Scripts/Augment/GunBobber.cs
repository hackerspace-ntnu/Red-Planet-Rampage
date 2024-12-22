using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class GunBobber : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    private PlayerMovement playerMovement;
    private InputManager inputManager;
    private bool isAllowed = true;
    public void SetPlayerMovement(PlayerMovement movement, InputManager input)
    {
        playerMovement = movement;
        movement.OnMove += AnimateBobbing;
        movement.OnJumpPerformed += StopBobbing;
        movement.OnLeapPerformed += LeapBobbing;
        inputManager = input;
        input.onZoomCanceled += StartBobbing;
        input.onZoomPerformed += StopBobbing;
    }

    private void AnimateBobbing(Rigidbody body)
    {
        if (!isAllowed || playerMovement.IsCrouching || playerMovement.StateIsAir || body.velocity.magnitude < 9f)
        {
            StopBobbing(body);
            return;
        }

        animator.SetBool("isBobbing", true);
        animator.speed = body.velocity.magnitude/8f;
    }
    private void LeapBobbing(Rigidbody body)
    {
        animator.SetBool("isBobbing", false);
        animator.speed = 1;
        animator.SetTrigger("leaped");
    }

    private void StopBobbing(Rigidbody body)
    {
        animator.SetBool("isBobbing", false);
        animator.speed = 1;
    }

    private void StopBobbing(InputAction.CallbackContext ctx)
    {
        StopBobbing(playerMovement.Body);
        isAllowed = false;
    }

    private void StartBobbing(InputAction.CallbackContext ctx)
    {
        isAllowed = true;
    }

    private void OnDisable()
    {
        if (playerMovement == null)
            return;

        playerMovement.OnMove -= AnimateBobbing;
        playerMovement.OnJumpPerformed -= StopBobbing;
        playerMovement.OnLeapPerformed -= LeapBobbing;
        inputManager.onZoomCanceled -= StartBobbing;
        inputManager.onZoomPerformed -= StopBobbing;
    }
}
