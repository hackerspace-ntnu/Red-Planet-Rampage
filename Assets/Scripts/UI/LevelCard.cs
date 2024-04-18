using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCard : MonoBehaviour
{
    [SerializeField]
    private GameObject levelCard;
    [SerializeField]
    private string cardName;

    public GameObject getLevelCard()
    {
        if (levelCard != null) {
            return levelCard;
        }
        return null;
    }

    public string getCardName()
    {
        if (!string.IsNullOrEmpty(cardName))
        {
            return cardName;
        }
        return null;
    }
}
