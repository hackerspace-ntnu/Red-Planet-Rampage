using UnityEngine;

public abstract class FireRateController
{
    public abstract bool shouldFire(bool triggerPressed, bool triggerHeld);
}
public class SemiAutoFirerateController : FireRateController
{
    private float fireDelay;
    private float lastFired = 0f;

    public SemiAutoFirerateController(float fireRate)
    {
        this.fireDelay = 1 / fireRate;
    }
    public override bool shouldFire(bool triggerPressed, bool triggerHeld)
    {
        if (triggerPressed && lastFired + fireDelay < Time.fixedTime)
        {
            lastFired = Time.fixedTime;
            return true;
        }
        return false;
    }
}
public class FullAutoFirerateController : FireRateController
{
    private float fireDelay;
    private float lastFired;

    public FullAutoFirerateController(float fireRate)
    {
        fireDelay = 1 / fireRate;
        lastFired = Time.fixedTime;
    }
    public override bool shouldFire(bool triggerPressed, bool triggerHeld)
    {

        if (triggerHeld && (lastFired + fireDelay < Time.fixedTime))
        {
            lastFired = Time.fixedTime;
            return true;
        }
        return false;
    }
}
public class BurstFirerateController : FireRateController
{
    private float fireDelay;
    private float lastFired = 0f;
    private int burst;
    private int currentBurstNum = 0;
    public BurstFirerateController(float fireRate, int burst)
    {
        this.fireDelay = 1 / fireRate;
        this.burst = burst;
    }
    public override bool shouldFire(bool triggerPressed, bool triggerHeld)
    {
        if ((triggerPressed || (currentBurstNum > 0 && currentBurstNum < this.burst)) && lastFired + fireDelay < Time.fixedTime)
        {
            lastFired = Time.fixedTime;
            currentBurstNum = (currentBurstNum + 1) % burst;
            return true;
        }
        return false;
    }
}
