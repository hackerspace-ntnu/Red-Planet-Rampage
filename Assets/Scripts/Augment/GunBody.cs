using UnityEngine;

public class GunBody : MonoBehaviour
{
    [SerializeField]
    private GunStats stats;

    // Base stats of the gun
    public GunStats InstantiateBaseStats { get => Instantiate(stats); }

    // Where to attach barrel
    public Transform attachmentSite;

    // For displaying items with correct alignment
    public Transform midpoint;

    // Where to attach player hands
    public Transform RightHandTarget;
    public Transform LeftHandTarget;

    [SerializeField, Range(0, 1)]
    protected float reloadEfficiencyPercentage = 1f;

    //TODO: Modifier refactor
    protected GunController gunController;

    public virtual void Start()
    {
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
        {
            return;
        }
        // TODO: refactor this, which additionaly only exists to support placeholder weapons with no reload implementation
        gunController.onFireEnd += Reload;
    }

    protected virtual void Reload(GunStats stats)
    {
        if (gunController.stats.Ammo == 0)
            gunController.Reload(reloadEfficiencyPercentage);
    }

    private void OnDestroy()
    {
        if (!gunController) return;
        gunController.onFireEnd -= Reload;
    }
}
