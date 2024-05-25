using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class BiddingPlayer : NetworkBehaviour
{
    [SerializeField]
    protected PlayerManager playerManager;
    [SerializeField]
    protected Transform sign;
    [SerializeField]
    protected Transform signMesh;
    [SerializeField]
    protected Transform signTarget;

    [SerializeField]
    protected TMP_Text chipText;
    [SerializeField]
    protected TMP_Text signCross;
    protected BiddingPlatform currentPlatform = null;

    private void Start()
    {
        GetComponent<PlayerIK>().RightHandIKTarget = signTarget;
        playerManager.onSelectedBiddingPlatformChange += AnimateChipStatus;
    }

    public void SetIdentity()
    {
        playerManager.identity.onChipChange += UpdateChipStatus;
        chipText.text = playerManager.identity.chips.ToString();
    }

    public void SetPlayerInput()
    {
        if (playerManager.inputManager)
        {
            playerManager.inputManager.onFirePerformed += AnimateBid;
            playerManager.inputManager.onSelect += AnimateBid;
        }
    }

    private void AnimateBid(InputAction.CallbackContext ctx)
    {
        CmdAnimateBid();
    }

    [Command]
    private void CmdAnimateBid()
    {
        RpcAnimateBid();
    }

    [ClientRpc]
    private void RpcAnimateBid()
    {
        if (LeanTween.isTweening(signMesh.gameObject) || !currentPlatform)
            return;

        LeanTween.sequence()
            .append(LeanTween.rotateAroundLocal(signMesh.gameObject, Vector3.right, 90, 0.15f))
            .append(LeanTween.rotateAroundLocal(signMesh.gameObject, Vector3.right, -90, 0.4f));
    }

    protected void AnimateChipStatus(BiddingPlatform platform)
    {
        if (currentPlatform)
            currentPlatform.onBidPlaced -= AnimateSignContent;

        currentPlatform = platform;
        if (!currentPlatform)
        {
            if (LeanTween.isTweening(signCross.gameObject))
                LeanTween.cancel(signCross.gameObject);
            signCross.alpha = 0f;
            return;
        }

        currentPlatform.onBidPlaced += AnimateSignContent;
        AnimateSignContent(platform);
    }

    // TODO this stuff plays on the wrong player for network players
    protected void AnimateSignContent(BiddingPlatform platform)
    {
        bool isLeaderAndCanBid = (platform.LeadingBidder == playerManager.id) && (playerManager.identity.chips > 0);
        if (platform.chips < playerManager.identity.chips || isLeaderAndCanBid)
        {
            if (LeanTween.isTweening(signCross.gameObject))
                LeanTween.cancel(signCross.gameObject);
            signCross.alpha = 0f;
            return;
        }

        LeanTween.value(signCross.gameObject, (alpha) => signCross.alpha = alpha, 0f, 1f, 0.5f).setLoopPingPong();
    }

    protected void UpdateChipStatus(int chips)
    {
        chipText.text = chips.ToString();
    }

    private void FixedUpdate()
    {
        if (LeanTween.isTweening(signMesh.gameObject))
            return;
        var yAbove = Camera.main ? Camera.main.transform.position.y : sign.position.y + 100;
        sign.LookAt(new Vector3(sign.position.x, yAbove, sign.position.z));
    }

    private void OnDestroy()
    {
        if (!playerManager)
            return;
        if (playerManager.inputManager)
        {
            playerManager.inputManager.onFirePerformed -= AnimateBid;
            playerManager.inputManager.onSelect -= AnimateBid;
        }
        if (playerManager.identity)
        {
            playerManager.identity.onChipChange -= UpdateChipStatus;
        }
        playerManager.onSelectedBiddingPlatformChange -= AnimateChipStatus;
    }
}
