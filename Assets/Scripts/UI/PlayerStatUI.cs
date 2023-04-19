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

    [SerializeField]
    private TMP_Text gunNameText;

    [SerializeField]
    private StatBar damageBar;

    [SerializeField]
    private StatBar fireRateBar;

    [SerializeField]
    private StatBar projectilesPerShotBar;

    [SerializeField]
    private StatBar projectileSpeedBar;

    private Outline outline;

    public PlayerManager playerManager;

    void Start()
    {
        outline = GetComponent<Outline>();
        OnEnable();
    }

    void OnEnable()
    {
        if (!playerManager)
        {
            return;
        }

        statContainer.alpha = 1;

        SetName(playerManager.identity.playerName);
        SetColor(playerManager.identity.color);

        SetChips(playerManager.identity.chips);
        playerManager.identity.onChipChange += SetChips;

        SetGunName(playerManager.GetGunName());

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
        playerManager.identity.onInventoryChange -= OnInventoryChange;
        playerManager.onSelectedBiddingPlatformChange -= OnBiddingPlatformChange;
    }

    private void OnInventoryChange(Item item)
    {
        SetBaseGunStats(GunFactory.GetGunStats(playerManager.identity.Body, playerManager.identity.Barrel, playerManager.identity.Extension));
    }

    private void OnBiddingPlatformChange(BiddingPlatform platform)
    {
        if (platform == null || platform.Item == null)
        {
            ResetNewGunStats();
            SetGunName(playerManager.GetGunName());
            return;
        }

        Item body = playerManager.identity.Body;
        Item barrel = playerManager.identity.Barrel;
        Item extension = playerManager.identity.Extension;
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
        gunNameText.SetText(name);
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
