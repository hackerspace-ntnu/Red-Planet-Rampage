using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[System.Serializable]
public class Modifier
{
    public string name;
    public float addition = 0;
    public float multiplier = 0;
    public float exponential = 1;
}
public abstract class Augment : MonoBehaviour
{
    // Adds simple stat modifications
    public Modifier[] statModifiers;

    // This prefab is added as a child to the projectile in order to modify it
    public GameObject bulletModifierPrefab;

    // Used to keep track of added projectile modifier to delete it 
    private GameObject instantiatedBulletModifier;

    // All modifier children of current gun
    private List<ProjectileModifier> projectileModifiers = new List<ProjectileModifier>();

    // Where to shoot bullets
    public Transform[] outputs;


    public virtual void BuildStats(GunStats gunStats)
    {
        foreach (var modifier in statModifiers)
        {
            ModifiableFloat stat = (ModifiableFloat)typeof(GunStats).GetProperty(modifier.name).GetValue(gunStats, null);
            stat.AddBaseValue(modifier.addition);
            stat.AddMultiplier(modifier.multiplier);
            stat.AddExponential(modifier.exponential);
        }
        gunStats.ProjectilesPerShot.AddExponential(outputs.Length);
    }

    public List<ProjectileModifier> GetModifiers()
    {
        if (bulletModifierPrefab == null)
            return new List<ProjectileModifier>();

        if (instantiatedBulletModifier) 
            return projectileModifiers;

        instantiatedBulletModifier = Instantiate(bulletModifierPrefab, transform);
        return instantiatedBulletModifier.GetComponents<ProjectileModifier>().ToList();
    }

}
