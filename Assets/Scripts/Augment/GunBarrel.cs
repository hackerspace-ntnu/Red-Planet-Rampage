using UnityEngine;

public class GunBarrel : GunModifier
{

    public override int priority { get => 1; }

    // The barel decides the type of projectile to shoot

    [SerializeField]
    private GameObject projectile;

    // Where to attach extensions
    public Transform[] attachmentPoints;

    // Instantiates the projectile before returning it so that we dont accidentaly modify the prefab through scripts
    public GameObject Projectile
    {
        get
        {
            var instance = Instantiate(projectile);
            instance.SetActive(false);
            return instance;
        }
    }
}
