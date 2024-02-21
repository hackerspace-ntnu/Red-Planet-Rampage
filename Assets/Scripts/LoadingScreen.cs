using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CollectionExtensions;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Image radialTimer;

    [SerializeField] private GameObject loadingBar;

    [SerializeField] private GameObject staticText;

    [SerializeField] private TMP_Text tipsText;

    [SerializeField] private GameObject keybinds;

    [SerializeField] private GameObject auctionInstructions;

    [SerializeField] private List<string> tips;

    private float incrementTimer = 360f;
    private float rotateSpeed = 60;

    private static int loadingCounter = 0;

    private void Awake()
    {
        PlayerInputManagerController.Singleton.RemoveListeners();
        loadingCounter += 1;
    }

    private void OnEnable()
    {
        ShowContent();
        StartCoroutine(UpdateTimer());
    }

    private IEnumerator UpdateTimer()
    {
        radialTimer.material = Instantiate(radialTimer.material);

        for (int i = 0; i < 6; i++)
        {
            yield return new WaitForSeconds(1);
            incrementTimer -= 60f;
            radialTimer.material.SetFloat("_Arc2", incrementTimer);

            if (i == 4)
            {
                rotateSpeed *= 2;
            }
        }
    }

    private void ShowContent()
    {
        tipsText.text = "";
        staticText.SetActive(false);

        var isFirstLoadingScreen = loadingCounter == 1;
        var isBeforeFirstAuction = MatchController.Singleton?.RoundCount == 1 && !MatchController.Singleton.IsAuction;

        if (isFirstLoadingScreen)
        {
            keybinds.SetActive(true);
        }
        else if (isBeforeFirstAuction)
        {
            auctionInstructions.SetActive(true);
        }
        else
        {
            staticText.SetActive(true);
            tipsText.text = tips.RandomElement();
        }
    }

    private void Update()
    {
        loadingBar.transform.Rotate(Vector3.forward, Time.deltaTime * rotateSpeed);
    }
}
