using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for moving individual parts of the model that can't be controlled by the normal animation controller.
/// </summary>
public class HatBarrelModel : AugmentAnimator
{
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private GameObject hatPrefab;
    [SerializeField]
    private GameObject bullet;
    [SerializeField]
    private float bulletHeigth = 0.05f;

    [SerializeField]
    private Transform ammunitionHolder;
    private List<GameObject> ammunition = new List<GameObject>();

    private int magazineSize = 5;

    private bool isLastShot;

    public override void OnInitialize(GunStats stats)
    {
        if (ammunition.Count > 0)
            return;
        animator.speed = Mathf.Max(stats.Firerate, 1f);
        magazineSize = stats.MagazineSize;
        for (int i = 0; i < magazineSize; i++)
        {
            ammunition.Add(Instantiate(hatPrefab, ammunitionHolder.transform));
            ammunition[i].transform.Translate(new Vector3(0f, bulletHeigth * i, 0f));
        }
    }

    public override void OnReload(GunStats stats)
    {
        bullet.SetActive(true);
        for (int i = 0; i < stats.Ammo; i++)
        {
            ammunition[i].SetActive(true);
        }
    }

    public override void OnFire(GunStats stats)
    {
        animator.SetTrigger("Fire");
        if (stats.Ammo - 1 == 0)
        {
            isLastShot = false;
            return;
        }
            
        isLastShot = stats.Ammo - 2 == 0;
        for (int i = stats.Ammo - 2; i < magazineSize; i++)
        {
            ammunition[i].SetActive(false);
        }
    }

    // Called by animator!
    public void EnableBullet()
    {
        bullet.SetActive(ammunition[0].activeInHierarchy || isLastShot);
    }

    public void DisableBullet()
    {
        bullet.SetActive(false);
        OnShotFiredAnimation?.Invoke();
        OnAnimationEnd?.Invoke();
    }
}
