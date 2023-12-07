using System.Collections;
using UnityEngine;

public class AmmoBoxBody : GunBody
{
    [SerializeField]
    private GameObject radar;
    [SerializeField]
    private float ammoRadarCooldownTime = 2f;
    [SerializeField]
    private float radarRotationSpeed = 5f;
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
        selectedAmmoBox = AmmoBox.GetClosestAmmoBox(transform.position);
        yield return new WaitForSeconds(ammoRadarCooldownTime);
        if (gameObject)
            StartCoroutine(SetClosestAmmoBox());
    } 

    protected override void Reload(GunStats stats)
    {
        selectedAmmoBox = AmmoBox.GetClosestAmmoBox(transform.position);
    }
    private void Update()
    {
        if (!selectedAmmoBox)
            return;
        radar.transform.rotation = Quaternion.Slerp(radar.transform.rotation, Quaternion.LookRotation(selectedAmmoBox.transform.position - transform.position), Time.deltaTime * radarRotationSpeed);
    }
}
