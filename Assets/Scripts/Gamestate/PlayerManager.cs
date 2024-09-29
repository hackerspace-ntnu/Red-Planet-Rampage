using Mirror;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(AudioSource))]
public class PlayerManager : NetworkBehaviour
{
    [SyncVar]
    public uint id;

    // Layers 12 through 15 are gun layers.
    protected static int allGunsMask = (1 << 12) | (1 << 13) | (1 << 14) | (1 << 15);

    // Only Default and HitBox layers can be hit
    private static int hitMask = 1 | (1 << 3);

    private const int defaultLayer = 1;
    public int HitMask => hitMask;

    private int layerIndex;
    public int LayerIndex => layerIndex;

    [SerializeField]
    private LayerMask interactMask;

    private bool isAlive = true;
    public bool IsAlive => isAlive;

    [Header("Shooting")]

    [SerializeField]
    private float maxHitDistance = 100;

    [SerializeField]
    private float targetStartOffset = 0.28f;
    public float TargetStartOffset => targetStartOffset;
    public bool overrideAimTarget = false;

    // TODO add context when shooty system is done
    public delegate void HitEvent(PlayerManager killer, PlayerManager victim, DamageInfo info);

    public HitEvent onDeath;
    public HitEvent onKill;

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

    private int screenShakeTween;

    [Header("Related objects")]

    public InputManager inputManager;
    public PlayerIdentity identity;

    [SerializeField]
    private PlayerHUDController hudController;
    public PlayerHUDController HUDController => hudController;

    [SerializeField]
    private DecalProjector playerShadow;

    [SerializeField]
    protected GameObject aiTarget;
    protected AITarget aiTargetCollider;
    public Transform AiAimSpot;
    public Transform AiTarget
    {
        get
        {
            if (!aiTargetCollider)
                return transform;
            return aiTargetCollider.transform;
        }
    }

    protected GunController gunController;
    public GunController GunController => gunController;

    protected HealthController healthController;

    [Header("Physics")]

    [SerializeField]
    protected GameObject[] meshBase;

    [SerializeField]
    protected PlayerIK playerIK;
    public PlayerIK PlayerIK => playerIK;

    [SerializeField]
    private float deathKnockbackForceMultiplier = 10;

    [Header("Auction")]

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

    [Header("Movement sounds")]

    protected AudioSource audioSource;

    [SerializeField]
    private AudioGroup jumpSounds;

    [SerializeField]
    private AudioGroup leapSounds;

    private PlayerMovement movement;

    private void Start()
    {
        healthController = GetComponent<HealthController>();
        healthController.onDamageTaken += OnDamageTaken;
        healthController.onDeath += OnDeath;
        audioSource = GetComponent<AudioSource>();
        aiTargetCollider = Instantiate(aiTarget).GetComponent<AITarget>();
        aiTargetCollider.Owner = this;
        aiTargetCollider.transform.position = transform.position;
        movement = GetComponent<PlayerMovement>();

        // TODO call other stuff that sets up player object!
    }

    private void Update()
    {
        // TODO Do aiming in a different component please
        if (GunHolder && inputManager)
        {
            GunHolder.transform.forward = inputManager.transform.forward;
            UpdateAimAngleCmd(movement.AimAngle.y);
        }
    }

    // Hijinks for syncing aim from server to players
    [Command]
    private void UpdateAimAngleCmd(float yAngle)
    {
        UpdateAimAngleRpc(yAngle);
    }

    [ClientRpc]
    private void UpdateAimAngleRpc(float yAngle)
    {
        // Avoid updating if this is a local player
        if (inputManager)
            return;
        GunHolder.transform.localRotation = Quaternion.AngleAxis(yAngle * Mathf.Rad2Deg, Vector3.left);
        // Same-ish location as GunHolder for the real gun
        if (gunController)
            gunController.transform.parent.localRotation = Quaternion.AngleAxis(yAngle * Mathf.Rad2Deg, Vector3.left);
    }

    private void OnDamageTaken(HealthController healthController, float damage, DamageInfo info)
    {
        if (hudController)
            hudController.OnDamageTaken(damage, healthController.CurrentHealth, healthController.MaxHealth);
        if (info.sourcePlayer != this)
        {
            lastPlayerThatHitMe = info.sourcePlayer;
        }
    }

