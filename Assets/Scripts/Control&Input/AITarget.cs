using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITarget : MonoBehaviour
{
    public PlayerManager Owner;
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerManager playerManager))
            if (playerManager == Owner)
                transform.position = Owner.transform.position;
    }
}
