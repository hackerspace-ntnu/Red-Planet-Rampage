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

    public override void OnInitialize(GunStats stats)
    {
        animator.speed = Mathf.Max(stats.Firerate, 1f);
        magazineSize = stats.magazineSize;
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
        for (int i = stats.Ammo; i < magazineSize; i++)
        {
            ammunition[i].SetActive(false);
        }
        if (stats.Ammo == 0)
        {
            ToggleBullet();
        }
    }

    // Called by animator!
    public void ToggleBullet()
    {
        bullet.SetActive(!bullet.activeInHierarchy);
        if (!bullet.activeInHierarchy)
        {
            OnFireAnimationEnd?.Invoke();
        }
    }
}
