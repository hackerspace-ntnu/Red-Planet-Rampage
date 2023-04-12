using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolarBody : GunBody
{
    [SerializeField]
    private Material solarPanelMat;

    [SerializeField, Range(0, 1)]
    protected float reloadEfficiencyPercentagen = 0.1f;

    private const float sunDirection = 90f;
    private const float sunDirectionSpan = 45f;
    private const float coolDownSeconds = 0.5f;
    private bool isCooldown = false;

    public override void Start()
    {
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
        {
            Debug.Log("HatBarrel not attached to gun parent!");
            return;
        }
    }


    protected override void Reload(GunStats gunStats)
    {
        if (isCooldown)
            return;
        solarPanelMat.SetFloat("_On", 1);
        gunController.Reload(reloadEfficiencyPercentagen);
        isCooldown = true;
        StartCoroutine(CoolDown());
    }

    private IEnumerator CoolDown()
    {
        yield return new WaitForSeconds(coolDownSeconds);
        isCooldown = false;
    }

    void Update()
    {
        if (transform.parent.rotation.eulerAngles.y < sunDirection + sunDirectionSpan && transform.parent.rotation.eulerAngles.y > sunDirection - sunDirectionSpan)
        {
            Reload(gunController.stats);
        }
        else
        {
            solarPanelMat.SetFloat("_On", 0);
        }
    }
}
