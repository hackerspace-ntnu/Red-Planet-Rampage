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

    public override void Attach(GunController gunController)
    {
        gunController.onFireStart += Reload;
        StartCoroutine(SetClosestAmmoBox());
        if (!gunController.Player)
            return;
        playerHandRight.Subscribe(gunController.Player);
        playerHandRight.gameObject.SetActive(true);
        playerHandLeft.Subscribe(gunController.Player);
        playerHandLeft.gameObject.SetActive(true);
    }

    public override void Detach(GunController gunController)
    {
        gunController.onFireStart -= Reload;
        if (!gunController.Player)
            return;
        playerHandRight.Unsubscribe(gunController.Player);
        playerHandLeft.Unsubscribe(gunController.Player);
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
