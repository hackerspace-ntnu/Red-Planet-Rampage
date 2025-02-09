using UnityEngine;

public enum SpeedometerPattern
{
    Manual,
    Rigidbody,
    Random,
    Wave,
}

public class Speedometer : MonoBehaviour
{
    public Rigidbody Body;

    [SerializeField]
    private SpeedometerPattern pattern = SpeedometerPattern.Manual;

    [SerializeField]
    private float maxSpeed = 25;

    [SerializeField]
    private Renderer speedometer;

    [SerializeField]
    private int materialIndex = 0;

    [SerializeField]
    private bool invert = false;

    private Material speedometerMaterial;

    private float randomTarget = 1;
    private float randomDuration = .5f;
    private float randomTimePassed = 0;

    private void Start()
    {
        speedometer.materials[materialIndex] = Instantiate(speedometer.materials[materialIndex]);
        speedometerMaterial = speedometer.materials[materialIndex];
    }

    private void Update()
    {
        if (!speedometerMaterial || pattern is SpeedometerPattern.Manual)
            return;

        var currentValue = speedometerMaterial.GetFloat("_Value");
        var newValue = pattern switch
        {
            SpeedometerPattern.Rigidbody => VelocityValue(currentValue),
            SpeedometerPattern.Random => RandomValue(currentValue),
            SpeedometerPattern.Wave => WaveValue(currentValue),
            SpeedometerPattern.Manual or _ => currentValue,
        };

        speedometerMaterial.SetFloat("_Value", newValue);
    }

    private float VelocityValue(float currentValue)
    {
        if (!Body)
            return 0;

        var velocity = Body.velocity.magnitude;
        var targetValue = Mathf.Clamp(velocity, 0f, maxSpeed) / maxSpeed;
        return Mathf.Lerp(currentValue, targetValue, .3f);
    }

    private float RandomValue(float currentValue)
    {
        if (randomTimePassed >= randomDuration)
        {
            randomTimePassed = 0;
            randomDuration = Random.Range(.5f, 3f);
            randomTarget = Random.Range(0f, 1f);
        }
        randomTimePassed += Time.deltaTime;

        return Mathf.Lerp(currentValue, randomTarget, .15f);
    }

    private float WaveValue(float _) =>
        Mathf.Sin(Time.time * .5f) * Mathf.Cos(Time.time * .7f + 13.89f) * .5f + .5f;

    public void SetDisplayedValue(float value)
    {
        speedometerMaterial.SetFloat("_Value", invert ? 1 - value : value);
    }
}
