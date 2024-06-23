using UnityEngine;

/// <summary>
/// Abstract for reload and fire animations.
/// Not an interface since interface types can't be editor fields :(
/// </summary>
public abstract class AugmentAnimator : MonoBehaviour
{
    public delegate void AnimationEvent();
    public AnimationEvent OnShotFiredAnimation;
    public AnimationEvent OnAnimationEnd;

    public abstract void OnInitialize(GunStats stats);
    public abstract void OnReload(GunStats stats);
    public abstract void OnFire(GunStats stats);

    protected bool DoWeNeedToEscape()
    {
        try
        {
            if (!gameObject)
            {
                return true;
            }
        }
        catch (System.NullReferenceException)
        {
            return true;
        }
        return false;
    }
}
