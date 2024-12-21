using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnDeath : MonoBehaviour
{
    [SerializeField]
    private GameObject doomedObject;
    [SerializeField]
    private HealthController healthController;

    private void Start()
    {
        healthController.onDeath += OnDeath;
    }

    private void OnDeath(HealthController healthController, float damage, DamageInfo info)
    {
        Destroy(doomedObject);
    }
}
