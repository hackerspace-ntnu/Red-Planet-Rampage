using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIManager : PlayerManager
{
    private NavMeshAgent agent;
    public Transform mainPlayer;
    [SerializeField]
    private Animator animator;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        identity = GetComponent<PlayerIdentity>();
        healthController = GetComponent<HealthController>();
        healthController.onDamageTaken += OnDamageTaken;
        healthController.onDeath += OnDeath;
        SetGun(GunHolder);
        GunController.triggerPressed = true;
        GunController.triggerHeld = true;
        meshBase.GetComponentInChildren<SkinnedMeshRenderer>().material.color = identity.color;
    }

    public override void SetGun(Transform offset)
    {
        overrideAimTarget = false;
        var gun = GunFactory.InstantiateGunAI(identity.Body, identity.Barrel, identity?.Extension, this, offset);
        gunController = gun.GetComponent<GunController>();
        gunController.onFireStart += UpdateAimTarget;
        gunController.onFire += UpdateAimTarget;
        playerIK.LeftHandIKTarget = gunController.LeftHandTarget;
        if (gunController.RightHandTarget)
            playerIK.RightHandIKTarget = gunController.RightHandTarget;
        GetComponent<AmmoBoxCollector>().CheckForAmmoBoxBodyAgain();
    }

    private void OnDamageTaken(HealthController healthController, float damage, DamageInfo info)
    {
        if (info.sourcePlayer != this)
        {
            lastPlayerThatHitMe = info.sourcePlayer;
        }
    }
    void OnDeath(HealthController healthController, float damage, DamageInfo info)
    {
        var killer = info.sourcePlayer;
        if (info.sourcePlayer == this && lastPlayerThatHitMe)
        {
            killer = lastPlayerThatHitMe;
        }
        onDeath?.Invoke(killer, this);
        aimAssistCollider.SetActive(false);
        TurnIntoRagdoll(info);
    }

    private void UpdateAimTarget(GunStats stats)
    {

    }

    void Update()
    {
        if (!mainPlayer)
            return;
        agent.SetDestination(mainPlayer.position);
        animator.SetFloat("Forward", Vector3.Dot(agent.velocity, transform.forward) / agent.speed);
        animator.SetFloat("Right", Vector3.Dot(agent.velocity, transform.right) / agent.speed);
    }
}
