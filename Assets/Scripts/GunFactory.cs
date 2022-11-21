using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(GunController))]
public class GunFactory : MonoBehaviour
{
    public static GameObject InstantiateGun(GameObject bodyPrefab, GameObject barrelPrefab, GameObject extensionPrefab, Transform parent)
    {
        GameObject gun = Instantiate(new GameObject(), parent);
        GunFactory controller = gun.AddComponent<GunFactory>();
        controller.bodyPrefab = bodyPrefab;
        controller.barrelPrefab = barrelPrefab;
        controller.extensionPrefab = extensionPrefab;

        return gun;
    }
    // Prefabs of the different parts
    [SerializeField]
    public GameObject bodyPrefab;

    [SerializeField]
    public GameObject barrelPrefab;

    [SerializeField]
    public GameObject extensionPrefab;

    private GunController gunController;

    private void Start()
    {
        InitializeGun();
    }

    // Builds the gun from parts
    public void InitializeGun()
    {
        gunController = GetComponent<GunController>();

        // Destroys gun child before construction
        for (int i = this.transform.childCount; i > 0; --i)
            DestroyImmediate(this.transform.GetChild(0).gameObject);

        // Instantiates the different parts
        var gunBody = Instantiate(bodyPrefab, transform)
            .GetComponent<GunBody>();

        // Stats is retrieved from gun body
        // NEVER REFERENCE THE GUNSTAT PREFAB DIRECTLY, GUNBODY AUTOMATICALLY INSTANTIATES IT
        // Seriously, i have no moral qualms with making your skulls into decorative ornaments
        gunController.stats = gunBody.InstantiateBaseStats;

        var gunBarrel = Instantiate(barrelPrefab, gunBody.attachmentSite.position, gunBody.attachmentSite.rotation, transform)
            .GetComponent<GunBarrel>();

        // Gets the projectile from the barrel
        // It is stored as an inactive object in the gun, which allows for modifications without changing the prefab
        gunController.projectile = gunBarrel.Projectile;
        gunController.projectile.transform.SetParent(transform);
        gunController.projectile.GetComponent<ProjectileController>().stats = gunController.stats;

        if (extensionPrefab != null)
        {
            var extension = Instantiate(extensionPrefab, gunBarrel.attachmentPoints[0].position, gunBarrel.attachmentPoints[0].rotation, transform)
                .GetComponent<GunExtension>();
            extension.AttachToTransforms(gunBarrel.attachmentPoints);
            gunController.outputs = extension.outputs;
        }
        else
        {
            gunController.outputs = gunBarrel.outPuts;
        }

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

        // Runs attach of all modifyers in ascending order

        foreach (var modifier in GetComponentsInChildren<GunModifier>().OrderBy(x => x.priority))
        {
            modifier.Attach(gunController);
        }
    }
}

