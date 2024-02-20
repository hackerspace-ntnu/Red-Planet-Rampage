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
    private TMP_Text tipsText;

    private float incrementTimer = 360f;

    [SerializeField]
    private List<string> tips;

    private float rotateSpeed = 60;
    
    void Awake()
    {
        PlayerInputManagerController.Singleton.RemoveListeners();
    }
    void Start()
    {   
        tipsText.text = tips.RandomElement();
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
    void Update()
    {
        loadingBar.transform.Rotate(Vector3.forward, Time.deltaTime * rotateSpeed);
    }
}
