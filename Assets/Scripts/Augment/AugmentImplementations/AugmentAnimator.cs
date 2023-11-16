using UnityEngine;

/// <summary>
/// Abstract for reload and fire animations.
/// Not an interface since interface types can't be editor fields :(
/// </summary>
public abstract class AugmentAnimator : MonoBehaviour
{
    public delegate void AnimationEvent();
    public AnimationEvent OnFireAnimationEnd;

    public abstract void OnInitialize(GunStats stats);
    public abstract void OnReload(int ammo);
    public abstract void OnFire(int remainingAmmo);
}
