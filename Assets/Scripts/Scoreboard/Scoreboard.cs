using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class Scoreboard : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform content;

    private ScoreboardManager scoreboardManager;

    private Player player;

    private AudioSource audioSource;

    public int TotalCrimes { get => crimeData.Count; }

    [SerializeField]
    private TMP_Text title;

    [SerializeField]
    private TMP_Text subtitle;

    public Image photo;

    [SerializeField]
    private Image background;

    [SerializeField]
    private Transform crimeContent;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject crimeTextPrefab;


    private int currentStep;
    public int CurrentStep { get => currentStep; }

    private List<(string, string)> crimeData = new();
    private List<(TMP_Text, TMP_Text)> crimeTextComponents = new();

    void Start()
    {
        scoreboardManager = ScoreboardManager.Singleton;
        audioSource = GetComponent<AudioSource>();
    }

    public void SetupPoster(Player player, string subtitle)
    {
        background.color = player.playerIdentity.color;
        this.subtitle.text = subtitle;
        this.player = player;

        scoreboardManager.ShowNextCrime += NextStep;
    }

    public void AddPosterCrime(string crimeLabel, int Value)
    {
        GameObject crime = Instantiate(crimeTextPrefab, crimeContent);
        TMP_Text[] texts = crime.GetComponentsInChildren<TMP_Text>();

        crimeTextComponents.Add((texts[0], texts[1]));
        crimeData.Add((crimeLabel, Value.ToString()));
    }

    public void AddBlankPoster()
    {
        GameObject crime = Instantiate(crimeTextPrefab, crimeContent);
        TMP_Text[] texts = crime.GetComponentsInChildren<TMP_Text>();

        crimeTextComponents.Add((texts[0], texts[1]));
        crimeData.Add(("", ""));
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
    
    private IEnumerator DelayDisplayCrimes(int delay)
    {
        yield return new WaitForSeconds(delay);
    }
}
