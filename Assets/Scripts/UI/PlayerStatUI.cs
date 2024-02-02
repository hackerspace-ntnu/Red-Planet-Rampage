using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SecretName;

public class PlayerStatUI : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup statContainer;

    [SerializeField]
    private TMP_Text augmentDescription;

    public string gunNameText { get; private set; }

    [SerializeField]
    private StatBar damageBar;

    [SerializeField]
    private StatBar fireRateBar;

    [SerializeField]
    private StatBar projectilesPerShotBar;

    [SerializeField]
    private StatBar projectileSpeedBar;

    private Outline outline;

    private PlayerIdentity playerIdentity;

    public PlayerIdentity PlayerIdentity
    {
        get => playerIdentity;
        set
        {
            playerIdentity = value;
            Init();
        }
    }

    private void Init()
    {
        if (!playerIdentity)
        {
            return;
        }

        statContainer.alpha = 1;
        outline = GetComponent<Outline>();

        SetColor(playerIdentity.color);
        SetGunName(playerIdentity.GetGunName());

        // Set current stats
        OnInventoryChange(null);
        ResetNewGunStats();
        // Respond to stat changes
        playerIdentity.onInventoryChange += OnInventoryChange;
    }

    void OnDestroy()
    {
        if (!playerIdentity)
        {
            return;
        }
        playerIdentity.onInventoryChange -= OnInventoryChange;
    }

    private void OnInventoryChange(Item item)
    {
        SetBaseGunStats(GunFactory.GetGunStats(StaticInfo.Singleton.StartingBody, StaticInfo.Singleton.StartingBarrel, StaticInfo.Singleton.StartingExtension));
    }

    private void OnBiddingPlatformChange(BiddingPlatform platform)
    {
        Item body = playerIdentity.Body;
        Item barrel = playerIdentity.Barrel;
        Item extension = playerIdentity.Extension;

        if (platform == null || platform.Item == null)
        {
            ResetNewGunStats();
            SetGunName(playerIdentity.GetGunName());
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
        UpdateStats();
    }

    public void UpdateStats()
    {
        GunStats stats = GunFactory.GetGunStats(playerIdentity.Body, playerIdentity.Barrel, playerIdentity.Extension);
        SetNewGunStats(stats);
        SetGunName(GunFactory.GetGunName(playerIdentity.Body, playerIdentity.Barrel, playerIdentity.Extension));
    }

    public void SetDescription(string description)
    {
        augmentDescription.SetText(string.IsNullOrEmpty(description) ? "No extension" : description);
    }

    public void SetColor(Color color)
    {
        outline.effectColor = color;
    }

    public void SetGunName(string name)
    {
        gunNameText = name;
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
