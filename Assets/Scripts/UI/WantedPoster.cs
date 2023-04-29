using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WantedPoster : MonoBehaviour
{
    [SerializeField]
    private TMP_Text title;
    public TMP_Text Title { get { return title; } }

    [SerializeField]
    private TMP_Text subtitle;
    public TMP_Text Subtitle { get { return subtitle; } }

    public Image photo;

    [SerializeField]
    private TMP_Text history;
    public TMP_Text History { get { return history; } }

    [SerializeField]
    private TMP_Text historyValue;
    public TMP_Text HistoryValue { get { return historyValue; } }

    [SerializeField]
    private Transform crimeContent;
    public Transform CrimeContent { get { return crimeContent; } }

    [SerializeField]
    private TMP_Text totalValue;
    public TMP_Text TotalValue { get { return totalValue; } }

}
