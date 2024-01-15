using CollectionExtensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class AIManager : PlayerManager
{
    private NavMeshAgent agent;
    public Transform DestinationTarget;
    public Transform ShootingTarget;
    public List<PlayerManager> TrackedPlayers;
    private float autoAwareRadius = 25f;
    private float ignoreAwareRadius = 100f;
    [SerializeField]
    private Animator animator;
    private bool isDead = false;
    [SerializeField]
    private LayerMask ignoreMask;
    [SerializeField]
    private LayerMask playerMask;
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
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, autoAwareRadius);
    }

    private IEnumerator LookForTargets()
    {
        if (!isDead)
        {
            Transform closestPlayer = null;
            float closestDistance = -1f;
            foreach (var player in TrackedPlayers)
            {
                var targetDirection = player.AiTarget.transform.position - transform.position;
                var hitDistance = targetDirection.magnitude;
                var playerDistance = (player.transform.position - transform.position).magnitude;
                // Is the player nearby?
                if (playerDistance > ignoreAwareRadius)
                    continue;
                if (playerDistance > autoAwareRadius)
                {
                    // Is the tracked player in front of me? (viewable)
                    if (Vector3.Dot(transform.forward, targetDirection) < 0)
                        continue;
                    // Is there a line of sight to a tracked player?
                    if (Physics.Raycast(transform.position, targetDirection, hitDistance - 0.1f, ignoreMask))
                        continue;
                    // Is there another tracked player who is closer?
                    if (hitDistance < closestDistance)
                        continue;
                }

                closestPlayer = player.AiTarget.transform;
                closestDistance = hitDistance;
                DestinationTarget = closestPlayer;
                // Close enough to shoot!
                if ((player.transform.position - transform.position).magnitude < 15)
                    ShootingTarget = player.AiAimSpot;
            }

            if (closestPlayer == null)
            {
                ShootingTarget = null;
                if (DestinationTarget == null || (!DestinationTarget.gameObject.GetComponent<PlayerManager>() && !DestinationTarget.gameObject.activeInHierarchy))
                {
                    var target = MatchController.Singleton.GetRandomActiveChip();
                    if (target != null)
                        DestinationTarget = target;
                }
            }
            if (!DestinationTarget)
            {
                var player = TrackedPlayers.RandomElement();
                DestinationTarget = player.AiTarget;
                ShootingTarget = player.AiAimSpot;
            }
            agent.SetDestination(DestinationTarget.position);

            yield return new WaitForSeconds(1f);
            StartCoroutine(LookForTargets());
        }
    }

    void Update()
    {
#if UNITY_EDITOR
        foreach (var player in TrackedPlayers)
        {
            Debug.DrawLine(transform.position, player.transform.position, Color.blue);
        }
#endif
        if (!DestinationTarget || !agent.enabled)
            return;
#if UNITY_EDITOR
        Debug.DrawLine(transform.position, DestinationTarget.position, Color.green);
#endif
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
