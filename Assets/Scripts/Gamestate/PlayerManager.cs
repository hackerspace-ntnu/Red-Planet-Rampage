using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerManager : MonoBehaviour
{
    // Layers 12 through 15 are gun layers.
    private static int allGunsMask = (1 << 12) | (1 << 13) | (1 << 14) | (1 << 15);

    // TODO add context when shooty system is done
    public delegate void HitEvent(PlayerManager killer, PlayerManager victim);

    public HitEvent onDeath;

    public delegate void BiddingPlatformEvent(BiddingPlatform platform);
    public BiddingPlatformEvent onSelectedBiddingPlatformChange;

    public FPSInputManager fpsInput;
    public PlayerIdentity identity;

    [SerializeField]
    private GameObject meshBase;

    [SerializeField]
    private Rigidbody ragdoll;

    private BiddingPlatform selectedBiddingPlatform;
    public BiddingPlatform SelectedBiddingPlatform
    {
        get { return selectedBiddingPlatform; }
        set
        {
            selectedBiddingPlatform = value;
            onSelectedBiddingPlatformChange?.Invoke(value);
        }
    }


    private GunController gunController;

    private HealthController healthController;

    [SerializeField]
    private HUDController hudController;

    void Start()
    {
        healthController = GetComponent<HealthController>();
        healthController.onDamageTaken += OnDamageTaken;
        healthController.onDeath += OnDeath;
    }

    void OnDamageTaken(HealthController healthController, float damage, DamageInfo info)
    {
        hudController.UpdateHealthBar(healthController.CurrentHealth, healthController.MaxHealth);
    }

    void OnDeath(HealthController healthController, float damage, DamageInfo info)
    {
        onDeath?.Invoke(info.sourcePlayer, this);
        TurnIntoRagdoll(info.projectileState.position, info.projectileState.direction);
        hudController.DisplayDeathScreen(info.sourcePlayer.identity);
    }

    void TurnIntoRagdoll(Vector3 impactSite, Vector3 impactDirection)
    {
        // Disable components
        GetComponent<PlayerMovement>().enabled = false;
        healthController.enabled = false;
        // TODO display guns falling to the floor
        gunController.gameObject.SetActive(false);
        // Disable all colliders and physics
        // Ragdollify

        // TODO: Make accurate hitbox forces for the different limbs of the player

        GetComponent<RagdollController>().EnableRagdoll();
    }

    /// <summary>
    /// Function for setting a playerInput, adding movement related listeners to it
    /// and performing other necessary operations that require playerInput/-Identity.
    /// </summary>
    /// <param name="playerInput"></param>
    public void SetPlayerInput(FPSInputManager playerInput)
    {
        fpsInput = playerInput;
        identity = fpsInput.GetComponent<PlayerIdentity>();
        GetComponent<PlayerMovement>().SetPlayerInput(fpsInput);
        SetGun(fpsInput.transform);
        fpsInput.onFirePerformed += OnFire;
        fpsInput.onFireCanceled += OnFireEnd;
        // Set camera on canvas
        var canvas = hudController.GetComponent<Canvas>();
        canvas.worldCamera = fpsInput.GetComponentInChildren<Camera>();
        canvas.planeDistance = 0.11f;
        
        // Set player color
         //var meshRenderer = meshBase.GetComponentInChildren<SkinnedMeshRenderer>();
         //var ragdollRenderer = ragdoll.GetComponentInChildren<SkinnedMeshRenderer>();
         //meshRenderer.materials[0].SetColor("_Color", identity.color);
         //ragdollRenderer.materials[0].SetColor("_Color", identity.color);
    }

    void OnDestroy()
    {
        healthController.onDamageTaken -= OnDamageTaken;
        healthController.onDeath -= OnDeath;
        fpsInput.onFirePerformed -= OnFire;
        fpsInput.onFireCanceled -= OnFireEnd;
        //Remove the gun
        Destroy(gunController.gameObject);
    }

    private void OnFire(InputAction.CallbackContext ctx)
    {
        gunController.triggerHeld = true;
        gunController.triggerPressed = true;
        StartCoroutine(UnpressTrigger());

        if (!selectedBiddingPlatform) return;
        selectedBiddingPlatform.TryPlaceBid(identity);
    }

    IEnumerator UnpressTrigger()
    {
        yield return new WaitForFixedUpdate();
        gunController.triggerPressed = false;
    }

    private void OnFireEnd(InputAction.CallbackContext ctx)
    {
        gunController.triggerHeld = false;
    }

    public void SetLayer(int playerIndex)
    {
        int playerLayer = LayerMask.NameToLayer("Player " + playerIndex);

        // Set layers for the camera to ignore (the other players' gun layers, and this layer)
        // Bitwise negation of this player's model layer and all gun layers that do not belong to this player
        // Gun layers are 4 above their respective player layers.
        var mask = ((1 << 16) - 1) ^ ((1 << playerLayer) | ((1 << (playerLayer + 4)) ^ allGunsMask));
        fpsInput.GetComponent<Camera>().cullingMask = mask;

        // Set correct layer on self, mesh and gun (TODO)
        gameObject.layer = playerLayer;
        SetLayerOnSubtree(meshBase, playerLayer);
    }

    private void SetGun(Transform offset)
    {
        var gun = GunFactory.InstantiateGun(identity.Body.augment, identity.Barrel.augment, identity.Extension.augment, offset);
        // Set specific local transform
        gun.transform.localPosition = new Vector3(0.39f, -0.12f, -0.4f);
        gun.transform.localRotation = Quaternion.AngleAxis(-12.5f, Vector3.up);
        // Remember gun controller
        gunController = gun.GetComponent<GunController>();
        // Make gun remember who shoots with it
        gunController.player = this;
    }

    private void SetLayerOnSubtree(GameObject node, int layer)
    {
        node.layer = layer;
        foreach (Transform child in node.transform)
        {
            SetLayerOnSubtree(child.gameObject, layer);
        }
    }

    new public string ToString()
    {
        return "Player " + fpsInput.playerInput.playerIndex;
    }
}
