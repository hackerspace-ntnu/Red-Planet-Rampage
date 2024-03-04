using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardButton : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private Transform pokerRoom;
    [SerializeField]
    private GameObject cardButton;


    public void Start()
    {
        GameObject levelCard = Instantiate(cardButton, canvas.transform);
        levelCard.transform.localPosition = new Vector3(300f,300f,0f);
    }
}
