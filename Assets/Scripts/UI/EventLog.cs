using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

internal struct EventLogItem
{
    public TMP_Text text;
    public float creationTime;
}

public class EventLog : MonoBehaviour
{
    [SerializeField]
    private RectTransform itemHolder;

    [SerializeField]
    private TMP_Text[] items;

    [SerializeField]
    private float expiryTime = 5;

    private Queue<TMP_Text> availableItems = new();
    private Queue<EventLogItem> itemsInUse = new();

    private Dictionary<TMP_Text, int> tweens = new();

    private int currentIndex = 0;

    public static EventLog Singleton { get; private set; }

    private void Awake()
    {
        #region Singleton boilerplate

        if (Singleton != null)
        {
            if (Singleton != this)
            {
                Debug.LogWarning($"There's more than one {Singleton.GetType()} in the scene!");
                Destroy(gameObject);
            }

            return;
        }

        Singleton = this;

        #endregion Singleton boilerplate

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        foreach (var item in items)
        {
            item.alpha = 0;
            availableItems.Enqueue(item);
            tweens.Add(item, 0);
        }
    }

    public void Log(string message)
    {
        TMP_Text item;
        if (availableItems.Count > 0)
            item = availableItems.Dequeue();
        else
            item = itemsInUse.Dequeue().text;

        if (LeanTween.isTweening(tweens[item]))
            LeanTween.cancel(tweens[item]);

        item.text = message;
        item.alpha = 1;

        // New items appear on top
        item.transform.SetAsFirstSibling();

        itemsInUse.Enqueue(new EventLogItem()
        {
            creationTime = Time.time,
            text = item,
        });
    }

    private void Update()
    {
        while (itemsInUse.Count > 0 && Time.time - itemsInUse.Peek().creationTime > expiryTime)
        {
            // Make available
            var item = itemsInUse.Dequeue();
            availableItems.Enqueue(item.text);

            if (LeanTween.isTweening(tweens[item.text]))
                LeanTween.cancel(tweens[item.text]);

            // Fade out
            tweens[item.text] = LeanTween.value(item.text.gameObject, SetAlpha(item.text), 1, 0, .2f).id;
        }
    }

    private Action<float> SetAlpha(TMP_Text text) => (float alpha) => text.alpha = alpha;
}
