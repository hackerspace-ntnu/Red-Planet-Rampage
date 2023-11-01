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

    private Material solarPanelMaterial;

    [SerializeField, Range(0, 1)]
    protected float reloadEfficiencyPercentagen = 0.1f;

    private Quaternion globalLightDirection;

    private const float maxObscuringCheckDistance = 15f;
    private const float coolDownSeconds = 0.5f;
    private const float chargeUpSeconds = 1; 
    private bool isCooldown = false;

    private const int solarPanelMaterialIndex = 3;

    public override void Start()
    {
        meshRenderer.materials[solarPanelMaterialIndex] = Instantiate(meshRenderer.materials[solarPanelMaterialIndex]);
        solarPanelMaterial = meshRenderer.materials[solarPanelMaterialIndex];
        // The main directional light in the scene (the sun) should ideally be tagged as such, but this works for now
        globalLightDirection = FindFirstObjectByType<Light>().transform.rotation;

        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
        {
            Debug.Log("SolarBody not attached to gun parent!");
            return;
        }
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
        float orientationOverlap = Vector3.Dot(rayCastOrigin.transform.up, globalLightDirection.eulerAngles);
        if (!Physics.Raycast(rayCastOrigin.position, globalLightDirection.eulerAngles, maxObscuringCheckDistance, obscuringLayers.value) && orientationOverlap > 0)
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
