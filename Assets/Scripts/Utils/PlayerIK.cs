using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIK : MonoBehaviour
{
    [Header("Left Arm")]
    [SerializeField]
    private Transform leftArmRoot;
    [SerializeField]
    private float leftArmRootOffset = -70f;
    [SerializeField]
    private Transform leftArmElbow;
    [SerializeField]
    private float leftArmElbowOffset = -1.6f;
    [SerializeField]
    private Transform leftArmHand;
    // Which pole (direction) elbow bends towards
    [SerializeField]
    private Transform leftHandIKPole;
    public Transform LeftHandIKTarget { get; set; }
    
    [Header("Right Arm")]
    [SerializeField]
    private Transform rightArmRoot;
    [SerializeField]
    private float rightArmRootOffset = 70;
    [SerializeField]
    private Transform rightArmElbow;
    [SerializeField]
    private float rightArmElbowOffset = 1.6f;
    [SerializeField]
    private Transform rightArmHand;
    // Which pole (direction) elbow bends towards
    [SerializeField]
    private Transform rightHandIKPole;
    public Transform RightHandIKTarget { get; set; }

    [SerializeField]
    private float modelScaleOffset;

    private void LateUpdate()
    {
        if (LeftHandIKTarget)
            GenericTwoBodyIK.AnimateTransforms(leftArmRoot, leftArmElbow, leftArmHand, leftHandIKPole, LeftHandIKTarget, leftArmRootOffset, leftArmElbowOffset, modelScaleOffset);
        if (RightHandIKTarget)
            GenericTwoBodyIK.AnimateTransforms(rightArmRoot, rightArmElbow, rightArmHand, rightHandIKPole, RightHandIKTarget, rightArmRootOffset, rightArmElbowOffset, modelScaleOffset);
    }
}