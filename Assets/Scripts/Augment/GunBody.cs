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

    //TODO: Modifier refactor
    private GunController gunController;

    public void Start()
    {
        GunController gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
        {
            Debug.Log("HatBarrel not attached to gun parent!");
            return;

        }
    }

    public void Reload(float magazinePercentage)
    {
        
    }
}
