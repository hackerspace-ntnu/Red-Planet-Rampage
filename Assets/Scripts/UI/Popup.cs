using UnityEngine;
using Random = UnityEngine.Random;

public class Popup : MonoBehaviour
{
    [SerializeField]
    private float minTimeout = 2;

    [SerializeField]
    private float maxTimeout = 4;

    private Timer timer;

    private void Start()
    {
        timer = GetComponent<Timer>();
        timer.StartTimer(Random.Range(minTimeout, maxTimeout));
        timer.OnTimerRunCompleted += Disappear;
    }

    private void OnDestroy()
    {
        timer.OnTimerRunCompleted -= Disappear;
    }

    private void Disappear() => Destroy(gameObject);
}
