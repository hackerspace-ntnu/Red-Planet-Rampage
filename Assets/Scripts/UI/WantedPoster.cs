using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem;

public class WantedPoster : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private TMP_Text title;

    [SerializeField]
    private TMP_Text subtitle;

    public Image photo;

    [SerializeField]
    private Transform crimeContent;

    [SerializeField]
    private AnimationClip cameraEndRoundPan;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject crimeTextPrefab;

    private Player player;
    private MatchController matchController;
    private int roundSpoils;

    private float roundEndDelay;
    private int totalSteps;
    private int currentStep;
    private List<RectTransform> animatedCrimes = new List<RectTransform>();

    public void SetupPoster(Player player, MatchController matchController, string subtitle)
    {
        this.player = player;
        this.matchController = matchController;
        this.subtitle.text = subtitle;
        this.roundEndDelay = matchController.RoundEndDelay;
    }

    private void AddPosterCrime(string Crime, int Value)
    {
        GameObject crime = Instantiate(crimeTextPrefab, crimeContent);
        TMP_Text[] texts = crime.GetComponentsInChildren<TMP_Text>();

        texts[0].text = Crime;
        texts[1].text = Value.ToString();

        animatedCrimes.Add(crime.GetComponent<RectTransform>());
        crime.SetActive(false);
    }

    public void UpdatePosterValues()
    {
        if(matchController == null)
            matchController = MatchController.Singleton;

        Round lastRound = matchController.GetLastRound();
        
        // Add the different crimes players can gain money from
        
        // Previous savings
        AddPosterCrime("Savings", player.playerIdentity.chips);

        // Participation award
        AddPosterCrime("Base", matchController.ChipBaseReward);

        // Kill Award
        if(lastRound.KillCount(player.playerManager) != 0)
        {
            AddPosterCrime("Kills", matchController.ChipKillReward * lastRound.KillCount(player.playerManager));
        }

        // Round winner award
        if (lastRound.IsWinner(player.playerIdentity)){
            AddPosterCrime("Victor", matchController.ChipWinReward);
        }

        // New total
        roundSpoils = matchController.ChipBaseReward 
            + matchController.ChipKillReward * lastRound.KillCount(player.playerManager) 
            + (lastRound.IsWinner(player.playerIdentity) ? matchController.ChipWinReward : 0);

        AddPosterCrime("Total", roundSpoils);

        // Animate the crimes
        totalSteps = animatedCrimes.Count;
        currentStep = 0;
        float secondsPerStep = matchController.RoundEndDelay / totalSteps;
        StartCoroutine(Animate(cameraEndRoundPan.length, secondsPerStep));

        // Add player input
        player.playerManager.inputManager.onSelect += NextStep;
    }
    private void NextStep(InputAction.CallbackContext ctx)
    {
        DisplayCrime(currentStep);
        currentStep++;        
    }
    private void NextStep()
    {
        DisplayCrime(currentStep);
        currentStep++;
    }

    private void DisplayCrime(int index)
    {
         animatedCrimes[index].gameObject.SetActive(true);
    }

    private IEnumerator Animate(float startDelay, float secondsPerStep)
    {
        yield return new WaitForSeconds(startDelay);

        for(int i = 0; i < totalSteps; i++)
        {
            if(i == totalSteps - 1)
            {
                matchController.StartNextBidding();
            }
            NextStep();
            yield return new WaitForSeconds(secondsPerStep);
        }
    }
}

