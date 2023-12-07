using System.Collections;
using UnityEngine;

public class AmmoBoxBody : GunBody
{
    [SerializeField]
    private GameObject radar;
    [SerializeField]
    private float ammoRadarCooldownTime = 2f;
    private AmmoBox selectedAmmoBox;

    public override void Start()
    {
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
        {
            Debug.Log("Seeker not attached to gun parent!");
            return;
        }
        gunController.onFire += Reload;
        StartCoroutine(SetClosestAmmoBox());
    }

    private IEnumerator SetClosestAmmoBox()
    {
        selectedAmmoBox = AmmoBox.GetClosestAmmoBox(transform);
        yield return new WaitForSeconds(ammoRadarCooldownTime);
        if (gameObject)
            StartCoroutine(SetClosestAmmoBox());
    } 

    protected override void Reload(GunStats stats)
    {
        selectedAmmoBox = AmmoBox.GetClosestAmmoBox(transform);
    }
    private void Update()
    {
        if (!selectedAmmoBox)
            return;
        radar.transform.LookAt(selectedAmmoBox.transform);
    }
}
