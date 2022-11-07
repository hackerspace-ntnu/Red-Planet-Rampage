using UnityEngine;

[CreateAssetMenu(fileName = "GunStats", menuName = "ScriptableObjects/GunStats", order = 1)]
public class GunStats: ScriptableObject
{

    // -- IMPORTANT INFO --

    // It is fine to add new properties to this file, but be carefull when REMOVING them
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
    private ModifyableFloat firerate = new ModifyableFloat(5f);
    public ModifyableFloat Firerate { get => firerate;}

    // Time to reload gun
    [SerializeField]
    private ModifyableFloat reloadTime = new ModifyableFloat(3f);
    public ModifyableFloat ReloadTime { get => reloadTime; }

    
    //How many projectiles in a clip
    public int magazineSize = 20;

    // Damage of each projectile
    [SerializeField]
    private ModifyableFloat projectileDamage = new ModifyableFloat(10f);
    public ModifyableFloat ProjectileDamage { get => projectileDamage; }


    // Projectile initial velocity in in-game units
    [SerializeField]
    private ModifyableFloat projectileSpeed = new ModifyableFloat(1f);
    public ModifyableFloat ProjectileSpeed { get => projectileSpeed; }


    //How much the projectile is affected by gravity
    [SerializeField]
    private ModifyableFloat projectileGravityModifier = new ModifyableFloat(1f);
    public ModifyableFloat ProjectileGravityModifier { get => projectileGravityModifier; }


    // Recoil in radian units
    [SerializeField]
    private ModifyableFloat recoil = new ModifyableFloat(0f);
    public ModifyableFloat Recoil { get => recoil; }


    // Spread of projectiles in radian units
    [SerializeField]
    private ModifyableFloat projectileSpread = new ModifyableFloat(0f);
    public ModifyableFloat ProjectileSpread { get => projectileSpread; }


    // Number of projectiles fired per input
    public FireModes fireMode = FireModes.semiAuto;

    //Number of projectiles fired in a single burst if firemode is BurstMode
    public int burstNum = 1;


    // How large the projectile hitbox is, for 0 it will be a ray
    [SerializeField]
    private ModifyableFloat projectileSize = new ModifyableFloat(0f);
    public ModifyableFloat ProjectileSize { get => projectileSize; }

    
    // How to scale the projectile model 
    [SerializeField]
    private ModifyableFloat projectileScale = new ModifyableFloat(1f);
    public ModifyableFloat ProjectileScale { get => projectileScale; }


    // How much extra damage a crit does, the standard is a crid does double damage
    [SerializeField]
    private ModifyableFloat criticalMultiplier = new ModifyableFloat(2f);
    public ModifyableFloat CriticalMultiplier { get => criticalMultiplier; }


    // TODO: make modifyableInteger
    // Used for shotguns
    [SerializeField]
    private ModifyableFloat projectilesPerShot = new ModifyableFloat(1f);
    public ModifyableFloat ProjectilesPerShot { get => projectilesPerShot; }

}
