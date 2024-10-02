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
    [SerializeField]
    private PlayerHand playerHandLeft;
    [SerializeField]
    private PlayerHand playerHandRight;

    public override void Start()
    {
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
            return;
        gunController.onFireStart += Reload;
        StartCoroutine(SetClosestAmmoBox());
        if (!gunController.Player)
            return;
        playerHandRight.SetPlayer(gunController.Player);
        playerHandRight.gameObject.SetActive(true);
        playerHandLeft.SetPlayer(gunController.Player);
        playerHandLeft.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        if (gunController)
            gunController.onFireStart -= Reload;
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
