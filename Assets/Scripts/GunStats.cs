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
    private ModifiableFloat _firerate = new ModifiableFloat(5f);
    public ModifiableFloat firerate { get => _firerate;}

    // Time to reload gun
    [SerializeField]
    private ModifiableFloat _reloadTime = new ModifiableFloat(3f);
    public ModifiableFloat reloadTime { get => _reloadTime; }

    
    //How many projectiles in a clip
    public int magazineSize = 20;

    // Damage of each projectile
    [SerializeField]
    private ModifiableFloat _projectileDamage = new ModifiableFloat(10f);
    public ModifiableFloat projectileDamage { get => _projectileDamage; }


    // Projectile initial velocity in in-game units
    [SerializeField]
    private ModifiableFloat _projectileSpeed = new ModifiableFloat(1f);
    public ModifiableFloat projectileSpeed { get => _projectileSpeed; }


    //How much the projectile is affected by gravity
    [SerializeField]
    private ModifiableFloat _projectileGravityModifier = new ModifiableFloat(1f);
    public ModifiableFloat projectileGravityModifier { get => _projectileGravityModifier; }


    // Recoil in radian units
    [SerializeField]
    private ModifiableFloat _recoil = new ModifiableFloat(0f);
    public ModifiableFloat recoil { get => _recoil; }


    // Spread of projectiles in radian units
    [SerializeField]
    private ModifiableFloat _projectileSpread = new ModifiableFloat(0f);
    public ModifiableFloat projectileSpread { get => _projectileSpread; }


    // Number of projectiles fired per input
    public FireModes fireMode = FireModes.semiAuto;

    //Number of projectiles fired in a single burst if firemode is BurstMode
    public int burstNum = 1;


    // How large the projectile hitbox is, for 0 it will be a ray
    [SerializeField]
    private ModifiableFloat _projectileSize = new ModifiableFloat(0f);
    public ModifiableFloat projectileSize { get => _projectileSize; }

    
    // How to scale the projectile model 
    [SerializeField]
    private ModifiableFloat _projectileScale = new ModifiableFloat(1f);
    public ModifiableFloat projectileScale { get => _projectileScale; }


    // How much extra damage a crit does, the standard is a crid does double damage
    [SerializeField]
    private ModifiableFloat _criticalMultiplier = new ModifiableFloat(2f);
    public ModifiableFloat criticalMultiplier { get => _criticalMultiplier; }


    // TODO: make modifyableInteger
    // Used for shotguns
    [SerializeField]
    private ModifiableFloat _projectilesPerShot = new ModifiableFloat(1f);
    public ModifiableFloat projectilesPerShot { get => _projectilesPerShot; }

}
