using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunBarrel : GunModifyer
{
    
    public override int priority { get => 1; }

    // The barel decides the type of projectile to shoot

    [SerializeField]
    private GameObject projectile;


    // Instantiates the projectile before returning it so that we dont accidentaly modify the prefab through scripts
    public GameObject Projectile { get{
            var instance = Instantiate(projectile);
            instance.SetActive(false);
            return instance;
        }
    }

    // Where to attach extensions
    public Transform[] attachmentPoints;

    // Where to shoot bullets
    public Transform[] outPuts;
}
