using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class that can be extended in order to give a behavioural property that can be applied to any projectile created by any set of parts.
/// Why? Because putting a copy of every needed bullet script on every single bullet that is created is very inefficient.
/// 
/// Bullet modifier are set by putting all needed modifier scripts on a prefab, and assigning that prefab to the "Bullet Modifier Prefab" property in the editor found on augments.
/// 
/// Note: Behaviour tied to updating the projectile every frame (Update), should be done with ProjectileController delegates instead!
/// </summary>
public abstract class ProjectileModifier : MonoBehaviour
{
    //public ProjectileController projectile;

    public abstract void Attach(ProjectileController projectile);
    public abstract void Detach(ProjectileController projectile);
}
