using UnityEngine;

public class HackingExtension : GunExtension
{
    [SerializeField]
    private MeshRenderer meshRenderer;
    [SerializeField]
    private Material hackingScreen;
    [SerializeField]
    private float scrollAmount = 0.10f;
    private const int hackingScreenMaterialIndex = 1;

    private void Start()
    {
        meshRenderer.materials[hackingScreenMaterialIndex] = Instantiate(meshRenderer.materials[hackingScreenMaterialIndex]);
        hackingScreen = meshRenderer.materials[hackingScreenMaterialIndex];
    }

    public override void Attach(GunController gunController)
    {
        gunController.onFireStart += Fire;
    }

    public override void Detach(GunController gunController)
    {
        gunController.onFireStart -= Fire;
    }

    private void Fire(GunStats stats)
    {
        hackingScreen.SetFloat("_ScrollAmount", hackingScreen.GetFloat("_ScrollAmount") + scrollAmount);
    }
}
