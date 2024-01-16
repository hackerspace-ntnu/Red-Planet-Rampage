using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiddingAI : BiddingPlayer
{
    void Start()
    {
        playerManager.identity = GetComponent<PlayerIdentity>();
        chipText.text = playerManager.identity.chips.ToString();
        GetComponent<PlayerIK>().RightHandIKTarget = signTarget;
    }

}
