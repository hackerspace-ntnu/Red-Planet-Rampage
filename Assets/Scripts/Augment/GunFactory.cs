using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(GunController))]
public class GunFactory : MonoBehaviour
{
    public static GameObject InstantiateGun(Item bodyPrefab, Item barrelPrefab, Item extensionPrefab, Transform parent)
    {
        GameObject gun = Instantiate(new GameObject(), parent);
        GunFactory controller = gun.AddComponent<GunFactory>();
        controller.Body = bodyPrefab;
        controller.Barrel = barrelPrefab;
        controller.Extension = extensionPrefab;
        

        // Initialize everything
        gun.GetComponent<GunFactory>().InitializeGun();

        return gun;
    }

    public static GunStats GetGunStats(Item body, Item barrel, Item extension)
    {
        GunStats stats = body.augment.GetComponent<GunBody>().InstantiateBaseStats;
        barrel.augment.GetComponent<GunBarrel>().BuildStats(stats);
        extension?.augment.GetComponent<GunExtension>().BuildStats(stats);
        return stats;
    }

    public static string GetGunName(Item body, Item barrel, Item extension)
    {
        OverrideName result = StaticInfo.Singleton.SecretNames.Where(x => (x.Body == body && x.Barrel == barrel && x.Extension == extension)).FirstOrDefault();
        if (!(result.Name is null)) { return result.Name; }
        if (extension == null)
            return $"The {body.secretName} {barrel.secretName}";
        return $"The {body.secretName} {extension.secretName} {barrel.secretName}";
    }

    // Prefabs of the different parts
    [SerializeField]
    public Item Body;

    [SerializeField]
    public Item Barrel;

    [SerializeField]
    public Item Extension;

    private GunController gunController;

#if UNITY_EDITOR
    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "GunTest")
        {
            InitializeGun();
        }
    }
#endif

    // Builds the gun from parts
    public void InitializeGun()
    {
        gunController = GetComponent<GunController>();

        List<ProjectileModifier> modifiers = new List<ProjectileModifier>();

        // Destroys gun child before construction
        for (int i = this.transform.childCount; i > 0; --i)
            DestroyImmediate(this.transform.GetChild(0).gameObject);

        // Instantiates the different parts
        GunBody gunBody = Instantiate(Body.augment, transform)
            .GetComponent<GunBody>();

        // Stats is retrieved from gun body
        // NEVER REFERENCE THE GUNSTAT PREFAB DIRECTLY, GUNBODY AUTOMATICALLY INSTANTIATES IT
        // Seriously, i have no moral qualms with making your skulls into decorative ornaments
        gunController.stats = gunBody.InstantiateBaseStats;

        GunBarrel gunBarrel = Instantiate(Barrel.augment, gunBody.attachmentSite.position, gunBody.attachmentSite.rotation, transform)
            .GetComponent<GunBarrel>();

        // Gets the projectile from the barrel
        // It is stored as an inactive object in the gun, which allows for modifications without changing the prefab
        gunController.projectile = gunBarrel.Projectile;
        gunController.projectile.transform.SetParent(transform);
        ProjectileController projectileController = gunController.projectile.GetComponent<ProjectileController>();
        projectileController.stats = gunController.stats;

        // Sets firemode

        switch (gunController.stats.fireMode)
        {
            case GunStats.FireModes.semiAuto:
                gunController.fireRateController = new SemiAutoFirerateController(gunController.stats.Firerate);
                break;
            case GunStats.FireModes.burst:
                gunController.fireRateController = new BurstFirerateController(gunController.stats.Firerate, gunController.stats.burstNum);
                break;
            case GunStats.FireModes.fullAuto:
                gunController.fireRateController = new FullAutoFirerateController(gunController.stats.Firerate);
                break;
            default:
                gunController.fireRateController = new FullAutoFirerateController(gunController.stats.Firerate);
                break;
        }

        modifiers.AddRange(gunBarrel.GetModifiers());
        gunBarrel.BuildStats(gunController.stats);

        if (Extension != null)
        {
            // Instantiate extension itself *once*
            GunExtension gunExtension = Instantiate(Extension.augment, gunBarrel.attachmentPoints[0].position, gunBarrel.attachmentPoints[0].rotation, transform)
                .GetComponent<GunExtension>();
            // Instantiate remaining outputs and models, and register all outputs
            var outputs = new List<Transform>();
            outputs.AddRange(gunExtension.outputs);
            outputs.AddRange(gunExtension.AttachToTransforms(gunBarrel.attachmentPoints));
            gunController.outputs = outputs.ToArray();

            modifiers.AddRange(gunExtension.GetModifiers());
            gunExtension.BuildStats(gunController.stats);
        }
        else
        {
            gunController.outputs = gunBarrel.outputs;
        }

        modifiers.OrderByDescending(modifier => (int) modifier.GetPriority()).ToList();
        modifiers.ForEach(modifier => modifier.ModifyProjectile(ref projectileController));
        gunController.onInitialize?.Invoke(gunController.stats);
    }    
}

