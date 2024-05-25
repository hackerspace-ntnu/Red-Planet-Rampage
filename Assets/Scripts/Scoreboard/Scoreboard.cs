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

    private PlayerManager player;

    private AudioSource audioSource;

    public int TotalCrimes { get => crimeData.Count; }

    [SerializeField]
    private TMP_Text title;

    [SerializeField]
    private TMP_Text subtitle;

    [SerializeField]
    private TMP_Text progressDescription;

    public Image photo;

    [SerializeField]
    private GameObject wantedPoster;

    [SerializeField]
    private GameObject progressPoster;

    [SerializeField]
    private GameObject[] progressCrosses;

    [SerializeField]
    private Transform crimeContent;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject crimeTextPrefab;


    private int currentStep;
    public int CurrentStep { get => currentStep; }

    private List<(string, string)> crimeData = new();
    private List<(TMP_Text label, TMP_Text amount, Renderer chip)> crimeTextComponents = new();

    void Start()
    {
        scoreboardManager = ScoreboardManager.Singleton;
        audioSource = GetComponent<AudioSource>();
    }

    public void SetupPoster(PlayerManager player, string subtitle)
    {
        wantedPoster.GetComponent<Image>().color = player.identity.color;
        this.subtitle.text = subtitle;
        this.player = player;
        progressPoster.GetComponent<Image>().color = player.identity.color;
        progressDescription.text = player.identity.playerName;

        scoreboardManager.ShowNextCrime += NextStep;
        scoreboardManager.ShowVictoryProgress += ShowVictoryProgress;
    }

    public void AddPosterCrime(string crimeLabel, int Value)
    {
        var crime = Instantiate(crimeTextPrefab, crimeContent);
        var texts = crime.GetComponentsInChildren<TMP_Text>();
        var chip = crime.GetComponentInChildren<Renderer>();
        chip.enabled = false;

        crimeTextComponents.Add((texts[0], texts[1], chip));
        crimeData.Add((crimeLabel, Value.ToString()));
    }

    public void AddBlankPoster()
    {
        var crime = Instantiate(crimeTextPrefab, crimeContent);
        var texts = crime.GetComponentsInChildren<TMP_Text>();
        crime.GetComponentInChildren<Renderer>().enabled = false;

        crimeTextComponents.Add((texts[0], texts[1], null));
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
            var crime = crimeTextComponents[index];
            crime.label.text = crimeData[index].Item1;
            crime.amount.text = crimeData[index].Item2;
            if (crime.chip) crime.chip.enabled = true;
        }
    }

    private IEnumerator DelayDisplayCrimes(int delay)
    {
        yield return new WaitForSeconds(delay);
    }

    private void ShowVictoryProgress()
    {
        StartCoroutine(AnimateVictoryProgress());
    }

    private IEnumerator AnimateVictoryProgress()
    {
        wantedPoster.SetActive(false);
        progressPoster.SetActive(true);
        var delayTime = 0.5f;
        for (int i = 0; i < MatchController.Singleton.PlayerWins(player); i++)
        {
            progressCrosses[i].SetActive(true);
            progressCrosses[i].LeanScale(new Vector3(2f, 2f, 2f), 0.5f).setEasePunch();
            yield return new WaitForSeconds(delayTime);
        }
        yield return new WaitForSeconds(scoreboardManager.matchProgressDelay - delayTime * 3);
    }
}