    protected virtual void OnDeath(HealthController healthController, float damage, DamageInfo info)
    {
        var killer = info.sourcePlayer;
        if (info.sourcePlayer == this && lastPlayerThatHitMe)
        {
            killer = lastPlayerThatHitMe;
        }
        if (!isAlive)
            return;

        EventLog.Singleton.Log(info.DeathToString(this));

        onDeath?.Invoke(killer, this, info);
        isAlive = false;
        aimAssistCollider.SetActive(false);
        TurnIntoRagdoll(info);
        aiTargetCollider.gameObject.SetActive(false);
        if (hudController)
            hudController.DisplayDeathScreen(killer.identity);
        if (playerShadow)
            playerShadow.gameObject.SetActive(false);
    }

    protected void TurnIntoRagdoll(DamageInfo info)
    {
        // Disable components
        GetComponent<PlayerMovement>().enabled = false;
        healthController.enabled = false;
        playerIK.enabled = false;
        // TODO display guns falling to the floor
        if (gunController)
            gunController.gameObject.SetActive(false);
        GunHolder.gameObject.SetActive(false);

        if (inputManager)
        {
            var orbitCamera = GetComponent<OrbitCamera>();
            orbitCamera.Activate(inputManager);
        }

        // TODO: Make accurate hitbox forces for the different limbs of the player
        var ragdollController = GetComponent<RagdollController>();
        var force = info.force.normalized * Mathf.Log(info.damage) * deathKnockbackForceMultiplier;
        ragdollController.EnableRagdoll(force);
    }

    /// <summary>
    /// Function for setting a playerInput, adding movement related listeners to it
    /// and performing other necessary operations that require playerInput/-Identity.
    /// </summary>
    /// <param name="playerInput"></param>
    public void SetPlayerInput(InputManager playerInput)
    {
        ListenToPlayerInput(playerInput);
        identity = inputManager.GetComponent<PlayerIdentity>();
        var playerMovement = GetComponent<PlayerMovement>();
        playerMovement.enabled = true;
        playerMovement.SetPlayerInput(inputManager);
        playerMovement.OnMove += UpdateHudOnMove;
        playerMovement.OnJumpPerformed += UpdateHudOnJump;
        playerMovement.OnJumpPerformed += PlayJumpAudio;
        playerMovement.OnLeapPerformed += PlayLeapAudio;
        // Set camera on canvas
        if (hudController)
        {
            var canvas = hudController.GetComponent<Canvas>();
            canvas.worldCamera = inputManager.GetComponentInChildren<Camera>();
            canvas.planeDistance = 0.21f;
            identity.onChipChange += hudController.OnChipChange;
        }

        if (playerIK.TryGetComponent<BiddingPlayer>(out var biddingPlayer))
        {
            biddingPlayer.SetPlayerInput();
        }
    }

    private void ListenToPlayerInput(InputManager playerInput)
    {
        inputManager = playerInput;
        // Subscribe relevant input events
        inputManager.onFirePerformed += Fire;
        inputManager.onFireCanceled += FireEnd;
        inputManager.onSelect += TryPlaceBid;
        inputManager.onFirePerformed += TryPlaceBid;
        inputManager.onInteract += Interact;
    }

    public void ReassignPlayerInput(InputManager playerInput)
    {
        ListenToPlayerInput(playerInput);

        var playerMovement = GetComponent<PlayerMovement>();
        playerMovement.ReassignPlayerInput(inputManager);
        if (gunController)
            gunController.SetPlayerInput(playerInput);
    }

    public void ApplyIdentity()
    {
        // Set player color
        meshBase.ToList().ForEach(mesh => mesh.GetComponentInChildren<SkinnedMeshRenderer>().material.color = identity.color);

        var voicePlayer = GetComponent<VoicePlayer>();
        voicePlayer.Voice = PlayerInputManagerController.Singleton.VoiceForColor(identity.color);
        var isLocalPlayer = Peer2PeerTransport.PlayerDetails.FirstOrDefault(p => p.id == id).type is PlayerType.Local;
        if (isLocalPlayer)
            voicePlayer.Turn2D();

        if (playerIK.TryGetComponent<BiddingPlayer>(out var biddingPlayer))
        {
            // TODO refactor identity subscriptions
            biddingPlayer.SetIdentity();
        }
    }

