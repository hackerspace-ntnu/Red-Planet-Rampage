using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(AudioSource))]
public class PlayerManager : MonoBehaviour
{
    // Layers 12 through 15 are gun layers.
    private static int allGunsMask = (1 << 12) | (1 << 13) | (1 << 14) | (1 << 15);

    // Only Default and HitBox layers can be hit
    private static int hitMask = 1 | (1 << 3);
    public int HitMask => hitMask;

    [Header("Shooting")]

    [SerializeField]
    private float maxHitDistance = 100;

    [SerializeField]
    private float targetStartOffset = 0.28f;
    public float TargetStartOffset => targetStartOffset;
    public bool overrideAimTarget = false;

    // TODO add context when shooty system is done
    public delegate void HitEvent(PlayerManager killer, PlayerManager victim);

    public HitEvent onDeath;

    private PlayerManager lastPlayerThatHitMe;

    public delegate void BiddingPlatformEvent(BiddingPlatform platform);
    public BiddingPlatformEvent onSelectedBiddingPlatformChange;

    [SerializeField]
    private Item ammoMaskItem;

    [Header("Related objects")]

    public InputManager inputManager;
    public PlayerIdentity identity;

    [SerializeField]
    private PlayerHUDController hudController;
    public PlayerHUDController HUDController => hudController;


    [Header("Physics")]

    [SerializeField]
    private GameObject meshBase;

    [SerializeField]
    private PlayerIK playerIK;


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
    public GunController GunController => gunController;

    private HealthController healthController;

    [Header("Hit sounds")]

    private AudioSource audioSource;

    [SerializeField]
    private AudioGroup hitSounds;

    [SerializeField]
    private AudioGroup extraHitSounds;

    void Start()
    {
        healthController = GetComponent<HealthController>();
        healthController.onDamageTaken += OnDamageTaken;
        healthController.onDeath += OnDeath;
        audioSource = GetComponent<AudioSource>();
    }

    void OnDamageTaken(HealthController healthController, float damage, DamageInfo info)
    {
        hudController.OnDamageTaken(damage, healthController.CurrentHealth, healthController.MaxHealth);
        PlayOnHit();
        if (info.sourcePlayer != this)
        {
            lastPlayerThatHitMe = info.sourcePlayer;
        }
    }

    void OnDeath(HealthController healthController, float damage, DamageInfo info)
    {
        var killer = info.sourcePlayer;
        if (info.sourcePlayer == this && lastPlayerThatHitMe)
        {
            killer = lastPlayerThatHitMe;
        }
        onDeath?.Invoke(killer, this);
        TurnIntoRagdoll();
        hudController.DisplayDeathScreen(killer.identity);
    }

    void TurnIntoRagdoll()
    {
        // Disable components
        GetComponent<PlayerMovement>().enabled = false;
        healthController.enabled = false;
        playerIK.enabled = false;
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
        // Subscribe relevant input events
        inputManager.onFirePerformed += Fire;
        inputManager.onFireCanceled += FireEnd;
        inputManager.onSelect += TryPlaceBid;
        inputManager.onFirePerformed += TryPlaceBid;
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
            gunController.onFire -= UpdateHudFire;
            gunController.onReload -= UpdateHudReload;
            //Remove the gun
            Destroy(gunController.gameObject);
        }
    }

    private void UpdateAimTarget(GunStats stats)
    {
        if (overrideAimTarget)
            return;
        Vector3 cameraCenter = inputManager.transform.position;
        Vector3 cameraDirection = inputManager.transform.forward;
        Vector3 startPoint = cameraCenter + cameraDirection * targetStartOffset;
        if (Physics.Raycast(startPoint, cameraDirection, out RaycastHit hit, maxHitDistance, hitMask))
        {
            gunController.target = hit.point;
        }
        else
        {
            gunController.target = cameraCenter + cameraDirection * maxHitDistance;
        }
    }

    private void Fire(InputAction.CallbackContext ctx)
    {
        if (!gunController)
            return;
        gunController.triggerHeld = true;
        gunController.triggerPressed = true;
        StartCoroutine(UnpressTrigger());
    }

    private void UpdateHudFire(GunStats stats)
    {
        // stats variables must be dereferenced
        float ammo = stats.Ammo;
        float magazine = stats.magazineSize;
        hudController.UpdateOnFire(ammo / magazine);
    }

    private void UpdateHudReload(GunStats stats)
    {
        float ammo = stats.Ammo;
        float magazine = stats.magazineSize;
        hudController.UpdateOnReload(ammo / magazine);
    }

    private void TryPlaceBid(InputAction.CallbackContext ctx)
    {
        if (!selectedBiddingPlatform) return;
        selectedBiddingPlatform.TryPlaceBid(identity);
    }

    IEnumerator UnpressTrigger()
    {
        yield return new WaitForFixedUpdate();
        gunController.triggerPressed = false;
    }

    private void FireEnd(InputAction.CallbackContext ctx)
    {
        gunController.triggerHeld = false;
    }

    public void SetLayer(int playerIndex)
    {
        int playerLayer = LayerMask.NameToLayer("Player " + playerIndex);

        // Set layers for the camera to ignore (the other players' gun layers, and this layer)
        // Bitwise negation of this player's model layer and all gun layers that do not belong to this player
        // Gun layers are 4 above their respective player layers.
        var playerMask = 1 << playerLayer;
        var gunMask = (1 << (playerLayer + 4)) ^ allGunsMask;

        // Ignore ammo boxes if this player doesn't have the required body
        var hasAmmoBoxBody = identity.Body == ammoMaskItem;
        var ammoMask = hasAmmoBoxBody ? 0 : 1 << 6;

        var negatedMask = ((1 << 16) - 1) ^ (playerMask | gunMask | ammoMask);

        inputManager.GetComponent<Camera>().cullingMask = negatedMask;

        // Set correct layer on self, mesh and gun (TODO)
        gameObject.layer = playerLayer;
        SetLayerOnSubtree(meshBase, playerLayer);
    }

    public void SetGun(Transform offset)
    {
        var gun = GunFactory.InstantiateGun(identity.Body, identity.Barrel, identity?.Extension, this, offset);
        // Set specific local transform
        gun.transform.localPosition = new Vector3(0.39f, -0.34f, 0.5f);
        gun.transform.localRotation = Quaternion.AngleAxis(0.5f, Vector3.up);
        // Remember gun controller
        gunController = gun.GetComponent<GunController>();
        gunController.onFire += UpdateAimTarget;
        gunController.onFire += UpdateHudFire;
        gunController.onReload += UpdateHudReload;

        playerIK.LeftHandIKTarget = gunController.LeftHandTarget;
        playerIK.RightHandIKTarget = gunController.RightHandTarget;
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
        return "Player " + inputManager.playerInput.playerIndex;
    }

    private void PlayOnHit()
    {
        if (Random.Range(0, 1000) > 5)
        {
            hitSounds.Play(audioSource);
        }
        else
        {
            extraHitSounds.Play(audioSource);
        }
    }
}
