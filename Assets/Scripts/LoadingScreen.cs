using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CollectionExtensions;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField]
    private Image radialTimer;

    [SerializeField]
    private GameObject loadingBar;

    [SerializeField]
    private GameObject staticText;

    [SerializeField]
    private TMP_Text tipsText;

    [SerializeField]
    private GameObject keybinds;

    private float incrementTimer = 360f;

    [SerializeField]
    private List<string> tips;

    private float rotateSpeed = 60;

    private static int loadingCounter = 0;

    void Awake()
    {
        PlayerInputManagerController.Singleton.RemoveListeners();
        loadingCounter += 1;
        Debug.Log(loadingCounter);
    }
    void Start()
    {   
        tipsText.text = tips.RandomElement();

        if(loadingCounter <= 2){

            showInstructions();
        }

        StartCoroutine(UpdateTimer());
    }

    private IEnumerator UpdateTimer()
    {
        radialTimer.material = Instantiate(radialTimer.material);

        for(int i = 0; i < 6;i++)
        {
            yield return new WaitForSeconds(1);
            incrementTimer -= 60f;
            radialTimer.material.SetFloat("_Arc2",incrementTimer);

            if(i == 4){
                rotateSpeed *= 2;
            }
        }
    }

    private void showInstructions(){
        tipsText.text = "";
        staticText.SetActive(false);

        if (loadingCounter == 1) {
            keybinds.SetActive(true);
        } else {

            tipsText.text="Bidding instructions";
        }
    }
    
    void Update()
    {
        loadingBar.transform.Rotate(Vector3.forward, Time.deltaTime * rotateSpeed);
    }
}
