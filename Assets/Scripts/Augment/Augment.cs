using UnityEngine;
using UnityEngine.VFX;
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

    // Where to shoot bullets
    public Transform[] outputs;

    public virtual void BuildStats(GunStats gunStats)
    {
        foreach (var modifier in statModifiers)
        {
            try
            {
                ModifiableFloat stat = (ModifiableFloat)typeof(GunStats).GetProperty(modifier.name).GetValue(gunStats, null);
                stat.AddBaseValue(modifier.addition);
                stat.AddMultiplier(modifier.multiplier);
                stat.AddExponential(modifier.exponential);
            }
            catch (System.NullReferenceException)
            {
                Debug.LogError("No modifier property named: " + modifier.name);
            }
        }
        gunStats.ProjectilesPerShot.AddExponential(outputs.Length);
    }

    public List<ProjectileModifier> GetModifiers()
    {
        return GetComponents<ProjectileModifier>().ToList();
    }

    public static void DisableInstance(GameObject instance, AugmentType type)
    {
        switch (type)
        {
            case AugmentType.Body:
                instance.GetComponent<GunBody>().enabled = false;
                break;
            case AugmentType.Barrel:
                instance.GetComponent<ProjectileController>().enabled = false;
                instance.GetComponentsInChildren<VisualEffect>().Select(vfx => vfx.enabled = false);
                break;
            case AugmentType.Extension:
                instance.GetComponent<GunExtension>().enabled = false;
                break;
        }
    }
}
