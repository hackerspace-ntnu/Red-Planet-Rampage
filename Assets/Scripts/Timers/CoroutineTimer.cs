using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class CoroutineTimer
{
    public event Action OnWaitStart;
    public event Action OnWaitCompleted;
    public float ElapsedTime { get; set; } = 0;

    [SerializeField]
    private float waitTime = 10f;
    [SerializeField]
    private bool repeating = true;


    private Coroutine timerRoutine = null;
    public void Start(MonoBehaviour source)
    {
        timerRoutine = source.StartCoroutine(Run());
    }

    public IEnumerator Run()
    {
        do
        {
            OnWaitStart?.Invoke();
            yield return new WaitForSeconds(waitTime);
            OnWaitCompleted?.Invoke();
        }
        while (repeating);
    }
    public void Stop(MonoBehaviour source)
    {
        source.StopCoroutine(timerRoutine);
    }
}
