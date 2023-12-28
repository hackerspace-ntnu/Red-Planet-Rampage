using UnityEngine;

public class FlashModifier : MonoBehaviour, ProjectileModifier
{
    [SerializeField]
    private Renderer[] flashingMeshes;

    [SerializeField]
    private Material flashMaterial;

    [SerializeField]
    private AnimationCurve curve;

    [SerializeField]
    private float duration = .1f;

    private GunController gunController;
    private Material materialInstance;

    private void Awake()
    {
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
        {
            Debug.Log("FlashModifier not attached to gun parent!");
            return;
        }

        materialInstance = Instantiate(flashMaterial);
        foreach (var mesh in flashingMeshes)
        {
            mesh.materials[0] = materialInstance;
        }
    }

    public void Attach(ProjectileController projectile)
    {
        gunController.onFire += Flash;
    }

    public void Detach(ProjectileController controller)
    {
        gunController.onFire -= Flash;
    }

    private void Flash(GunStats stats)
    {
        gameObject.LeanValue(SetIntensity, 0, 1, duration);
    }

    private void SetIntensity(float intensity)
    {
        Debug.Log(intensity);
        Debug.Log(curve.Evaluate(intensity));
        materialInstance.SetFloat("_Intensity", curve.Evaluate(intensity));
    }
}
