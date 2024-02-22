using CollectionExtensions;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Popup : MonoBehaviour
{
    [SerializeField] private Sprite[] images;

    [SerializeField] private float minTimeout = 2;

    [SerializeField] private float maxTimeout = 4;

    [SerializeField] private AudioGroup audioGroup;

    private Timer timer;

    private void Start()
    {
        PickRandomImage();
        StartTimer();
        audioGroup.Play(GetComponent<AudioSource>());
    }

    private void PickRandomImage()
    {
        var sprite = images.RandomElement();
        var frame = GetComponent<Image>();
        var rect = GetComponent<RectTransform>();

        // Popups should show as half their real size for better resolution
        frame.sprite = sprite;
        frame.SetNativeSize();
        rect.sizeDelta *= 0.5f;
    }

    private void StartTimer()
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
