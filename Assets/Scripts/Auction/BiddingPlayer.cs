using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class BiddingPlayer : MonoBehaviour
{
    [SerializeField]
    private PlayerManager playerManager;
    [SerializeField]
    private Transform sign;
    [SerializeField]
    private Transform signMesh;
    [SerializeField]
    private Transform signTarget;
    [SerializeField]
    private Transform root;
    [SerializeField]
    private Transform joint;
    [SerializeField]
    private Transform end;
    [SerializeField]
    private Transform pole;

    [SerializeField]
    private TMP_Text chipText;

    private BiddingPlatform currentPlatform = null;

    private void Start()
    {
        playerManager.inputManager.onFirePerformed += AnimateBid;
        playerManager.inputManager.onSelect += AnimateBid;
        playerManager.identity.onChipChange += AnimateChipStatus;
        playerManager.onSelectedBiddingPlatformChange += AnimateChipStatus;

        chipText.text = playerManager.identity.chips.ToString();
    }
    private void AnimateBid(InputAction.CallbackContext ctx)
    {
        if (LeanTween.isTweening(signMesh.gameObject) || !currentPlatform)
            return;

        LeanTween.sequence()
            .append(LeanTween.rotateAroundLocal(signMesh.gameObject, Vector3.right, 90, 0.15f))
            .append(LeanTween.rotateAroundLocal(signMesh.gameObject, Vector3.right, -90, 0.4f));
    }

    private void AnimateChipStatus(BiddingPlatform platform)
    {
        currentPlatform = platform;
    }

    private void AnimateChipStatus(int chips)
    {
        chipText.text = chips.ToString();
    }

    private void FixedUpdate()
    {
        if (LeanTween.isTweening(signMesh.gameObject))
            return;
        sign.LookAt(new Vector3(sign.position.x, Camera.main.transform.position.y, sign.position.z));
    }

    private void LateUpdate()
    {
        GenericTwoBodyIK.AnimateTransforms(root, joint, end, pole, signTarget);
    }
    private void OnDestroy()
    {
        playerManager.inputManager.onFirePerformed -= AnimateBid;
        playerManager.inputManager.onSelect -= AnimateBid;
        playerManager.identity.onChipChange -= AnimateChipStatus;
        playerManager.onSelectedBiddingPlatformChange -= AnimateChipStatus;
    }
}
