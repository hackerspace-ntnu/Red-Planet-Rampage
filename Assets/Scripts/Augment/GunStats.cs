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
        semiAuto,
        burst,
        fullAuto
    }
    // Projectiles per second
    [SerializeField]
    private ModifiableFloat firerate = new ModifiableFloat(5f);
    public ModifiableFloat Firerate { get => firerate; }

    // Time to reload gun
    [SerializeField]
    private ModifiableFloat reloadTime = new ModifiableFloat(3f);
    public ModifiableFloat ReloadTime { get => reloadTime; }


    //How many projectiles in a clip
    public int magazineSize = 20;

    public int Ammo = 20;

    // Damage of each projectile
    [SerializeField]
    private ModifiableFloat projectileDamage = new ModifiableFloat(10f);
    public ModifiableFloat ProjectileDamage { get => projectileDamage; }


    // Projectile initial velocity in in-game units
    [SerializeField]
    private ModifiableFloat projectileSpeedFactor = new ModifiableFloat(1f);
    public ModifiableFloat ProjectileSpeedFactor { get => projectileSpeedFactor; }


    //How much the projectile is affected by gravity
    [SerializeField]
    private ModifiableFloat projectileGravityModifier = new ModifiableFloat(1f);
    public ModifiableFloat ProjectileGravityModifier { get => projectileGravityModifier; }


    // Recoil in radian units
    [SerializeField]
    private ModifiableFloat recoil = new ModifiableFloat(0f);
    public ModifiableFloat Recoil { get => recoil; }


    // Spread of projectiles in radian units
    [SerializeField]
    private ModifiableFloat projectileSpread = new ModifiableFloat(0f);
    public ModifiableFloat ProjectileSpread { get => projectileSpread; }


    // Number of projectiles fired per input
    public FireModes fireMode = FireModes.semiAuto;

    //Number of projectiles fired in a single burst if firemode is BurstMode
    public int burstNum = 1;


    // How large the projectile hitbox is, for 0 it will be a ray
    [SerializeField]
    private ModifiableFloat projectileSize = new ModifiableFloat(0f);
    public ModifiableFloat ProjectileSize { get => projectileSize; }


    // How to scale the projectile model 
    [SerializeField]
    private ModifiableFloat projectileScale = new ModifiableFloat(1f);
    public ModifiableFloat ProjectileScale { get => projectileScale; }


    // How much extra damage a crit does, the standard is a crid does double damage
    [SerializeField]
    private ModifiableFloat criticalMultiplier = new ModifiableFloat(2f);
    public ModifiableFloat CriticalMultiplier { get => criticalMultiplier; }


    // TODO: make modifyableInteger
    // Used for shotguns
    [SerializeField]
    private ModifiableFloat projectilesPerShot = new ModifiableFloat(1f);
    public ModifiableFloat ProjectilesPerShot { get => projectilesPerShot; }

}
