using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WantedPoster : MonoBehaviour
{
    public int TotalCrimes { get => crimeData.Count; }

    [Header("Prefabs")]
    [SerializeField]
    private GameObject crimeTextPrefab;

    [Header("References")]
    [SerializeField]
    private TMP_Text title;

    [SerializeField]
    private TMP_Text subtitle;

    public Image photo;

    [SerializeField]
    private Image background;

    [SerializeField]
    private Transform crimeContent;


    private int currentStep;
    public int CurrentStep { get => currentStep; }

    private List<(string, string)> crimeData = new();
    private List<(TMP_Text, TMP_Text)> crimeTextComponents = new();

    private Scoreboard scoreboard;

    public void SetupPoster(Player player, string subtitle)
    {
        background.color = player.playerIdentity.color;
        this.subtitle.text = subtitle;
        scoreboard = GetComponentInParent<Scoreboard>();

        scoreboard.ShowNextCrime += NextStep;
    }

    public void AddPosterCrime(string crimeLabel, int Value)
    {
        GameObject crime = Instantiate(crimeTextPrefab, crimeContent);
        TMP_Text[] texts = crime.GetComponentsInChildren<TMP_Text>();

        crimeTextComponents.Add((texts[0], texts[1]));
        crimeData.Add((crimeLabel, Value.ToString()));
    }

    private void NextStep()
    {
        DisplayCrime(currentStep);
        currentStep++;
    }

    private void DisplayCrime(int index)
    {
        if (index <= crimeData.Count - 1)
        {
            crimeTextComponents[index].Item1.text = crimeData[index].Item1;
            crimeTextComponents[index].Item2.text = crimeData[index].Item2;
        }
    }
}

