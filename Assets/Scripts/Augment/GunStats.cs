using UnityEngine;

[CreateAssetMenu(fileName = "GunStats", menuName = "Augments/New GunStats")]
public class GunStats : ScriptableObject
{

    // -- IMPORTANT INFO --

    // It is fine to add new properties to this file, but be careful when REMOVING them
    // If you remove, for example, magazineSize from these properties and then save, all object data of magazine size 
    // will be removed in every instance of the sriptable object
    public enum FireModes
    {
        SemiAuto,
        Burst,
        FullAuto
    }

    [SerializeField]
    [Tooltip("Projectiles per second")]
    private ModifiableFloat firerate = new ModifiableFloat(5f);
    /// <summary>
    /// Projectiles per second
    /// </summary>
    public ModifiableFloat Firerate => firerate;

    [SerializeField]
    [Tooltip("The time it takes to reload the gun")]
    private ModifiableFloat reloadTime = new ModifiableFloat(3f);
    /// <summary>
    /// The time it takes to reload the gun
    /// </summary>
    public ModifiableFloat ReloadTime => reloadTime;


    [SerializeField]
    [Tooltip("Number of projectiles in a clip")]
    private ModifiableFloat magazine = new ModifiableFloat(20f);
    /// <summary>
    /// Number of projectiles in a clip
    /// </summary>
    public ModifiableFloat Magazine => magazine;

    /// <summary>
    /// Discretized magazine size
    /// </summary>
    [HideInInspector]
    public int MagazineSize => Mathf.Max(1, Mathf.RoundToInt(magazine.Value()));

    /// <summary>
    /// Current amount of ammo
    /// </summary>
    [HideInInspector]
    public int Ammo = 0;

    [SerializeField]
    [Tooltip("Damage of each projectile")]
    private ModifiableFloat projectileDamage = new ModifiableFloat(10f);
    /// <summary>
    /// Damage of each projectile
    /// </summary>
    public ModifiableFloat ProjectileDamage => projectileDamage;


    [SerializeField]
    [Tooltip("Initial projectile velocity in in-game units")]
    private ModifiableFloat projectileSpeedFactor = new ModifiableFloat(1f);
    /// <summary>
    /// Initial projectile velocity in in-game units
    /// </summary>
    public ModifiableFloat ProjectileSpeedFactor => projectileSpeedFactor;


    [SerializeField]
    [Tooltip("How much the projectile is affected by gravity")]
    private ModifiableFloat projectileGravityModifier = new ModifiableFloat(1f);
    /// <summary>
    /// How much the projectile is affected by gravity
    /// </summary>
    public ModifiableFloat ProjectileGravityModifier => projectileGravityModifier;


    [SerializeField]
    [Tooltip("Recoil in radians")]
    private ModifiableFloat recoil = new ModifiableFloat(0f);
    /// <summary>
    /// Recoil in radians
    /// </summary>
    public ModifiableFloat Recoil => recoil;


    [SerializeField]
    [Tooltip("Spread of projectiles in radians")]
    private ModifiableFloat projectileSpread = new ModifiableFloat(0f);
    /// <summary>
    /// Spread of projectiles in radians
    /// </summary>
    public ModifiableFloat ProjectileSpread => projectileSpread;


    /// <summary>
    /// Fire mode
    /// - Full Auto: Hold button/trigger to shoot continuously.
    /// - Semi Auto: You need to press for each shot.
    /// - Burst: Not used. Coil barrel has burst fire through its animation.
    /// </summary>
    [Tooltip("Fire mode")]
    public FireModes fireMode = FireModes.SemiAuto;

    /// <summary>
    /// Number of projectiles fired in a single burst if firemode is Burst
    /// </summary>
    [Tooltip("Number of projectiles fired in a single burst if firemode is Burst")]
    public int burstNum = 1;


    [SerializeField]
    [Tooltip("How large the projectile hitbox is, for 0 it will be a ray")]
    private ModifiableFloat projectileSize = new ModifiableFloat(0f);
    /// <summary>
    /// How large the projectile hitbox is, for 0 it will be a ray
    /// </summary>
    public ModifiableFloat ProjectileSize => projectileSize;

    [SerializeField]
    [Tooltip("How to scale the projectile model")]
    private ModifiableFloat projectileScale = new ModifiableFloat(1f);
    /// <summary>
    /// How to scale the projectile model
    /// </summary>
    public ModifiableFloat ProjectileScale => projectileScale;

    [SerializeField]
    [Tooltip("How much damage a critical hit does, should be >= 1")]
    private ModifiableFloat criticalMultiplier = new ModifiableFloat(2f);
    /// <summary>
    /// How much damage a critical hit does, should be >= 1
    /// </summary>
    public ModifiableFloat CriticalMultiplier => criticalMultiplier;


    [SerializeField]
    [Tooltip("Number of projectiles fired per shot")]
    private ModifiableFloat projectilesPerShot = new ModifiableFloat(1f);
    /// <summary>
    /// Number of projectiles fired per shot
    /// </summary>
    public ModifiableFloat ProjectilesPerShot => projectilesPerShot;

    [Header("Visual/non-functional")]
    [SerializeField]
    private ModifiableFloat screenShakeFactor = new ModifiableFloat(1f);
    public ModifiableFloat ScreenShakeFactor => screenShakeFactor;
    [SerializeField]
    private ModifiableFloat crosshairRadius = new ModifiableFloat(1f);
    public ModifiableFloat CrosshairRadius => crosshairRadius;
}
