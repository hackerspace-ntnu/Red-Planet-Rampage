using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunBody : MonoBehaviour
{
    [SerializeField]
    private GunStats stats;

    // Base stats of the gun
    public GunStats InstantiateBaseStats { get => Instantiate(stats); }

    // Where to attach barrel
    public Transform attachmentSite;

    [SerializeField, Range(0,1)]
    protected float reloadEfficiencyPercentage = 1f;

    //TODO: Modifier refactor
    protected GunController gunController;

    public virtual void Start()
    {
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
        {
            Debug.Log("HatBarrel not attached to gun parent!");
            return;
        }
        // TODO: refactor this, which additionaly only exists to support placeholder weapons with no reload implementation
        gunController.onFire += Reload;
    }

    protected virtual void Reload(GunStats stats)
    {
        if (gunController.stats.Ammo == 1)
            gunController.Reload(reloadEfficiencyPercentage);
    }

    private void OnDestroy()
    {
        gunController.onFire -= Reload;
    }
}
