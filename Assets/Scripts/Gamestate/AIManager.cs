using CollectionExtensions;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

public class AIManager : PlayerManager
{
    private NavMeshAgent agent;
    public Transform DestinationTarget;
    public Transform ShootingTarget;
    public List<PlayerManager> TrackedPlayers;
    private const float autoAwareRadius = 25f;
    private const float ignoreAwareRadius = 100f;
    [SerializeField]
    private Animator animator;
    private bool isDead = false;
    [SerializeField]
    private LayerMask ignoreMask;
    public BiddingAI biddingAI;
    private Rigidbody body;
    [SerializeField]
    private AnimationCurve jumpYoffset;

    private delegate void NavMeshEvent();
    private NavMeshEvent onLinkStart;
    private NavMeshEvent onLinkEnd;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        body = GetComponent<Rigidbody>();
        healthController = GetComponent<HealthController>();
        agent.autoTraverseOffMeshLink = false;
        onLinkStart += AnimateOffMesh;
        onLinkEnd += AnimateStopCrouch;
        healthController.onDamageTaken += OnDamageTaken;
        healthController.onDeath += OnDeath;
        aiTargetCollider = Instantiate(aiTarget).GetComponent<AITarget>();
        aiTargetCollider.Owner = this;
        aiTargetCollider.transform.position = transform.position;
        StartCoroutine(LookForTargets());
        TrackedPlayers.ForEach(player => player.onDeath += RemovePlayer);
    }

    private void OnDestroy()
    {
        healthController.onDamageTaken -= OnDamageTaken;
        healthController.onDeath -= OnDeath;
        TrackedPlayers.ForEach(player => player.onDeath -= RemovePlayer);
    }

    public void SetIdentity(PlayerIdentity identity)
    {
        this.identity = identity;
        if (!biddingAI)
            SetGun(GunHolder);
        meshBase.GetComponentInChildren<SkinnedMeshRenderer>().material.color = identity.color;
        if (biddingAI)
            biddingAI.SetIdentity(identity);
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
        if (info.damageType != DamageType.Explosion)
            return;
        StartCoroutine(WaitAndToggleAgent());
    }

    public IEnumerator WaitAndToggleAgent()
    {
        agent.enabled = false;
        body.isKinematic = false;
        yield return new WaitForSeconds(0.5f);
        body.isKinematic = true;
        agent.enabled = true;
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
        agent.enabled = false;
        body.isKinematic = false;
        TurnIntoRagdoll(info);
        isDead = true;
    }

    private void UpdateAimTarget(GunStats stats)
    {
        if (!ShootingTarget)
            return;
        gunController.target = ShootingTarget.position
            + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f))
                * (transform.position - ShootingTarget.position).magnitude * 0.1f;
    }
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, autoAwareRadius);
    }

    private IEnumerator LookForTargets()
    {
        if (isDead)
            yield break;
        var previousDestination = DestinationTarget;
        Transform closestPlayer = null;
        float closestDistance = ignoreAwareRadius;
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
                if (hitDistance > closestDistance)
                    continue;
            }

            closestPlayer = player.AiTarget.transform;
            closestDistance = hitDistance;
            DestinationTarget = closestPlayer;
            ShootingTarget = player.AiAimSpot;
        }

        if (closestPlayer == null)
        {
            ShootingTarget = null;
            if (DestinationTarget == null || !DestinationTarget || (!DestinationTarget.gameObject.GetComponent<PlayerManager>() && !DestinationTarget.gameObject.activeInHierarchy))
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

        yield return new WaitForSeconds(3f);
        StartCoroutine(LookForTargets());
    }

    private void AnimateOffMesh()
    {
        animator.SetBool("Crouching", true);
        animator.SetTrigger("Leap");
        StartCoroutine(AnimateJumpCurve(0.7f));
    }

    private void AnimateStopCrouch()
    {
        animator.SetBool("Crouching", false);
    }

    IEnumerator AnimateJumpCurve(float duration)
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 startPos = agent.transform.position;
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;
        transform.LookAt(new Vector3(endPos.x, transform.position.y, endPos.z), transform.up);
        float normalizedTime = 0.0f;
        while (normalizedTime < 1.0f)
        {
            float yOffset = jumpYoffset.Evaluate(normalizedTime);
            agent.transform.position = Vector3.Lerp(startPos, endPos, normalizedTime) + yOffset * Vector3.up;
            normalizedTime += Time.deltaTime / duration;
            yield return null;
        }
        agent.CompleteOffMeshLink();
        onLinkEnd?.Invoke();
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
        if (agent.isOnOffMeshLink)
            onLinkStart?.Invoke();
        if (!ShootingTarget)
            return;
        GunOrigin.LookAt(ShootingTarget.position, transform.up);
        Fire();
    }

    private void FixedUpdate()
    {
        var nextPosition = agent.nextPosition;
        transform.position = Vector3.Lerp(transform.position, nextPosition, agent.speed * Time.fixedDeltaTime);
        transform.LookAt(new Vector3(nextPosition.x, transform.position.y, nextPosition.z), transform.up);
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
