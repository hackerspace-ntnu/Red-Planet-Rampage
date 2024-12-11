using System.Diagnostics.Eventing.Reader;
using UnityEngine;
using UnityEngine.UI;

public class TilingCowboys : MonoBehaviour
{
    [SerializeField] private RawImage cowboys;
    [SerializeField] private bool rotateCowboys = false;
    [SerializeField, Range(0, 2)] private float speedMultiplier = 0.5f; 
    private Vector2 cowboyVelocity;



    // Start is called before the first frame update
    void Start()
    {
        
        if (rotateCowboys)
        {
            // Create a random background angle
            var angle = Random.Range(-15f, 15f);
            cowboys.transform.eulerAngles = angle * Vector3.forward;
            // Background moves along same angle as it is tilted, but horizontally
            cowboyVelocity = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * speedMultiplier;
        }
        else {
            Vector2 angle = new Vector2(Random.Range(-1, 1), Random.Range(-1, 1));
            angle.Normalize();
            // Give a random tiling direction
            cowboyVelocity = angle * speedMultiplier;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Move the cowboys
        var uv = cowboys.uvRect;
        cowboys.uvRect = new Rect(uv.x + cowboyVelocity.x * Time.deltaTime, uv.y + cowboyVelocity.y * Time.deltaTime, uv.width, uv.height);
    }
}
