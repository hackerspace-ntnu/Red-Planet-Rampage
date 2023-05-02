using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WantedPoster : MonoBehaviour
{
    [SerializeField]
    private TMP_Text title;

    [SerializeField]
    private TMP_Text subtitle;

    public Image photo;

    [SerializeField]
    private TMP_Text savingsValue;

    [SerializeField]
    private Transform crimeContent;

    [SerializeField]
    private TMP_Text totalValue;

    [SerializeField]
    private GameObject crimeTextPrefab;

    private Player player;
    private MatchController matchController;
    private int roundSpoils;

    public void SetupPoster(Player player, MatchController matchController, string subtitle)
    {
        this.player = player;
        this.matchController = matchController;
        this.subtitle.text = subtitle;
    }

    private void AddPosterCrime(string Crime, int Value)
    {
        GameObject crime = Instantiate(crimeTextPrefab, crimeContent);
        TMP_Text[] texts = crime.GetComponentsInChildren<TMP_Text>();

        texts[0].text = Crime;
        texts[1].text = Value.ToString();
    }

    public void UpdatePosterValues()
    {
        if(matchController == null)
            matchController = MatchController.Singleton;

        Round lastRound = matchController.GetLastRound();
        int 
        savingsValue.text = player.playerIdentity.chips.ToString();

        AddPosterCrime("Base", matchController.ChipBaseReward);
        AddPosterCrime("Kills", matchController.ChipKillReward * lastRound.KillCount(player.playerManager));

        if (lastRound.IsWinner(player.playerIdentity)){
            AddPosterCrime("Victor", matchController.ChipWinReward);
        }

        roundSpoils = matchController.ChipBaseReward 
            + matchController.ChipKillReward * lastRound.KillCount(player.playerManager) 
            + (lastRound.IsWinner(player.playerIdentity) ? matchController.ChipWinReward : 0);

        LeanTween.value(totalValue.gameObject, RandomNumber, 0, player.playerIdentity.chips + roundSpoils, 5);
    }

    private void RandomNumber(float number)
    {
        if (number == player.playerIdentity.chips + roundSpoils)
        {
            totalValue.text = (player.playerIdentity.chips + roundSpoils).ToString("F0");
        }           
        else 
        {
            //totalValue.text = number.ToString("F0");
            totalValue.text = Random.Range(0, 99).ToString("F0");
        }
        //
    }
}

