using UnityEngine;
using UnityEngine.UI;

public class TilingCowbots : MonoBehaviour
{
    [SerializeField] private RawImage cowboys;
    [SerializeField] private bool rotateCowbots = false;
    [SerializeField, Range(0, 2)] private float speedMultiplier = 0.5f; 
    private Vector2 cowbotVelocity;

    void Start()
    {
        if (rotateCowbots)
        {
            // Create a random background angle
            var angle = Random.Range(-15f, 15f);
            cowboys.transform.eulerAngles = angle * Vector3.forward;
            // Background moves along same angle as it is tilted, but horizontally
            cowbotVelocity = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * speedMultiplier;
        }
        else {
            // Create a random angle to move towards
            Vector2 angle = new Vector2(Random.Range(-1, 1), Random.Range(-1, 1));
            
            // If random returns zero, reset to 1
            if (angle == Vector2.zero)
                angle = Vector2.right;
            
            angle.Normalize();
            
            // Apply the random tiling direction
            cowbotVelocity = angle * speedMultiplier;
        }
    }

    void Update()
    {
        // Move the cowboys
        var uv = cowboys.uvRect;
        cowboys.uvRect = new Rect(uv.x + cowbotVelocity.x * Time.deltaTime, uv.y + cowbotVelocity.y * Time.deltaTime, uv.width, uv.height);
    }
}
