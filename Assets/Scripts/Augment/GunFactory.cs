using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using SecretName;

[RequireComponent(typeof(GunController))]
public class GunFactory : MonoBehaviour
{
    public static GameObject InstantiateGun(Item bodyPrefab, Item barrelPrefab, Item extensionPrefab, PlayerManager owner, Transform parent)
    {
        GameObject gun = Instantiate(new GameObject(), parent);
        GunFactory controller = gun.AddComponent<GunFactory>();
        controller.Body = bodyPrefab;
        controller.Barrel = barrelPrefab;
        controller.Extension = extensionPrefab;

        // Initialize everything
        gun.GetComponent<GunFactory>().InitializeGun(owner);

        var playerIndex = owner.inputManager && owner.inputManager.playerInput ? owner.inputManager.playerInput.playerIndex : 3;
        var cullingLayer = LayerMask.NameToLayer("Gun " + playerIndex);

        GunFactory displayGun = owner.GunOrigin.GetComponent<GunFactory>();
        displayGun.Body = bodyPrefab;
        displayGun.Barrel = barrelPrefab;
        displayGun.Extension = extensionPrefab;
        displayGun.InitializeGun();

        var cullingLayerDisplay = LayerMask.NameToLayer("Player " + playerIndex);

        var firstPersonGunController = gun.GetComponent<GunFactory>().GunController;
        var gunAnimations = displayGun.GetComponentsInChildren<AugmentAnimator>(includeInactive: true);
        foreach (var animation in gunAnimations)
        {
            animation.OnInitialize(firstPersonGunController.stats);
            firstPersonGunController.onFireStart += animation.OnFire;
            firstPersonGunController.onReload += animation.OnReload;
        }

        // Animator initializers may instantiate objects, so we should set layers *afterwards*.
        SetGunLayer(gun.GetComponent<GunFactory>(), cullingLayer);
        SetGunLayer(displayGun, cullingLayerDisplay);

        // Turn off shadow casting for first person gun
        gun.GetComponentsInChildren<MeshRenderer>(includeInactive: true).ToList()
            .ForEach(mesh => mesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off);
        gun.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true).ToList()
            .ForEach(mesh => mesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off);

        firstPersonGunController.RightHandTarget = displayGun.GunController.RightHandTarget;

        if (displayGun.GunController.HasRecoil)
            firstPersonGunController.onFire += displayGun.GunController.PlayRecoil;

        if (displayGun.GunController.projectile is BulletController)
            ((BulletController)gun.GetComponent<GunFactory>().GunController.projectile).Trail.layer = 0;

        if (displayGun.GunController.projectile is MeshProjectileController)
            ((MeshProjectileController)gun.GetComponent<GunFactory>().GunController.projectile).Vfx.gameObject.layer = 0;

        if (displayGun.GunController.projectile is LazurController)
            ((LazurController)gun.GetComponent<GunFactory>().GunController.projectile).Vfx.gameObject.layer = 0;

        return gun;
    }

    public static GameObject InstantiateGunAI(Item bodyPrefab, Item barrelPrefab, Item extensionPrefab, PlayerManager owner, Transform parent)
    {
        GunFactory displayGun = owner.GunOrigin.gameObject.AddComponent<GunFactory>();
        displayGun.Body = bodyPrefab;
        displayGun.Barrel = barrelPrefab;
        displayGun.Extension = extensionPrefab;
        displayGun.InitializeGun(owner);

        return displayGun.gameObject;
    }

    private static void SetGunLayer(GunFactory gunFactory, int cullingLayer)
    {
        gunFactory.gameObject.layer = cullingLayer;

        var children = gunFactory.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (var child in children)
        {
            child.gameObject.layer = cullingLayer;
        }

    }

    public static GameObject InstantiateGun(Item bodyPrefab, Item barrelPrefab, Item extensionPrefab, PlayerManager owner, RectTransform parent)
    {
        GameObject gun = Instantiate(new GameObject());
        GunFactory controller = gun.AddComponent<GunFactory>();
        controller.Body = bodyPrefab;
        controller.Barrel = barrelPrefab;
        controller.Extension = extensionPrefab;

        // Initialize everything
        gun.GetComponent<GunFactory>().InitializeGun(owner);
        gun.transform.SetParent(parent);
        gun.transform.position = parent.position;
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
        OverrideName result = StaticInfo.Singleton.SecretNames
                                        .FirstOrDefault(x => x.Body == body && x.Barrel == barrel && x.Extension == extension);

        if (result.Name is not null)
            return result.Name;

        if (extension == null)
            return $"The {body.secretName.Capitalized()} {barrel.secretName.Capitalized()}";

        return $"The {body.secretName.Capitalized()} {extension.secretName.Capitalized()} {barrel.secretName.Capitalized()}";
    }

    public static string GetGunName(Item body, Item barrel, Item extension, out bool isSecret)
    {
        OverrideName result = StaticInfo.Singleton.SecretNames
                                        .FirstOrDefault(x => x.Body == body && x.Barrel == barrel && x.Extension == extension);
        isSecret = result.Name is not null;
        if (isSecret)
            return result.Name;

        if (extension == null)
            return $"The {body.secretName.Capitalized()} {barrel.secretName.Capitalized()}";

        return $"The {body.secretName.Capitalized()} {extension.secretName.Capitalized()} {barrel.secretName.Capitalized()}";
    }

    public static bool TryGetGunAchievement(Item body, Item barrel, Item extension, out SecretCombinationType achievement)
    {
        OverrideName result = StaticInfo.Singleton.SecretNames
                                        .FirstOrDefault(x => x.Body == body && x.Barrel == barrel && x.Extension == extension);

        achievement = SecretCombinationType.None;
        if (result.Name is null)
            return false;

        achievement = result.Achievement;
        return true;
    }

    // Prefabs of the different parts
    [SerializeField]
    public Item Body;

    [SerializeField]
    public Item Barrel;

    [SerializeField]
    public Item Extension;
    private GunController gunController;
    public GunController GunController => gunController;

