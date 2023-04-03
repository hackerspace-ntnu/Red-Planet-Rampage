using UnityEngine;

public class GunModifier : MonoBehaviour
{

    // Sets the order the modifiers are applied, for consitency
    public virtual int priority { get => 100; }

    // Adds simple stat modifications
    public Modifier[] statModifiers;

    public ProjectileModifier[] projectileModifiers;

    // This prefab is added as a child to the projectile in order to modify it
    // REMOVED
    //public GameObject bulletModifierPrefab;

    // Used to keep track of added projectile modifier to delete it 
    // private GameObject instantiatedBulletModifier;

    // Where to shoot bullets
    public Transform[] outputs;


    public virtual void Attach(GunController gun)
    {
        Modify(gun.stats);
        foreach (ProjectileModifier modifier in projectileModifiers)
        {
            modifier.Attach(gun.projectile);
        }
    }

    public virtual void Modify(GunStats stats)
    {
        foreach (var modifier in statModifiers)
        {
            ModifiableFloat stat = (ModifiableFloat)typeof(GunStats).GetProperty(modifier.name).GetValue(stats, null);
            stat.AddBaseValue(modifier.addition);
            stat.AddMultiplier(modifier.multiplier);
            stat.AddExponential(modifier.exponential);
        }
        stats.ProjectilesPerShot.AddExponential(outputs.Length);
    }

    public virtual void Detach(GunController gun)
    {
        foreach (var modifier in statModifiers)
        {
            ModifiableFloat stat = (ModifiableFloat)typeof(GunStats).GetProperty(modifier.name).GetValue(gun.stats);
            stat.AddBaseValue(-modifier.addition);
            stat.AddMultiplier(-modifier.multiplier);
            stat.AddExponential(1 / modifier.exponential);
        }
        foreach (ProjectileModifier modifier in projectileModifiers)
        {
            modifier.Detach(gun.projectile);
        }
    }
}
