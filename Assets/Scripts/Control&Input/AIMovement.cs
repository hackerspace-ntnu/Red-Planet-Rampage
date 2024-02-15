using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class AIMovement : PlayerMovement
{
    void Start()
    {
        body = GetComponent<Rigidbody>();
        hitbox = GetComponent<BoxCollider>();
        strafeForce = strafeForceGrounded;
    }
    private void Update()
    {
        UpdateAnimatorParameters();
    }

    private void FixedUpdate()
    {
    }

    public void Jump()
    {
        if (StateIsAir)
            return;
        body.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
    }
}
