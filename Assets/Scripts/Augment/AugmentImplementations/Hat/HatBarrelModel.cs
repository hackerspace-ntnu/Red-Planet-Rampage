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
    private int remainingBullets;

    void Start()
    {
        remainingBullets = magazineSize;
        for (int i = 0; i < magazineSize; i++)
        {
            ammunition.Add(Instantiate(hatPrefab, ammunitionHolder.transform));
            ammunition[i].transform.Translate(new Vector3(0f, bulletHeigth * i, 0f));
        }
    }

    public void OnReload()
    {
        // TODO: Add optional parameter to reload a percentage of total magazine when function is called.
        remainingBullets = magazineSize;
        ammunition.ForEach(gameObject => gameObject.SetActive(true));
        ammunitionHolder.transform.Translate(new Vector3(0f, bulletHeigth * ammunition.Count, 0f));
    }

    public void OnFire()
    {
        if(remainingBullets <= 0)
        {
            // TODO: Remove Reload call from here when reload mechanics are implemented.
            // For now it will automatically reload if you shoot with no shots in magazine
            OnReload();
            return;
        }

        ammunition[magazineSize - remainingBullets].SetActive(false);
        // Warning: LeanTween not being able to perform LeanMove within the specified time will move the transform too far.
        // This is only a problem when you have crazy high firereate and no reloading to let it catch up.
        // As this probably is a non-issue when reload mechanics are implemented, I'll leave this as is for now.
        ammunitionHolder.LeanMove(new Vector3(ammunitionHolder.position.x, ammunitionHolder.position.y-bulletHeigth, ammunitionHolder.position.z), 0.2f);
        remainingBullets--;
    }

    public void ToggleBullet()
    {
        bullet.SetActive(!bullet.activeInHierarchy);
    }
}
