using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SecretName;

public class PlayerStatUI : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup statContainer;

    [SerializeField]
    private TMP_Text playerNameText;

    [SerializeField]
    private TMP_Text chipsText;

    public string gunNameText { get; private set; }

    [SerializeField]
    private RectTransform gunPreviewPanel;

    private GameObject gunPreviewGameObject;

    [SerializeField]
    private Chip chip;

    [SerializeField]
    private StatBar damageBar;

    [SerializeField]
    private StatBar fireRateBar;

    [SerializeField]
    private StatBar projectilesPerShotBar;

    [SerializeField]
    private StatBar projectileSpeedBar;

    private Outline outline;

    private PlayerManager playerManager;

    [SerializeField]
    private float gunPreviewScale;

    private float gunPreviewPositionZ = -1f;

    public PlayerManager PlayerManager
    {
        get => playerManager;
        set
        {
            playerManager = value;
            Init();
        }
    }

    private void Init()
    {
        if (!playerManager)
        {
            return;
        }

        statContainer.alpha = 1;
        outline = GetComponent<Outline>();

        SetName(playerManager.identity.playerName);
        SetColor(playerManager.identity.color);

        SetChips(playerManager.identity.chips);
        playerManager.identity.onChipChange += SetChips;
        playerManager.identity.onChipSpent += AnimateTransaction;
        playerManager.identity.onChipGain += AnimateTransaction;

        SetGunName(playerManager.GetGunName());
        gunPreviewGameObject = GunFactory.InstantiateGun(playerManager.identity.Body, playerManager.identity.Barrel, playerManager.identity.Extension, null, gunPreviewPanel);
        gunPreviewGameObject.transform.Rotate(new Vector3(0f, 90f));
        gunPreviewGameObject.transform.localScale = new Vector3(gunPreviewScale, gunPreviewScale, gunPreviewScale);
        gunPreviewGameObject.transform.Translate(new Vector3(0f, 0f, gunPreviewPositionZ));

        // Set current stats
        OnInventoryChange(null);
        ResetNewGunStats();
        // Respond to stat changes
        playerManager.identity.onInventoryChange += OnInventoryChange;
        playerManager.onSelectedBiddingPlatformChange += OnBiddingPlatformChange;

    }

    void OnDisable()
    {
        // Hide stats
        statContainer.alpha = 0;
    }

    void OnDestroy()
    {
        if (!playerManager)
        {
            return;
        }

        playerManager.identity.onChipChange -= SetChips;
        playerManager.identity.onChipSpent -= AnimateTransaction;
        playerManager.identity.onChipGain -= AnimateTransaction;
        playerManager.identity.onInventoryChange -= OnInventoryChange;
        playerManager.onSelectedBiddingPlatformChange -= OnBiddingPlatformChange;
    }

    private void AnimateTransaction(int amount)
    {
        if (chip == null)
            return;
        chip.AnimateTransaction(amount, playerManager.SelectedBiddingPlatform.transform);
    }

    private void OnInventoryChange(Item item)
    {
        SetBaseGunStats(GunFactory.GetGunStats(playerManager.identity.Body, playerManager.identity.Barrel, playerManager.identity.Extension));
    }

    private void OnBiddingPlatformChange(BiddingPlatform platform)
    {
        Item body = playerManager.identity.Body;
        Item barrel = playerManager.identity.Barrel;
        Item extension = playerManager.identity.Extension;

        if (platform == null || platform.Item == null)
        {
            ResetNewGunStats();
            SetGunName(playerManager.GetGunName());
            SetGunPreview(body, barrel, extension);
            return;
        }

        switch (platform.Item.augmentType)
        {
            case AugmentType.Body:
                body = platform.Item;
                break;
            case AugmentType.Barrel:
                barrel = platform.Item;
                break;
            case AugmentType.Extension:
                extension = platform.Item;
                break;
            default:
                Debug.Log($"No appropritate augmentType ({platform.Item.augmentType}) found in item.");
                break;
        }
        GunStats stats = GunFactory.GetGunStats(body, barrel, extension);
        SetNewGunStats(stats);
        SetGunName(GunFactory.GetGunName(body, barrel, extension));
        SetGunPreview(body, barrel, extension);
    }

    public void SetName(string name)
    {
        playerNameText.SetText(name);
    }

    public void SetColor(Color color)
    {
        outline.effectColor = color;
    }

    public void SetChips(int amount)
    {
        chipsText.SetText(amount.ToString());
    }

    public void SetGunName(string name)
    {
        gunNameText = name;
    }

    public void SetGunPreview(Item body, Item barrel, Item extension)
    {
        GunFactory gunFactory = gunPreviewGameObject.GetComponent<GunFactory>();
        gunFactory.Body = body;
        gunFactory.Barrel = barrel;
        gunFactory.Extension = extension;
        gunFactory.InitializeGun();
    }

    public void SetBaseGunStats(GunStats gunStats)
    {
        // TODO Change the set of stats every time (?)
        damageBar.BaseValue = gunStats.ProjectileDamage;
        fireRateBar.BaseValue = gunStats.Firerate;
        projectilesPerShotBar.BaseValue = gunStats.ProjectilesPerShot;
        projectileSpeedBar.BaseValue = gunStats.ProjectileSpeedFactor;
    }

    public void SetNewGunStats(GunStats gunStats)
    {
        damageBar.NewValue = gunStats.ProjectileDamage;
        fireRateBar.NewValue = gunStats.Firerate;
        projectilesPerShotBar.NewValue = gunStats.ProjectilesPerShot;
        projectileSpeedBar.NewValue = gunStats.ProjectileSpeedFactor;
    }

    public void ResetNewGunStats()
    {
        damageBar.NewValue = damageBar.BaseValue;
        fireRateBar.NewValue = fireRateBar.BaseValue;
        projectilesPerShotBar.NewValue = projectilesPerShotBar.BaseValue;
        projectileSpeedBar.NewValue = projectileSpeedBar.BaseValue;
    }
}
