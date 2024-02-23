using UnityEngine;

public class MainMenuSunMovement : MonoBehaviour
{
    [SerializeField]
    private float RotationDegrees = 360;
    [SerializeField]
    private float RotationSeconds = 360;

    void Start()
    {
        transform.LeanRotateAroundLocal(Vector3.up, RotationDegrees, RotationSeconds).setLoopCount(-1);
        RenderSettings.skybox.SetFloat("_StarDensity", 5);
    }

    private void Update()
    {
        RenderSettings.skybox.SetVector("_SunDirection", transform.forward);
        RenderSettings.skybox.SetFloat("_MaxGradientTreshold", Mathf.Min( 0.25f, Vector3.Dot(transform.forward, -Vector3.up)));
    }
}
