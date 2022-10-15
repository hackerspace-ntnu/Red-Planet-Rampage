using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FireRateController
{
    public abstract bool shouldFire(bool triggerPressed, bool triggerHeld);
}
public class SemiAutoFirerateController: FireRateController
{
    private float fireDelay;
    private float lastFired = 0f;

    public SemiAutoFirerateController(float fireRate)
    {
        this.fireDelay = 1/fireRate;
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
    private float lastFired = 0f;

    public FullAutoFirerateController(float fireRate)
    {
        this.fireDelay = 1 / fireRate;
    }
    public override bool shouldFire(bool triggerPressed, bool triggerHeld)
    {
        if (triggerHeld && lastFired + fireDelay < Time.fixedTime)
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
            currentBurstNum = (currentBurstNum + 1)%burst;
            return true;
        }
        return false;
    }
}
public abstract class GunController : MonoBehaviour
{
    [SerializeField]
    protected Transform output;

    protected FireRateController fireRateController;

    public GunBaseStats stats;
    public bool triggerHeld, triggerPressed;


    private void Start()
    {
        if(stats == null){ stats = new GunBaseStats(); }

        switch (stats.fireMode)
        {
            case GunBaseStats.FireModes.semiAuto:
                fireRateController = new SemiAutoFirerateController(stats.firerate);
                break;
            case GunBaseStats.FireModes.burst:
                fireRateController = new BurstFirerateController(stats.firerate, stats.burstNum);
                break;
            case GunBaseStats.FireModes.fullAuto:
                fireRateController = new FullAutoFirerateController(stats.firerate);
                break;
            default:
                break;
        }

    }

    private void FixedUpdate()
    {
        if (fireRateController.shouldFire(triggerPressed, triggerHeld))
        {
            fireGun();
        }
    }
    protected abstract void fireGun();
}

