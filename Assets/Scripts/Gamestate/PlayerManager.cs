using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(AudioSource))]
public class PlayerManager : MonoBehaviour
{
    // Layers 12 through 15 are gun layers.
    protected static int allGunsMask = (1 << 12) | (1 << 13) | (1 << 14) | (1 << 15);

    // Only Default and HitBox layers can be hit
    private static int hitMask = 1 | (1 << 3);

    private const int defaultLayer = 1;
    public int HitMask => hitMask;

    [SerializeField]
    private LayerMask interactMask;

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

    protected PlayerManager lastPlayerThatHitMe;

    public delegate void BiddingPlatformEvent(BiddingPlatform platform);
    public BiddingPlatformEvent onSelectedBiddingPlatformChange;

    [SerializeField]
    protected Item ammoMaskItem;

    [SerializeField]
    public Transform GunHolder;
    [SerializeField]
    public Transform GunOrigin;

    [SerializeField]
    protected GameObject aimAssistCollider;

    [Header("Related objects")]

    public InputManager inputManager;
    public PlayerIdentity identity;

    [SerializeField]
    private PlayerHUDController hudController;
    public PlayerHUDController HUDController => hudController;

    [SerializeField]
    protected GameObject aiTarget;
    protected AITarget aiTargetCollider;
    public Transform AiTarget
    {   
        get 
        {
            if (!aiTargetCollider)
                return transform;
            return aiTargetCollider.transform;
        }
    }


    [Header("Physics")]

    [SerializeField]
    protected GameObject meshBase;

    [SerializeField]
    protected PlayerIK playerIK;
    public PlayerIK PlayerIK => playerIK;

    [SerializeField]
    private float deathKnockbackForceMultiplier = 10;


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

    protected GunController gunController;
    public GunController GunController => gunController;

    protected HealthController healthController;

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
        aiTargetCollider = Instantiate(aiTarget).GetComponent<AITarget>();
        aiTargetCollider.Owner = this;
    }

    private void Update()
    {
        if (GunHolder)
            GunHolder.transform.forward = inputManager.transform.forward;
    }

    void OnDamageTaken(HealthController healthController, float damage, DamageInfo info)
    {
        if (hudController)
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
        aimAssistCollider.SetActive(false);
        TurnIntoRagdoll(info);
        hudController.DisplayDeathScreen(killer.identity);
    }

    protected void TurnIntoRagdoll(DamageInfo info)
    {
        GetComponent<Rigidbody>().GetAccumulatedForce();
        // Disable components
        GetComponent<PlayerMovement>().enabled = false;
        healthController.enabled = false;
        playerIK.enabled = false;
        // TODO display guns falling to the floor
        gunController.gameObject.SetActive(false);
        GunHolder.gameObject.SetActive(false);
        // Disable all colliders and physics
        // Ragdollify

        // TODO: Make accurate hitbox forces for the different limbs of the player

        var ragdollController = GetComponent<RagdollController>();
        if (inputManager)
            ragdollController.ReparentCamera(inputManager.transform);

        var force = info.force.normalized * info.damage * deathKnockbackForceMultiplier;
        ragdollController.EnableRagdoll(force);
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
        var playerMovement = GetComponent<PlayerMovement>();
        playerMovement.SetPlayerInput(inputManager);
        playerMovement.onMove += UpdateHudOnMove;
        // Subscribe relevant input events
        inputManager.onFirePerformed += Fire;
        inputManager.onFireCanceled += FireEnd;
        inputManager.onSelect += TryPlaceBid;
        inputManager.onFirePerformed += TryPlaceBid;
        inputManager.onInteract += Interact;
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
            gunController.onFireStart -= UpdateAimTarget;
            gunController.onFire -= UpdateAimTarget;
            gunController.onFireEnd -= UpdateHudFire;
            gunController.onReload -= UpdateHudReload;
            //Remove the gun
            Destroy(gunController.gameObject);
        }
        if (TryGetComponent(out PlayerMovement playerMovement))
        {
            playerMovement.onMove -= UpdateHudOnMove;
        }

    }

    private void UpdateAimTarget(GunStats stats)
    {
        if (overrideAimTarget)
            return;
        Vector3 cameraCenter = inputManager.transform.position;
        Vector3 cameraDirection = inputManager.transform.forward;
        Vector3 startPoint = cameraCenter + cameraDirection * targetStartOffset;
        if (Physics.Raycast(cameraCenter, cameraDirection, out RaycastHit hitInfo, targetStartOffset, defaultLayer))
        {
            gunController.target = hitInfo.point;
            GunController.TargetIsTooClose = true;
            return;
        }
        GunController.TargetIsTooClose = false;
        if (Physics.Raycast(startPoint, cameraDirection, out RaycastHit hit, maxHitDistance, hitMask))
        {
            gunController.target = hit.point;
        }
        else
        {
            gunController.target = cameraCenter + cameraDirection * maxHitDistance;
        }
    }

    private void Interact(InputAction.CallbackContext ctx)
    {
        if (!Physics.Raycast(inputManager.transform.position, inputManager.transform.forward, out var hit,
                maxHitDistance, interactMask))
            return;

        if (!hit.transform.TryGetComponent<Interactable>(out var interactable))
            return;

        interactable.Interact(this);
    }

    private void Fire(InputAction.CallbackContext ctx)
    {
        if (!gunController)
            return;
        gunController.triggerHeld = true;
        gunController.triggerPressed = true;
        StartCoroutine(UnpressTrigger());
    }

    private void UpdateHudOnMove(Rigidbody body)
    {
        hudController.SetSpeedLines(body.velocity);
    }

    private void UpdateHudFire(GunStats stats)
    {
        // stats variables must be dereferenced
        float ammo = stats.Ammo;
        float magazine = stats.MagazineSize;
        hudController.UpdateOnFire(ammo / magazine);
    }

    private void UpdateHudReload(GunStats stats)
    {
        float ammo = stats.Ammo;
        float magazine = stats.MagazineSize;
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

    public virtual void SetLayer(int playerIndex)
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

    public virtual void SetGun(Transform offset)
    {
        overrideAimTarget = false;
        var gun = GunFactory.InstantiateGun(identity.Body, identity.Barrel, identity?.Extension, this, offset);
        // Set specific local transform
        gun.transform.localPosition = new Vector3(0.39f, -0.34f, 0.5f);
        gun.transform.localRotation = Quaternion.AngleAxis(0.5f, Vector3.up);
        // Remember gun controller
        gunController = gun.GetComponent<GunController>();
        gunController.onFireStart += UpdateAimTarget;
        gunController.onFire += UpdateAimTarget;
        gunController.onFireEnd += UpdateHudFire;
        gunController.onReload += UpdateHudReload;
        playerIK.LeftHandIKTarget = gunController.LeftHandTarget;
        if (gunController.RightHandTarget)
            playerIK.RightHandIKTarget = gunController.RightHandTarget;
        GetComponent<AmmoBoxCollector>().CheckForAmmoBoxBodyAgain();
    }

    public void RemoveGun()
    {
        for (int i = gunController.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(gunController.transform.GetChild(i).gameObject);
        }
        Destroy(gunController.gameObject);
        for (int i = GunOrigin.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(GunOrigin.transform.GetChild(i).gameObject);
        }
    }

    protected void SetLayerOnSubtree(GameObject node, int layer)
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
        if (Random.Range(0, 10000) > 5)
        {
            hitSounds.Play(audioSource);
        }
        else
        {
            extraHitSounds.Play(audioSource);
        }
    }
}
