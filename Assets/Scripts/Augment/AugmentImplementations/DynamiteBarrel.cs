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

    void Start()
    {
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController || !gunController.Player)
            return;

        gunController.Player.onDeath += OnDeath;
        stickyModifer.OnStuckToTarget += AddDynamite;

        if (gunController.Player is AIManager)
        {
            StartCoroutine(TryDetonating());
            return;
        }   

        gunController.Player.GetComponent<PlayerMovement>().ResetZoom();
        gunController.Player.inputManager.onZoomPerformed += OnZoom;
    }

    private void AddDynamite(StuckObject stuckObject)
    {
        if (stuckObject is StickyDynamite)
            activeDynamites.Add((StickyDynamite)stuckObject);
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
        activeDynamites.ForEach(dynamite => dynamite.Detonate(gunController.Player));
        activeDynamites.Clear();
    }

    private void OnDeath(PlayerManager killer, PlayerManager victim, DamageInfo info)
    {
        StopAllCoroutines();
        if (!gunController || !gunController.Player)
            return;

        activeDynamites.ForEach(dynamite => dynamite.gameObject.SetActive(false));

        stickyModifer.OnStuckToTarget -= AddDynamite;

        if (gunController.Player is not AIManager)
            gunController.Player.inputManager.onZoomPerformed -= OnZoom;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        if (!gunController || !gunController.Player)
            return;

        activeDynamites.ForEach(dynamite => dynamite.gameObject.SetActive(false));

        stickyModifer.OnStuckToTarget -= AddDynamite;

        if (gunController.Player is not AIManager)
            gunController.Player.inputManager.onZoomPerformed -= OnZoom;
    }
}
