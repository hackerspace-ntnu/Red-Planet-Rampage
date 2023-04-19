using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerManager : MonoBehaviour
{
    // Layers 12 through 15 are gun layers.
    private static int allGunsMask = (1 << 12) | (1 << 13) | (1 << 14) | (1 << 15);

    // Only Default and HitBox layers can be hit
    private static int hitMask = 1 | (1 << 3);

    [SerializeField]
    private float maxHitDistance = 100;

    // TODO add context when shooty system is done
    public delegate void HitEvent(PlayerManager killer, PlayerManager victim);

    public HitEvent onDeath;

    public delegate void BiddingPlatformEvent(BiddingPlatform platform);
    public BiddingPlatformEvent onSelectedBiddingPlatformChange;

    public InputManager inputManager;
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
    private PlayerHUDController hudController;

    private MatchController matchController;
    
    void Start()
    {
        healthController = GetComponent<HealthController>();
        healthController.onDamageTaken += OnDamageTaken;
        healthController.onDeath += OnDeath;
        matchController = FindObjectOfType<MatchController>();
    }

    void OnDamageTaken(HealthController healthController, float damage, DamageInfo info)
    {
        hudController.OnDamageTaken(damage, healthController.CurrentHealth, healthController.MaxHealth);
    }

    void OnDeath(HealthController healthController, float damage, DamageInfo info)
    {
        onDeath?.Invoke(info.sourcePlayer, this);
        TurnIntoRagdoll(info.position, info.force);
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
    public void SetPlayerInput(InputManager playerInput)
    {
        inputManager = playerInput;
        identity = inputManager.GetComponent<PlayerIdentity>();
        GetComponent<PlayerMovement>().SetPlayerInput(inputManager);
        SetGun(inputManager.transform);
        inputManager.onFirePerformed += OnFire;
        inputManager.onFireCanceled += OnFireEnd;
        inputManager.onPausePerformed += Pause;
        // Set camera on canvas
        var canvas = hudController.GetComponent<Canvas>();
        canvas.worldCamera = inputManager.GetComponentInChildren<Camera>();
        canvas.planeDistance = 0.11f;

        // Set player color
        var meshRenderer = meshBase.GetComponentInChildren<SkinnedMeshRenderer>().material.color = identity.color;
    }

    void OnDestroy()
    {
        healthController.onDamageTaken -= OnDamageTaken;
        healthController.onDeath -= OnDeath;
        if (gunController)
        {
            gunController.onFire -= UpdateAimTarget;
            //Remove the gun
            Destroy(gunController.gameObject);
        }
    }

    private void UpdateAimTarget(GunStats stats)
    {
        Vector3 cameraCenter = inputManager.transform.position;
        Vector3 cameraDirection = inputManager.transform.forward;
        if (Physics.Raycast(cameraCenter, cameraDirection, out RaycastHit hit, maxHitDistance, hitMask))
        {
            gunController.target = hit.point;
        }
        else
        {
            gunController.target = cameraCenter + cameraDirection * maxHitDistance;
        }
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
        inputManager.GetComponent<Camera>().cullingMask = mask;

        // Set correct layer on self, mesh and gun (TODO)
        gameObject.layer = playerLayer;
        SetLayerOnSubtree(meshBase, playerLayer);
    }

    private void SetGun(Transform offset)
    {
        var gun = GunFactory.InstantiateGun(identity.Body, identity.Barrel, identity?.Extension, this, offset);
        // Set specific local transform
        gun.transform.localPosition = new Vector3(0.39f, -0.34f, 0.5f);
        gun.transform.localRotation = Quaternion.AngleAxis(0.5f, Vector3.up);
        // Remember gun controller
        gunController = gun.GetComponent<GunController>();
        gunController.onFire += UpdateAimTarget;
    }

    private void SetLayerOnSubtree(GameObject node, int layer)
    {
        node.layer = layer;
        foreach (Transform child in node.transform)
        {
            SetLayerOnSubtree(child.gameObject, layer);
        }
    }

    public void Pause(InputAction.CallbackContext ctx)
    {
        matchController.TogglePause();
    }


    new public string ToString()
    {
        return "Player " + inputManager.playerInput.playerIndex;
    }
}
