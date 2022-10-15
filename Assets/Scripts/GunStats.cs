using UnityEngine;

[CreateAssetMenu(fileName = "GunBaseStats", menuName = "ScriptableObjects/GunBaseStats", order = 1)]
public class GunBaseStats: ScriptableObject
{
    public enum FireModes
    {
        semiAuto,
        burst,
        fullAuto
    }
    // Bullets per second
    public float firerate = 5f;
    
    // Time to reload gun
    public float reloadTime = 3;

    //How many bullets in a clip
    public int magazineSize = 20;

    // Damage of each bullet
    public float bulletDamage = 10f;

    // Bullet initial velocity in in-game units
    public float bulletSpeed = 10f;

    //How much the bullet is affected by gravity
    public float bulletGravityModifier = 1f;

    // Recoil in radian units
    public float recoil = 0f;

    // Spread of bullets in radian units
    public float bulletSpread = 0f;

    // Number of bullets fired per input
    public FireModes fireMode = FireModes.semiAuto;

    //Number of bullets fired in a single burst if firemode is BurstMode
    public int burstNum = 1;

    public float projectileSize = 0;
}
