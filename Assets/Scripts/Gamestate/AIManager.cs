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
    private int collisionLayers;
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
        aiTargetCollider = Instantiate(aiTarget).GetComponent<AITarget>();
        aiTargetCollider.Owner = this;
        aiTargetCollider.transform.position = transform.position;
        collisionLayers = LayerMask.GetMask("Default", "HitBox");
        StartCoroutine(LookForTargets());
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
        aiTarget.SetActive(false);
        TurnIntoRagdoll(info);
        agent.enabled = false;
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
            yield return new WaitForSeconds(5f);
            Transform closestPlayer = null;
            float closestDistance = -1f;
            for (int i = 0; i < TrackedPlayers.Count - 1; i++)
            {
                if (!TrackedPlayers[i].AiTarget.gameObject.activeInHierarchy)
                    continue;
                if (!Physics.Raycast(transform.position, TrackedPlayers[i].AiTarget.transform.position - transform.position, out RaycastHit hit, 30f))
                    continue;
                if (hit.distance < closestDistance)
                    continue;
                closestPlayer = TrackedPlayers[i].AiTarget.transform;
                closestDistance = hit.distance;
                DestinationTarget = closestPlayer;
                ShootingTarget = TrackedPlayers[i].transform;
            }
            if (closestPlayer != null)
                Debug.Log("CHASING " + closestPlayer.name);
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

            StartCoroutine(LookForTargets());
        }
    }

    void Update()
    {
        //Debug.DrawRay(transform.position, TrackedPlayers[0].AiTarget.transform.position - transform.position, Color.red, 30f);
        if (!DestinationTarget || !agent.enabled)
            return;
        agent.SetDestination(DestinationTarget.position);
        animator.SetFloat("Forward", Vector3.Dot(agent.velocity, transform.forward) / agent.speed);
        animator.SetFloat("Right", Vector3.Dot(agent.velocity, transform.right) / agent.speed);
        if (ShootingTarget)
            GunOrigin.LookAt(ShootingTarget.position, transform.up);
    }
}
