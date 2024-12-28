using UnityEngine;

public class Speedometer : MonoBehaviour
{
    public Rigidbody Body;

    [SerializeField]
    private float maxSpeed = 25;

    [SerializeField]
    private Renderer speedometer;

    [SerializeField]
    private int materialIndex = 0;

    private Material speedometerMaterial;

    private void Start()
    {
        speedometer.materials[materialIndex] = Instantiate(speedometer.materials[materialIndex]);
        speedometerMaterial = speedometer.materials[materialIndex];
    }

    private void Update()
    {
        if (!Body || !speedometerMaterial)
            return;
        var velocity = Body.velocity.magnitude;
        var targetValue = Mathf.Clamp(velocity, 0f, maxSpeed) / maxSpeed;
        var currentValue = speedometerMaterial.GetFloat("_Value");
        speedometerMaterial.SetFloat("_Value", Mathf.Lerp(currentValue, targetValue, .3f));
    }

    public void SetDisplayedValue(float value)
    {
        speedometerMaterial.SetFloat("_Value", value);
    }
}
