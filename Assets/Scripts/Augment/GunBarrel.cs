using UnityEngine;

[RequireComponent(typeof(ProjectileController))]
public class GunBarrel : Augment
{ 
    // Where to attach extensions
    public Transform[] attachmentPoints;

    public  ProjectileController Projectile{get => GetComponent<ProjectileController>();}
}
