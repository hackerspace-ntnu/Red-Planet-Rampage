using UnityEngine;
using UnityEngine.VFX;
using System.Linq;
using System.Collections.Generic;
using Mirror;

[System.Serializable]
public class Modifier
{
    public string name;
    public float addition = 0;
    public float multiplier = 0;
    public float exponential = 1;
}
public abstract class Augment : NetworkBehaviour
{
    // Adds simple stat modifications
    public Modifier[] statModifiers;

    // Where to shoot bullets
    public Transform[] outputs;

    // For displaying items with correct alignment
    public Transform midpoint;

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
                foreach (var vfx in instance.GetComponentsInChildren<VisualEffect>())
                {
                    vfx.gameObject.SetActive(false);
                }
                break;
            case AugmentType.Extension:
                instance.GetComponent<GunExtension>().enabled = false;
                break;
        }
    }

    public static Transform Midpoint(GameObject instance, AugmentType type)
    {
        if (type == AugmentType.Body)
        {
            return instance.GetComponent<GunBody>().midpoint;
        }
        else
        {
            return instance.GetComponent<Augment>().midpoint;
        }
    }
}
