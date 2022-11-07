using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GunController : MonoBehaviour
{
    // Prefabs of the different parts
    [SerializeField]
    private GameObject bodyPrefab;

    [SerializeField]
    private GameObject barrelPrefab;

    [SerializeField]
    private GameObject extensionPrefab;

    // Where the gun fires the bullets from
    // Is automatically set by barrel or extension (if one i available)
    [HideInInspector]
    public Transform[] outputs;

    // Keeps track of when gun should be fired
    protected FireRateController fireRateController;

    // All the stats of the gun and projectile
    public GunStats stats{ get; private set; }

    // Inputs
    public bool triggerHeld, triggerPressed;


    //Projectile to shoot
    public GameObject projectile { get; private set; }


    private void Start()
    {
        InitializeGun();
    }

    // Builds the gun from parts
    public void InitializeGun()
    {
        // Destroys gun child before construction
        for (int i = this.transform.childCount; i > 0; --i)
            DestroyImmediate(this.transform.GetChild(0).gameObject);

        // Instantiates the different parts
        var gunBody = Instantiate(bodyPrefab, transform)
            .GetComponent<GunBody>();

        // Stats is retrieved from gun body
        // NEVER REFERENCE THE GUNSTAT PREFAB DIRECTLY, GUNBODY AUTOMATICALLY INSTANTIATES IT
        // Seriously, i have no moral qualms with making your skulls into decorative ornaments
        stats = gunBody.Stats;

        var gunBarrel = Instantiate(barrelPrefab, gunBody.attachmentSite.position, gunBody.attachmentSite.rotation, transform)
            .GetComponent<GunBarrel>();

        // Gets the projectile from the barrel
        // It is stored as an inactive object in the gun, which allows for modifications without changing the prefab
        projectile = gunBarrel.Projectile;
        projectile.transform.SetParent(transform);
        projectile.GetComponent<ProjectileController>().stats = stats;

        if (extensionPrefab != null)
        {
            var extension = Instantiate(extensionPrefab, gunBarrel.attachmentPoints[0].position, gunBarrel.attachmentPoints[0].rotation, transform)
                .GetComponent<GunExtension>();
            extension.AttachToTransforms(gunBarrel.attachmentPoints);
            outputs = extension.outputs;
        }
        else
        {
            outputs = gunBarrel.outPuts;
        }

        // Sets firemode

        switch (stats.fireMode)
        {
            case GunStats.FireModes.semiAuto:
                this.fireRateController = new SemiAutoFirerateController(stats.Firerate);
                break;
            case GunStats.FireModes.burst:
                this.fireRateController = new BurstFirerateController(stats.Firerate, stats.burstNum);
                break;
            case GunStats.FireModes.fullAuto:
                this.fireRateController = new FullAutoFirerateController(stats.Firerate);
                break;
            default:
                this.fireRateController = new FullAutoFirerateController(stats.Firerate);
                break;
        }

        // Runs attach of all modifyers in ascending order

        foreach (var modifyer in GetComponentsInChildren<GunModifier>().OrderBy(x => x.priority))
        {
            modifyer.Attach(this);
        }
    }

    private void FixedUpdate()
    {
        if (fireRateController.shouldFire(triggerPressed, triggerHeld))
        {
            FireGun();
        }
    }
    private void FireGun()
    {
        foreach(var output in outputs)
        {
            for(int i = 0; i < Mathf.Max((int) stats.ProjectilesPerShot.value(), 1); i++)
            {
                // Adds spread
                Quaternion dir = output.rotation;
                if(stats.ProjectileSpread > 0)
                {
                    Vector2 rand = Random.insideUnitCircle * stats.ProjectileSpread;
                    dir = dir * Quaternion.Euler(rand.x, rand.y, 0f);
                }
                // Makes projectile 
                // TODO: generalize this so that different methods of "Creating" bullets can be used to save performance
                Instantiate(projectile, output.position, dir).SetActive(true);
            }
        }
    }

}

