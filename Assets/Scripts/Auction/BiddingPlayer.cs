using Mirror;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
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
    private Color maxChipColor;
    [SerializeField]
    protected TMP_Text signCross;
    protected BiddingPlatform currentPlatform = null;
    [SerializeField]
    protected Renderer[] playerRenderers;
    protected List<Material> instantiatedMaterials = new();

    private void Start()
    {
        GetComponent<PlayerIK>().RightHandIKTarget = signTarget;
        playerManager.onSelectedBiddingPlatformChange += AnimateChipStatus;
        playerManager.onSelectedBiddingPlatformChange += ToggleDither;
        InstantiateMaterials();
        AuctionDriver.Singleton.OnYieldChange += UpdateYieldStatus;
    }

    protected void InstantiateMaterials()
    {
        playerRenderers.ToList()
            .ForEach(mesh =>
            {
                for (int i = 0; i < mesh.materials.Length; i++)
                {
                    mesh.materials[i] = Instantiate(mesh.materials[i]);
                    instantiatedMaterials.Add(mesh.materials[i]);
                }
            });
    }

    public void SetIdentity()
    {
        playerManager.identity.onChipChange += UpdateChipStatus;
        chipText.text = "<sprite name=\"chip\">" + playerManager.identity.Chips.ToString();
        chipText.color = playerManager.identity.HasMaxChips ? maxChipColor : Color.black;
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

    protected void ToggleDither(BiddingPlatform platform)
    {
        instantiatedMaterials
            .ForEach(material =>
                material.SetFloat("_DitherDensity", platform != null ? 0.9f : 1f));
    }

    // TODO this stuff plays on the wrong player for network players
    protected void AnimateSignContent(BiddingPlatform platform)
    {
        bool isLeaderAndCanBid = (platform.LeadingBidder == playerManager.id) && (playerManager.identity.Chips > 0);
        if (platform.chips < playerManager.identity.Chips || isLeaderAndCanBid)
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
        chipText.text = "<sprite name=\"chip\">" + chips.ToString();
        chipText.color = playerManager.identity.HasMaxChips ? maxChipColor : Color.black;
    }

    private void UpdateYieldStatus()
    {
        signMesh.gameObject.SetActive(!AuctionDriver.Singleton.IsPlayerYielding(playerManager));
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
        AuctionDriver.Singleton.OnYieldChange -= UpdateYieldStatus;
        playerManager.onSelectedBiddingPlatformChange -= ToggleDither;
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
