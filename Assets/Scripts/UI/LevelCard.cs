using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCard : MonoBehaviour
{
    [SerializeField]
    private GameObject levelCard;
    public GameObject LevelCardPrefab => levelCard;

    [SerializeField]
    private Canvas canvas;

    public Canvas Canvas => canvas;

    [SerializeField]
    private string cardName;
    public string CardName => cardName;
}
