using UnityEngine;

[System.Serializable]
public class Modifier
{
    public string name;
    public float addition = 0;
    public float multiplier = 0;
    public float exponential = 1;
}
public class GunModifier : MonoBehaviour
{

    // Sets the order the modifiers are applied, for consitency
    public virtual int priority { get => 100; }

    // Adds simple stat modifications
    public Modifier[] statModifiers;

    // This prefab is added as a child to the projectile in order to modify it
    public GameObject bulletModifierPrefab;

    // Used to keep track of added projectile modfyer to delete it 
    private GameObject instantiatedBulletModifier;

    // Where to shoot bullets
    public Transform[] outputs;

    // Is run when the gun is built
    public virtual void Attach(GunController gun)
    {
        Modify(gun.stats);
        if (bulletModifierPrefab != null)
        {
            instantiatedBulletModifier = Instantiate(bulletModifierPrefab, gun.projectile.transform);
            instantiatedBulletModifier.GetComponent<ProjectileModifier>().projectile = gun.projectile.GetComponent<ProjectileController>();
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

    // Is run when component is removed
    // Not currently in use
    public virtual void Detach(GunController gun)
    {
        foreach (var modifier in statModifiers)
        {
            ModifiableFloat stat = (ModifiableFloat)typeof(GunStats).GetProperty(modifier.name).GetValue(gun.stats);
            stat.AddBaseValue(-modifier.addition);
            stat.AddMultiplier(-modifier.multiplier);
            stat.AddExponential(1 / modifier.exponential);
        }
        if (instantiatedBulletModifier != null)
        {
            Destroy(instantiatedBulletModifier);
        }
    }
}
