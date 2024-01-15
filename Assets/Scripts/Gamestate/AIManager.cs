using CollectionExtensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class AIManager : PlayerManager
{
    private NavMeshAgent agent;
    public Transform DestinationTarget;
    public Transform ShootingTarget;
    public List<PlayerManager> TrackedPlayers;
    [SerializeField]
    private Animator animator;
    private bool isDead = false;
    [SerializeField]
    private LayerMask ignoreMask;
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        identity = GetComponent<PlayerIdentity>();
        healthController = GetComponent<HealthController>();
        healthController.onDamageTaken += OnDamageTaken;
        healthController.onDeath += OnDeath;
        SetGun(GunHolder);
        meshBase.GetComponentInChildren<SkinnedMeshRenderer>().material.color = identity.color;
        aiTargetCollider = Instantiate(aiTarget).GetComponent<AITarget>();
        aiTargetCollider.Owner = this;
        aiTargetCollider.transform.position = transform.position;
        StartCoroutine(LookForTargets());
        TrackedPlayers.ForEach(player => player.onDeath += RemovePlayer);
    }

    private void RemovePlayer(PlayerManager killer, PlayerManager victim)
    {
        TrackedPlayers.Remove(victim);
        victim.onDeath -= RemovePlayer;
    }

    public override void SetLayer(int playerIndex)
    {
        int playerLayer = LayerMask.NameToLayer("Player " + playerIndex);
        gameObject.layer = playerLayer;
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
        aiTargetCollider.gameObject.SetActive(false);
        TurnIntoRagdoll(info);
        agent.enabled = false;
        isDead = true;
    }

    private void UpdateAimTarget(GunStats stats)
    {
        if (!ShootingTarget)
            return;
        gunController.target = ShootingTarget.position;
    }

    private IEnumerator LookForTargets()
    {
        if (!isDead)
        {
            Transform closestPlayer = null;
            float closestDistance = -1f;
            for (int i = 0; i < TrackedPlayers.Count - 1; i++)
            {
                // Is the tracked player still alive?
                if (!TrackedPlayers[i].AiTarget.gameObject.activeInHierarchy)
                    continue;
                // Is the tracked player in front of me? (viewable)
                var playerDirection = TrackedPlayers[i].AiTarget.transform.position - transform.position;
                if (Vector3.Dot(transform.forward, playerDirection) < 0)
                    continue;
                var hitDistance = playerDirection.magnitude;
                // Is there a line of sight to a tracked player?
                if (Physics.Raycast(transform.position, playerDirection, hitDistance - 0.1f, ignoreMask))
                    continue;
                // Is there another tracked player who is closer?
                if (hitDistance < closestDistance)
                    continue;
                closestPlayer = TrackedPlayers[i].AiTarget.transform;
                closestDistance = hitDistance;
                DestinationTarget = closestPlayer;
                // Close enough to shoot!
                if (hitDistance < 15)
                    ShootingTarget = TrackedPlayers[i].AiAimSpot;
            }

            if (closestPlayer == null)
            {
                ShootingTarget = null;
                if (DestinationTarget == null || (!DestinationTarget.gameObject.GetComponent<PlayerManager>() && !DestinationTarget.gameObject.activeInHierarchy))
                {
                    var target = MatchController.Singleton.GetRandomActiveChip();
                    if (target == null)
                    {
                        var player = TrackedPlayers.RandomElement();
                        target = player.AiTarget;
                        ShootingTarget = player.AiAimSpot;
                    }
                        
                    DestinationTarget = target;
                }
            }
            if (DestinationTarget)
                agent.SetDestination(DestinationTarget.position);
            yield return new WaitForSeconds(2.5f);
            StartCoroutine(LookForTargets());
        }
    }

    void Update()
    {
        if (!DestinationTarget || !agent.enabled)
            return;
        animator.SetFloat("Forward", Vector3.Dot(agent.velocity, transform.forward) / agent.speed);
        animator.SetFloat("Right", Vector3.Dot(agent.velocity, transform.right) / agent.speed);
        if (!ShootingTarget)
            return;
        GunOrigin.LookAt(ShootingTarget.position, transform.up);
        Fire();
    }

    private void Fire()
    {
        if (!gunController)
            return;
        gunController.triggerHeld = true;
        gunController.triggerPressed = true;
        StartCoroutine(UnpressTrigger());
    }
}
