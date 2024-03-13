using UnityEngine;

public class MainMenuSunMovement : MonoBehaviour
{
    [SerializeField]
    private float RotationDegrees = 360;
    [SerializeField]
    private float RotationSeconds = 360;

    private int tween;
    private Quaternion originalRotation;

    private void Start()
    {
        RenderSettings.skybox.SetFloat("_StarDensity", 5);
        originalRotation = transform.rotation;
        Animate();
    }

    public void Restart()
    {
        if (LeanTween.isTweening(tween))
            LeanTween.cancel(tween);
        transform.rotation = originalRotation;
        Animate();
    }

    public void Animate()
    {
        tween = transform.LeanRotateAroundLocal(Vector3.up, RotationDegrees, RotationSeconds).setLoopCount(-1).id;
    }

    private void Update()
    {
        RenderSettings.skybox.SetVector("_SunDirection", transform.forward);
        RenderSettings.skybox.SetFloat("_MaxGradientTreshold", Mathf.Min( 0.25f, Vector3.Dot(transform.forward, -Vector3.up)));
    }
}