#if UNITY_EDITOR
    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "GunTest")
        {
            InitializeGun(null);
        }
    }
#endif

    // Builds the gun from parts
    public void InitializeGun(PlayerManager owner = null)
    {
        gunController = GetComponent<GunController>();
        // Make gun remember who shoots with it
        if (owner)
            gunController.SetPlayer(owner);

        List<ProjectileModifier> modifiers = new List<ProjectileModifier>();

        // Destroys gun child before construction
        // Don't refactor this to a simple foreach, we're destroying each element!
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            DestroyImmediate(transform.GetChild(i).gameObject);
#else
            Destroy(transform.GetChild(i).gameObject);
#endif
        }

        // Instantiates the different parts
#if UNITY_EDITOR
        GunBody gunBody = ((GameObject)PrefabUtility.InstantiatePrefab(Body.augment, transform))
            .GetComponent<GunBody>();
#else
        GunBody gunBody = Instantiate(Body.augment, transform)
            .GetComponent<GunBody>();
#endif
        // Stats is retrieved from gun body
        // NEVER REFERENCE THE GUNSTAT PREFAB DIRECTLY, GUNBODY AUTOMATICALLY INSTANTIATES IT
        // Seriously, i have no moral qualms with making your skulls into decorative ornaments
        gunController.stats = gunBody.InstantiateBaseStats;

        gunController.RightHandTarget = gunBody.RightHandTarget;
        gunController.LeftHandTarget = gunBody.LeftHandTarget;

#if UNITY_EDITOR
        GunBarrel gunBarrel = ((GameObject)PrefabUtility.InstantiatePrefab(Barrel.augment, transform))
            .GetComponent<GunBarrel>();
#else
        GunBarrel gunBarrel = Instantiate(Barrel.augment, transform)
            .GetComponent<GunBarrel>();
#endif

        gunBarrel.transform.position = gunBody.attachmentSite.position;
        gunBarrel.transform.rotation = gunBody.attachmentSite.rotation;

        gunController.projectile = gunBarrel.Projectile;
        // And make projectile remember who shot it.
        gunController.projectile.player = owner;

        // Gets the projectile from the barrel
        // It is stored as an inactive object in the gun, which allows for modifications without changing the prefab


        // Sets firemode

        modifiers.AddRange(gunBarrel.GetModifiers());
        gunBarrel.BuildStats(gunController.stats);

        if (Extension != null)
        {
            // Instantiate extension itself *once*
#if UNITY_EDITOR
            GunExtension gunExtension = ((GameObject)PrefabUtility.InstantiatePrefab(Extension.augment, transform))
                .GetComponent<GunExtension>();
#else
            GunExtension gunExtension = Instantiate(Extension.augment, transform)
                .GetComponent<GunExtension>();
#endif
            gunExtension.transform.position = gunBarrel.attachmentPoints[0].position;
            gunExtension.transform.rotation = gunBarrel.attachmentPoints[0].rotation;

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

        // TODO: The output system needs rework 
        gunController.projectile.projectileOutput = gunController.outputs[0];

        gunController.projectile.stats = gunController.stats;

        modifiers.OrderByDescending(modifier => (int)modifier.GetPriority()).ToList();
        modifiers.ForEach(modifier => modifier.Attach(gunController.projectile));
        gunController.onInitializeGun?.Invoke(gunController.stats);

        // Moved it below the stat changes so the stats actually, yknow, affect the firerate.
        // Will be removed anyways when the firemode system is updated to accomodate a wider variety of guns
        switch (gunController.stats.fireMode)
        {
            case GunStats.FireModes.SemiAuto:
                gunController.fireRateController = new SemiAutoFirerateController(gunController.stats.Firerate);
                break;
            case GunStats.FireModes.Burst:
                gunController.fireRateController = new BurstFirerateController(gunController.stats.Firerate, gunController.stats.burstNum);
                break;
            case GunStats.FireModes.FullAuto:
                gunController.fireRateController = new FullAutoFirerateController(gunController.stats.Firerate);
                break;
            default:
                gunController.fireRateController = new FullAutoFirerateController(gunController.stats.Firerate);
                break;
        }
        // Ensure ammo == magazineSize after all modifiers are applied
        gunController.Reload(1);
    }
}

