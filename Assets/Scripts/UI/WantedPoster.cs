using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WantedPoster : MonoBehaviour
{
    [SerializeField]
    private TMP_Text title;
    public TMP_Text Title => title;

    [SerializeField]
    private TMP_Text subtitle;
    public TMP_Text Subtitle => subtitle;

    public Image photo;

    [SerializeField]
    private TMP_Text history;
    public TMP_Text History => history;

    [SerializeField]
    private TMP_Text historyValue;
    public TMP_Text HistoryValue => historyValue;

    [SerializeField]
    private Transform crimeContent;
    public Transform CrimeContent => crimeContent;

    [SerializeField]
    private TMP_Text totalValue;
    public TMP_Text TotalValue => totalValue;

}
