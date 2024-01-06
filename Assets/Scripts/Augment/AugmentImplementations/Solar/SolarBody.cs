using System.Collections;
using UnityEngine;

public class SolarBody : GunBody
{

    [SerializeField]
    private MeshRenderer meshRenderer;

    [SerializeField]
    private Transform rayCastOrigin;

    [SerializeField]
    private LayerMask obscuringLayers;

    [SerializeField]
    private PlayerHand playerHandRight;
    [SerializeField]
    private PlayerHand playerHandLeft;

    private Material solarPanelMaterial;

    [SerializeField, Range(0, 1)]
    protected float reloadEfficiencyPercentagen = 0.1f;

    private Transform globalLightDirection;

    private const float maxObscuringCheckDistance = 15f;
    private const float coolDownSeconds = 2f;
    private const float chargeUpSeconds = 1; 
    private bool isCooldown = false;

    private const int solarPanelMaterialIndex = 3;

    public override void Start()
    {
        meshRenderer.materials[solarPanelMaterialIndex] = Instantiate(meshRenderer.materials[solarPanelMaterialIndex]);
        solarPanelMaterial = meshRenderer.materials[solarPanelMaterialIndex];
        GameObject mainLight = GameObject.FindGameObjectsWithTag("MainLight")[0];
        globalLightDirection = mainLight ? mainLight.transform : FindAnyObjectByType<Light>().transform;

        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
        {
            Debug.Log("SolarBody not attached to gun parent!");
            return;
        }

        if (!gunController.Player)
            return;
        playerHandRight.SetPlayer(gunController.Player);
        playerHandRight.gameObject.SetActive(true);
        playerHandLeft.SetPlayer(gunController.Player);
        playerHandLeft.gameObject.SetActive(true);
    }


    protected override void Reload(GunStats gunStats)
    {
        if (isCooldown)
            return;
        if (solarPanelMaterial.GetFloat("_On") == 0)
        {
            LeanTween.value(gameObject, SetEmissionStrength, 0, 1, chargeUpSeconds);
        }
        solarPanelMaterial.SetFloat("_On", 1);
        gunController.Reload(reloadEfficiencyPercentagen);
        isCooldown = true;
        StartCoroutine(CoolDown());
    }

    private IEnumerator CoolDown()
    {
        yield return new WaitForSeconds(coolDownSeconds);
        isCooldown = false;
    }

    private void SetEmissionStrength(float strength)
    {
        solarPanelMaterial.SetFloat("_EmissionStrength", strength);
    }

    void FixedUpdate()
    {
        if (!Physics.Raycast(rayCastOrigin.position, -globalLightDirection.forward, maxObscuringCheckDistance, obscuringLayers.value))
        {
            Reload(gunController.stats);
        }
        else
        {
            SetEmissionStrength(0);
            solarPanelMaterial.SetFloat("_On", 0);
        }
    }
}
