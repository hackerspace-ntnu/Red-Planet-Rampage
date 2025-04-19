using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class DynamiteBarrel : GunBarrel
{
    private List<StickyDynamite> activeDynamites = new();
    [SerializeField]
    private StickyProjectileModifier stickyModifer;
    [SerializeField]
    private Mesh explodingBarrelMesh;
    private bool isDetonatableEarly = false;
    [SerializeField]
    private MeshProjectileController meshProjectiles;
    private SateliteUplink sateliteUplink;
    // Avoids crashing game with too many particle spawners in the case of sticky + in air
    // TODO: allow for even more by instantiating particles with a positionbuffer instead
    private const int maxDetonatableInTotal = 10;

    public override void Attach(GunController gunController)
    {
        if (!gunController || !gunController.Player)
            return;

        gunController.Player.onDeath += OnDeath;
        stickyModifer.OnStuckToTarget += AddDynamite;

        // If bullets are large enough (enlarger), they should look like explosive barrels instead
        if (Projectile.stats.ProjectileScale >= 3)
            GetComponent<MeshProjectileController>().Vfx.SetMesh("Mesh", explodingBarrelMesh);

        // If fire extension, dynamites should be able to explode while in air
        if (gunController.GetComponentInChildren<Fire>())
            isDetonatableEarly = true;
        else
            sateliteUplink = gunController.GetComponentInChildren<SateliteUplink>();

        if (gunController.Player is AIManager)
        {
            StartCoroutine(TryDetonating());
            return;
        }
        syncDirection = SyncDirection.ClientToServer;
        gunController.Player.GetComponent<PlayerMovement>().ResetZoom();
        if (gunController.Player.inputManager)
            gunController.Player.inputManager.onZoomPerformed += OnZoom;
    }

    private void AddDynamite(StuckObject stuckObject)
    {
        if (stuckObject is not StickyDynamite)
            return;
        var stuckDynamite = (StickyDynamite)stuckObject;
        activeDynamites.Add(stuckDynamite);
        stuckDynamite.ResetDynamite();
        if (Projectile.stats.ProjectileScale >= 3)
            stuckDynamite.SetBarrel();
    }

    // Letting the AIs also explode stuff every now and then
    private IEnumerator TryDetonating()
    {
        while (true)
        {
            yield return new WaitForSeconds(2);
            if (activeDynamites.Any())
                activeDynamites.ForEach(dynamite => dynamite.Detonate(gunController.Player));
            activeDynamites.Clear();
        }
    }

    private void OnZoom(InputAction.CallbackContext ctx)
    {
        if (authority)
            CmdDetonate();
    }

    [Command]
    private void CmdDetonate()
    {
        RpcDetonate();
    }

    [ClientRpc]
    private void RpcDetonate()
    {
        try
        {
            if (isDetonatableEarly)
            {
                var explosivePositions = meshProjectiles.ProjectilePositions;
                explosivePositions.Take(Mathf.Max(maxDetonatableInTotal - activeDynamites.Count, 0))
                    .ToList().ForEach(position =>
                    {
                        var dynamite = (StickyDynamite)stickyModifer.InstantiateManual(position);
                        dynamite.Explosion.Init();
                        activeDynamites.Add(dynamite);
                    });
                meshProjectiles.ClearProjectiles();
            }
            else if (sateliteUplink)
                sateliteUplink.TargetManual(activeDynamites
                    .Select(dynamite => dynamite.transform.position));

            if (authority && activeDynamites.Count > 0 && gunController.Player.HUDController)
                gunController.Player.HUDController.CrossHairDetonationAnimation();

            activeDynamites.ForEach(dynamite => dynamite.Detonate(gunController.Player));
            activeDynamites.Clear();
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to detonate!");
            Debug.LogError(e);
        }
    }

    private void OnDeath(PlayerManager killer, PlayerManager victim, DamageInfo info)
    {
        Detach(gunController);
    }

    public override void Detach(GunController gunController)
    {
        StopAllCoroutines();
        if (!gunController || !gunController.Player || !gunController.Player.IsAlive)
            return;

        activeDynamites.ForEach(dynamite => dynamite.gameObject.SetActive(false));

        stickyModifer.OnStuckToTarget -= AddDynamite;

        if (gunController.Player is not AIManager && gunController.Player.inputManager)
        {
            gunController.Player.inputManager.onZoomPerformed -= OnZoom;
            gunController.Player.GetComponent<PlayerMovement>().ReEnableZoom();
        }
    }
}
