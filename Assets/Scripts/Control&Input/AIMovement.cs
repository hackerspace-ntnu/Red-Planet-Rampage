using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AIMovement : PlayerMovement
{
    public Transform Target;
    private NavMeshAgent agent;
    private float timeSinceJump = 0f;

    protected override void Start()
    {
        body = GetComponent<Rigidbody>();
        hitbox = GetComponent<BoxCollider>();
        strafeForce = strafeForceGrounded;
    }

    private void Update()
    {
        UpdateAnimatorParameters();
        if (!Target)
            return;
        transform.LookAt(new Vector3(Target.position.x, transform.position.y, Target.position.z), transform.up);
        timeSinceJump += Time.deltaTime;
        if (timeSinceJump >= 3f)
        {
            timeSinceJump = timeSinceJump % 3f;
            if (Physics.Raycast(transform.position, Target.position, ignoreMask) && !StateIsAir)
                this.enabled = false;
            Jump();
        }
    }

    protected override void FixedUpdate()
    {
        if (!StateIsAir && (!Target || Vector3.Distance(Target.position, transform.position) > 35))
        {
            this.enabled = false;
            return;
        }
        var direction = (Target.position - transform.position).normalized;
        UpdatePosition(new Vector3(direction.x, 0, direction.z));
    }

    private void OnEnable()
    {
        body = GetComponent<Rigidbody>();
        body.isKinematic = false;
        hitbox = GetComponent<BoxCollider>();
        hitbox.isTrigger = false;
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;
        strafeForce = strafeForceGrounded;
    }

    private void OnDisable()
    {
        body.isKinematic = true;
        hitbox.isTrigger = true;
        agent.enabled = true;
    }

    public void Jump()
    {
        if (StateIsAir)
            return;
        body.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
    }
}
