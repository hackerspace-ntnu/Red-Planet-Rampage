using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField]
    private Image radialTimer;

    private float incrementTimer = 360f;
    

    void Start()
    {
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
