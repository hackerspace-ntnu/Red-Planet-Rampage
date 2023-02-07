using TMPro;
using UnityEngine;

public class PlayerStatUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text playerNameText;

    [SerializeField]
    private TMP_Text chipsText;

    [SerializeField]
    private StatBar damageBar;

    [SerializeField]
    private StatBar fireRateBar;

    [SerializeField]
    private StatBar projectilesPerShotBar;

    [SerializeField]
    private StatBar projectileSpeedBar;

    public PlayerManager playerManager;

    void Start()
    {
        SetName(playerManager.identity.playerName);

        SetChips(playerManager.identity.chips);
        playerManager.identity.onChipChange += SetChips;

        // Set current stats
        OnInventoryChange(null);
        ResetNewGunStats();
        // Respond to stat changes
        playerManager.identity.onInventoryChange += OnInventoryChange;
        playerManager.onSelectedBiddingPlatformChange += OnBiddingPlatformChange;
    }

    void OnDestroy()
    {
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
    }

    public void SetName(string name)
    {
        playerNameText.SetText(name);
    }

    public void SetChips(int amount)
    {
        chipsText.SetText($"{amount.ToString()} chips");
    }

    public void SetBaseGunStats(GunStats gunStats)
    {
        // TODO Change the set of stats every time (?)
        damageBar.BaseValue = gunStats.ProjectileDamage;
        fireRateBar.BaseValue = gunStats.Firerate;
        projectilesPerShotBar.BaseValue = gunStats.ProjectilesPerShot;
        projectileSpeedBar.BaseValue = gunStats.ProjectileSpeed;
    }

    public void SetNewGunStats(GunStats gunStats)
    {
        damageBar.NewValue = gunStats.ProjectileDamage;
        fireRateBar.NewValue = gunStats.Firerate;
        projectilesPerShotBar.NewValue = gunStats.ProjectilesPerShot;
        projectileSpeedBar.NewValue = gunStats.ProjectileSpeed;
    }

    public void ResetNewGunStats()
    {
        damageBar.NewValue = damageBar.BaseValue;
        fireRateBar.NewValue = fireRateBar.BaseValue;
        projectilesPerShotBar.NewValue = projectilesPerShotBar.BaseValue;
        projectileSpeedBar.NewValue = projectileSpeedBar.BaseValue;
    }
}
