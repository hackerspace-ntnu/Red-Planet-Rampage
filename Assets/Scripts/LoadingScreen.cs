using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField]
    private Image radialTimer;

    [SerializeField]
    private TMP_Text tipsText;

    private float incrementTimer = 360f;
    private string currentTip;

    private string dash = "A clever trick awaits: crouch, jump, then spring off any surface for a speedy dash to victory";
    private string leap = "You can spring into action by crouching and then leaping in any direction with a jump";
    private string weaponCombinations = " With more than 250 weapon combinations available, you've got endless strategies to explore"; 
    private string skate = "You can get creative and use the pan as a makeshift skateboard for some unexpected moves";
    private string auctionContest = "Bidding strategically to throw off your opponents can be just as effective as going for the augment yourself";
    private string explosiveBarrels = "Leveraging explosive barrels can help you seize the high ground advantage";
    private string saveChips = "Saving chips could be the secret to dominating the auction later on";

    private List<string> tips; 


    void Start()
    {       
        tips = new List<string>{dash, leap, weaponCombinations, skate, auctionContest, explosiveBarrels, saveChips}; 
        int randomIndex = Random.Range(0, tips.Count);
        currentTip = tips[randomIndex];
        tipsText.text = currentTip;

        StartCoroutine(UpdateTimer());

    }
    private IEnumerator UpdateTimer(){
        radialTimer.material = Instantiate(radialTimer.material);

        for(int i = 0; i < 6;i++){
            yield return new WaitForSeconds(1);
            incrementTimer -= 60f;
            radialTimer.material.SetFloat("_Arc2",incrementTimer);
        }
    }
}
