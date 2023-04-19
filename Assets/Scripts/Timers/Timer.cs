using UnityEngine;
using System;
using System.Collections.Generic;
using System.Data;



public class Timer : MonoBehaviour
{
    private bool running = false;
    public bool IsRunning  => running;

    public float ElapsedTime { get; set; } = 0;
    public float WaitTime => baseWaitTime + additionalWaitTime;
    public float Overtime => additionalWaitTime;

    [Header("Timer settings are controlled by other components")]
    [SerializeField, Uneditable]
    private float baseWaitTime = 0f;
    [SerializeField, Uneditable]
    private float additionalWaitTime = 0f;
    [SerializeField, Uneditable]
    private bool repeat = false;

    /// <summary>
    /// Runs once upon starting a timer
    /// </summary>
    public event Action OnStartTimer;
    /// <summary>
    /// Runs once upon starting a timer, and once upon every subsequent restart given repeat=true
    /// </summary>
    public event Action OnTimerRunStarted;
    /// <summary>
    /// Runs once every time the timer elapsedTime 
    /// </summary>
    public event Action OnTimerRunCompleted;
    /// <summary>
    /// Runs once when the timer stops.
    /// </summary>
    public event Action OnStopTimer;
    /// <summary>
    /// Runs every time update changes time 
    /// </summary>
    public event Action OnTimerUpdate;

    public event Action OnTimerPaused;

    public event Action OnTimerResumed;

    public void StartTimer(float baseWaitTime, bool repeating = false)
    {
        if (IsRunning)
        {
            Debug.LogWarning("Timer Started though it's already active!");
            return;
        }

        this.baseWaitTime = baseWaitTime;
        repeat = repeating;
        running = true;
        OnStartTimer?.Invoke();
        OnTimerRunStarted?.Invoke();
    }

    public void AddTime(float addedTime)
    {
        additionalWaitTime += addedTime;
    }

    public void PauseTimer()
    {
        if (!IsRunning)
        {
            Debug.LogWarning("Timer paused though it's not active!");
            return;
        }

        running = false;
        OnTimerPaused?.Invoke();
    }

    public void ResumeTimer()
    {
        if (!IsRunning)
        {
            Debug.LogWarning("Timer resumed though it's already active!");
            return;
        }

        running = true;
        OnTimerResumed?.Invoke();
    }

    public void StopTimer()
    {
        if (!IsRunning)
        {
            Debug.LogWarning("Timer stopped though it's not active!");
            return;
        }
        OnStopTimer?.Invoke();
        ElapsedTime = 0f;
        repeat = false;
        running = false;
    }
    /// <summary>
    /// End the current run (loop if repeating).
    /// NOTE: This will also call <code>StopTimer()</code> if the timer is not set to repeat.
    /// </summary>
    public void EndTimerRun()
    {
        if (!IsRunning)
        {
            Debug.LogWarning("Timer ended run though it's not active!");
            return;
        }
        OnTimerRunCompleted?.Invoke();
        if (repeat)
        {
            ElapsedTime = 0;
            OnTimerRunStarted?.Invoke();
        }
        else
        {
            StopTimer();
        }
    }

    private void Update()
    {
        if (!IsRunning) 
            return;

        OnTimerUpdate?.Invoke();

        if (ElapsedTime > WaitTime)
        {
            // Any delegate can, and is highly likely to be set to stop the timer,
            // therefore we must make sure we're still running before we execute
            // any further calls.
            if (!IsRunning)
                return;
            OnTimerRunCompleted?.Invoke();
            if (repeat)
            {
                // The same applies as above.
                if (!IsRunning) 
                    return;
                ElapsedTime -= baseWaitTime;
                OnTimerRunStarted?.Invoke();
            }
            else
            {
                StopTimer();
            }
        }
        ElapsedTime += Time.deltaTime;
    }
}
