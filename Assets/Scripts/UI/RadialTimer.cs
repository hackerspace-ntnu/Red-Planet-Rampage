using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RadialTimer : MonoBehaviour
{
    [SerializeField]
    private Timer timer;

    [SerializeField]
    private TMP_Text text;

    [SerializeField]
    private Image progressCircle;

    private void Start()
    {
        progressCircle.material = Instantiate(progressCircle.material);
        progressCircle.material.SetFloat("_Arc2", 0);
        timer.OnTimerUpdate += OnTimerUpdate;
    }

    private void OnDestroy()
    {
        timer.OnTimerUpdate -= OnTimerUpdate;
    }
    private void OnTimerUpdate()
    {
        text.text = Mathf.Round(timer.WaitTime - timer.ElapsedTime).ToString();
        progressCircle.material.SetFloat("_Arc1", 360f - 360f * ((timer.WaitTime - timer.ElapsedTime) / timer.WaitTime));
    }
}
