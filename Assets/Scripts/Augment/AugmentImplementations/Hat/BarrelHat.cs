using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that ties the functions and properties of the completed gun to the animations of the hat barrel
/// </summary>
public class BarrelHat : MonoBehaviour
{
    private GunController gunController;
    [SerializeField]
    private Animator animator;

    void Awake()
    {
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController) 
        {
            Debug.Log("HatBarrel not attached to gun parent!");
            return;
        }
        gunController.onInitialize += OnInitialize;
        gunController.onFire += OnFire;
    }

    private void OnInitialize(GunStats gunstats)
    {
        animator.speed = Mathf.Max(gunstats.Firerate, 1f);
    }

    private void OnFire(GunStats gunstats)
    {
        animator.SetTrigger("Fire");
    }

    private void OnDestroy()
    {
        gunController.onFire -= OnFire;
        gunController.onInitialize -= OnInitialize;
    }
}