    void OnDestroy()
    {
        if (healthController)
        {
            healthController.onDamageTaken -= OnDamageTaken;
            healthController.onDeath -= OnDeath;
        }
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
            playerMovement.OnMove -= UpdateHudOnMove;
            playerMovement.OnJumpPerformed -= UpdateHudOnJump;
            playerMovement.OnJumpPerformed -= PlayJumpAudio;
            playerMovement.OnLeapPerformed -= PlayLeapAudio;
        }
        if (hudController)
            identity.onChipChange -= hudController.OnChipChange;
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
        if (hudController)
            hudController.SetSpeedLines(body.velocity);
    }

    private void UpdateHudOnJump(Rigidbody body)
    {
        if (hudController)
            hudController.AnimateHudJump();
    }

    private void PlayJumpAudio(Rigidbody body)
    {
        jumpSounds.Play(audioSource);
    }

    private void PlayLeapAudio(Rigidbody body)
    {
        leapSounds.Play(audioSource);
    }

    private void UpdateHudFire(GunStats stats)
    {
        // stats variables must be dereferenced
        float ammo = stats.Ammo < 1 ? 0 : stats.Ammo;
        float magazine = stats.MagazineSize;
        hudController.UpdateOnFire(ammo / magazine);
    }

    private void UpdateHudReload(GunStats stats)
    {
        float ammo = stats.Ammo;
        float magazine = stats.MagazineSize;
        hudController.UpdateOnReload(ammo / magazine);
    }

    private void UpdateHudCrosshair(GunStats stats)
    {
        HUDController.UpdateOnInitialize(stats);
    }

    private void TryPlaceBid(InputAction.CallbackContext ctx)
    {
        if (!selectedBiddingPlatform) return;
        selectedBiddingPlatform.PlaceBid(identity);
    }

    protected IEnumerator UnpressTrigger()
    {
        yield return new WaitForFixedUpdate();
        gunController.triggerPressed = false;
    }

    private void FireEnd(InputAction.CallbackContext ctx)
    {
        if (!gunController)
            return;
        gunController.triggerHeld = false;
    }

    private void ScreenShake(GunStats stats)
    {
        if (!inputManager)
            return;
        if (LeanTween.isTweening(screenShakeTween))
        {
            LeanTween.cancel(screenShakeTween);
            inputManager.PlayerCamera.gameObject.transform.localPosition = Vector3.zero;
        }
        screenShakeTween = inputManager.PlayerCamera.gameObject.LeanMoveLocal(new Vector3(0.02f, 0.02f, 0f) * stats.ScreenShakeFactor, 0.1f).setEaseShake().id;
    }

    public virtual void SetLayer(int playerIndex)
    {
        layerIndex = playerIndex;
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

        if (inputManager)
            inputManager.PlayerCamera.cullingMask = negatedMask;

        // Set correct layer on self, mesh and gun (TODO)
        gameObject.layer = playerLayer;
        meshBase.ToList().ForEach(mesh => SetLayerOnSubtree(mesh, playerLayer));
        if (hudController)
            SetLayerOnSubtree(hudController.gameObject, LayerMask.NameToLayer("Gun " + playerIndex));
    }

    public virtual void SetGun(Transform offset)
    {
        var hadGunBefore = gunController != null;
        overrideAimTarget = false;
        var gun = GunFactory.InstantiateGun(identity.Body, identity.Barrel, identity.Extension, this, offset);
        // Set specific local transform
        gun.transform.localPosition = new Vector3(0.39f, -0.34f, 0.5f);
        gun.transform.localRotation = Quaternion.AngleAxis(0.5f, Vector3.up);
        // Remember gun controller
        gunController = gun.GetComponent<GunController>();
        if (inputManager)
        {
            gunController.onFireStart += UpdateAimTarget;
            gunController.onFire += UpdateAimTarget;
            gunController.onFireEnd += ScreenShake;
            gunController.onFireEnd += UpdateHudFire;
            gunController.onReload += UpdateHudReload;
            UpdateHudCrosshair(gunController.stats);
            gunController.projectile.OnHitboxCollision += hudController.HitAnimation;
        }
        playerIK.LeftHandIKTarget = gunController.LeftHandTarget;
        if (gunController.RightHandTarget)
            playerIK.RightHandIKTarget = gunController.RightHandTarget;
        GetComponent<AmmoBoxCollector>().CheckForAmmoBoxBodyAgain();
        if (hadGunBefore)
            // This is required for the networkbehaviours on the gun to be given an identity and not break
            GetComponent<NetworkIdentity>().InitializeNetworkBehaviours();
    }

    public void SetGunNetwork(Transform offset)
    {
        overrideAimTarget = false;
        var gun = GunFactory.InstantiateGunAI(
             identity.Body,
             identity.Barrel,
             identity.Extension,
            this, offset);
        gunController = gun.GetComponent<GunController>();
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

    private void SetLayerOnSubtree(GameObject node, int layer)
    {
        node.layer = layer;
        foreach (Transform child in node.transform)
        {
            SetLayerOnSubtree(child.gameObject, layer);
        }
    }

    public new string ToString() => identity.ToString();
}
