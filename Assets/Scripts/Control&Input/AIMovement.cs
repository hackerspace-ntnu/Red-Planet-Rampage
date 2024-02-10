using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class AIMovement : PlayerMovement
{
    public Vector3 MoveDirection;
    [SerializeField, ReadOnly]
    private PlayerState state = PlayerState.GROUNDED;
    void Start()
    {
        MoveDirection = Vector3.zero;
        body = GetComponent<Rigidbody>();
        hitbox = GetComponent<BoxCollider>();
        strafeForce = strafeForceGrounded;
    }
    private void Update()
    {
        UpdateAnimatorParameters();
    }

    protected override void UpdateRotation()
    {
        //var lookSpeedFactor = 10;
        //var lookInput = 1 * Time.deltaTime;
        //aimAngle += lookInput * lookSpeedFactor;
        // Constrain aiming angle vertically and wrap horizontally.
        // + and - Mathf.Deg2Rad is offsetting with 1 degree in radians,
        // which is neccesary to avoid IK shortest path slerping that causes aniamtions to break at exactly the halfway points.
        // This is way more computationaly efficient than creating edgecase checks in IK with practically no gameplay impact
        //aimAngle.y = Mathf.Clamp(aimAngle.y, -Mathf.PI / 2 + Mathf.Deg2Rad, Mathf.PI / 2 - Mathf.Deg2Rad);
        aimAngle.x = (aimAngle.x + Mathf.PI) % (2 * Mathf.PI) - Mathf.PI;
        // Rotate rigidbody.
        body.MoveRotation(Quaternion.LookRotation(MoveDirection));
    }

    private void FixedUpdate()
    {
        UpdatePosition(MoveDirection);
        UpdateRotation();
    }

}
