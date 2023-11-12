using System.Collections;
using UnityEngine;

public class AmmoBoxCollector : MonoBehaviour
{
    private const string ammoBoxBodyName = "Gatling";

    private bool hasAmmoBoxBody;
    public bool CanReload => hasAmmoBoxBody;

    private PlayerManager player;

    private void Start()
    {
        player = GetComponent<PlayerManager>();
        StartCoroutine(CheckForAmmoBoxBody());
    }

    private IEnumerator CheckForAmmoBoxBody()
    {
        // Wait for one frame so that players actually have their gun bodies :9
        yield return null;
        hasAmmoBoxBody = player.identity.Body.displayName == ammoBoxBodyName;
    }

    public bool HasFullMagazine => player.GunController.stats.Ammo >= player.GunController.stats.magazineSize;

    public void Reload()
    {
        if (hasAmmoBoxBody)
        {
            player.GunController.Reload(1f);
        }
    }
}
