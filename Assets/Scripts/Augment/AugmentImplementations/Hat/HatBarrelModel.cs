using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for moving individual parts of the model that can't be controlled by the normal animation controller.
/// </summary>
public class HatBarrelModel : MonoBehaviour
{
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

    public void OnInitialize(int magazine)
    {
        magazineSize = magazine;
        for (int i = 0; i < magazineSize; i++)
        {
            ammunition.Add(Instantiate(hatPrefab, ammunitionHolder.transform));
            ammunition[i].transform.Translate(new Vector3(0f, bulletHeigth * i, 0f));
        }
    }

    public void OnReload(int ammo)
    {
        bullet.SetActive(true);
        for (int i = 0; i < ammo; i++)
        {
            ammunition[i].SetActive(true);
        }
    }

    public void OnFire(int remainingAmmo)
    {
        for (int i = remainingAmmo; i < magazineSize; i++)
        {
            ammunition[i].SetActive(false);
        }
        if(remainingAmmo == 0)
        {
            ToggleBullet();
        }
    }

    public void ToggleBullet()
    {
        bullet.SetActive(!bullet.activeInHierarchy);
    }
}
