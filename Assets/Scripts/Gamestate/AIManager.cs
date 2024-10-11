using CollectionExtensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using VectorExtensions;

public class AIManager : PlayerManager
{
    private NavMeshAgent agent;
    public Transform DestinationTarget;
    public Transform ShootingTarget;

    private List<PlayerManager> trackedPlayers = new();
    public List<PlayerManager> TrackedPlayers
    {
        get => trackedPlayers;
        set
        {
            trackedPlayers = value;
            TrackedPlayers.ForEach(player => player.onDeath += RemovePlayer);
        }
    }
    private const float autoAwareRadius = 25f;
    private const float ignoreAwareRadius = 1000f;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private LayerMask ignoreMask;

    public BiddingAI biddingAI;
    private Rigidbody body;
    private Collider colliderBox;
    private AIMovement aiMovement;
    [SerializeField]
    private AnimationCurve jumpYoffset;
    [SerializeField]
    private float itemStoppingDistance = 0.5f;
    [SerializeField]
    private float shootingStoppingDistance = 5f;
    [SerializeField]
    private bool updateRotation = true;
    [SerializeField]
    private Item[] disabledItems;
    private delegate void NavMeshEvent();
    private NavMeshEvent onLinkStart;
    private NavMeshEvent onLinkEnd;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        body = GetComponent<Rigidbody>();
        colliderBox = GetComponent<Collider>();
        colliderBox.isTrigger = true;
        aiMovement = GetComponent<AIMovement>();
        healthController = GetComponent<HealthController>();
        agent.autoTraverseOffMeshLink = false;
        onLinkStart += AnimateJump;
        onLinkEnd += AnimateStopCrouch;
        healthController.onDamageTaken += OnDamageTaken;
        healthController.onDeath += OnDeath;
        aiTargetCollider = Instantiate(aiTarget).GetComponent<AITarget>();
        aiTargetCollider.Owner = this;
        aiTargetCollider.transform.position = transform.position;
        StartCoroutine(LookForTargets());
    }

    private void OnDestroy()
    {
        healthController.onDamageTaken -= OnDamageTaken;
        healthController.onDeath -= OnDeath;
        TrackedPlayers.ForEach(player => player.onDeath -= RemovePlayer);

        if (!gunController)
            return;

        gunController.onFireStart -= UpdateAimTarget;
        gunController.onFire -= UpdateAimTarget;
    }

    public void SetIdentity(PlayerIdentity identity)
    {
        this.identity = identity;
        if (!biddingAI)
            SetGun(GunHolder);
        meshBase.ToList().ForEach(mesh => mesh.GetComponentInChildren<SkinnedMeshRenderer>().material.color = identity.color);
        if (biddingAI)
            biddingAI.SetIdentity(identity);
    }

    private void RemovePlayer(PlayerManager killer, PlayerManager victim, DamageInfo info)
    {
        TrackedPlayers.Remove(victim);
        victim.onDeath -= RemovePlayer;
        if (ShootingTarget == victim.AiAimSpot)
            ShootingTarget = null;
    }

    public override void SetLayer(int playerIndex)
    {
        int playerLayer = LayerMask.NameToLayer("Player " + playerIndex);
        gameObject.layer = playerLayer;
    }

    private Item ChoosePart(Item current, IEnumerable<Item> available, Item fallback)
    {
        bool hasDisabledPart = IsDisabledItem(current);
        var safeParts = available.Where(item => !IsDisabledItem(item)).ToList();
        var part = fallback;
        if (!hasDisabledPart)
            part = current;
        else if (safeParts.Count > 0)
            part = safeParts.RandomElement();
        return part;
    }

    public override void SetGun(Transform offset)
    {
        overrideAimTarget = false;

        var body = ChoosePart(identity.Body, identity.Bodies, StaticInfo.Singleton.StartingBody);
        var barrel = ChoosePart(identity.Barrel, identity.Barrels, StaticInfo.Singleton.StartingBarrel);
        var extension = ChoosePart(identity.Extension, identity.Extensions, StaticInfo.Singleton.StartingExtension);

        var gun = GunFactory.InstantiateGunAI(body, barrel, extension, this, offset);
        gunController = gun.GetComponent<GunController>();
        gunController.Initialize();
        gunController.onFireStart += UpdateAimTarget;
        gunController.onFire += UpdateAimTarget;
        playerIK.LeftHandIKTarget = gunController.LeftHandTarget;
        if (gunController.RightHandTarget)
            playerIK.RightHandIKTarget = gunController.RightHandTarget;
        GetComponent<AmmoBoxCollector>().CheckForAmmoBoxBodyAgain();
    }

    private bool IsDisabledItem(Item item)
    {
        return disabledItems.Any(disabledItem => disabledItem == item);
    }

    private void OnDamageTaken(HealthController healthController, float damage, DamageInfo info)
    {
        if (info.sourcePlayer != this)
        {
            lastPlayerThatHitMe = info.sourcePlayer;
            // Immediately target this player
            // TODO gate this between difficulty
            ShootingTarget = info.sourcePlayer.AiAimSpot;
        }
        if (info.damageType != DamageType.Explosion)
            return;
        StartCoroutine(WaitAndToggleAgent());
    }

    public IEnumerator WaitAndToggleAgent()
    {
        agent.enabled = false;
        body.isKinematic = false;
        colliderBox.isTrigger = false;

        yield return new WaitForSeconds(0.25f);
        while (aiMovement.StateIsAir)
            yield return null;

        body.isKinematic = true;
        agent.enabled = true;
        colliderBox.isTrigger = true;
    }

    protected override void OnDeath(HealthController healthController, float damage, DamageInfo info)
    {
        base.OnDeath(healthController, damage, info);
        aiMovement.enabled = false;
        agent.enabled = false;
        body.isKinematic = false;
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
        if (!IsAlive)
            yield break;
        if (!aiMovement || !aiMovement.enabled)
            FindPlayers();
        yield return new WaitForSeconds(3f);
        StartCoroutine(LookForTargets());
    }

    private void FindPlayers()
    {
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
                DestinationTarget = target;
            }
        }
        else
        {
            aiMovement.Target = ShootingTarget;
            var isStrafeDistance = closestDistance < 10f;
            aiMovement.enabled = isStrafeDistance;
        }

        if (!DestinationTarget)
        {
            // TODO handle having no target better?
            if (trackedPlayers.Count == 0)
                return;
            var player = TrackedPlayers.RandomElement();
            DestinationTarget = player.AiTarget;
            ShootingTarget = player.AiAimSpot;
        }
        agent.stoppingDistance = ShootingTarget ? shootingStoppingDistance : itemStoppingDistance;
        try
        {
            if (agent.enabled)
                agent.SetDestination(DestinationTarget.position);
            else
                aiMovement.enabled = true;
        }
        catch
        {
            aiMovement.enabled = true;
        }

    }

    private void AnimateJump()
    {
        animator.SetBool("Crouching", true);
        animator.SetTrigger("Leap");
        StartCoroutine(AnimateJumpCurve(0.6f));
    }

    private void AnimateStopCrouch()
    {
        animator.SetBool("Crouching", false);
    }

    IEnumerator AnimateJumpCurve(float duration)
    {
        if (!agent.enabled)
            yield return null;
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
        // TODO why is the agent sometimes not enabled here?
        if (agent.enabled)
            agent.CompleteOffMeshLink();
        onLinkEnd?.Invoke();
    }

    private void Update()
    {
        if (!IsAlive)
            return;

#if UNITY_EDITOR
        foreach (var player in TrackedPlayers)
        {
            Debug.DrawLine(transform.position, player.transform.position, Color.blue);
        }
#endif
        if (ShootingTarget)
        {
            // Face in target direction
            var horizontalDirection = (ShootingTarget.position.xz() - transform.position.xz()).normalized;
            transform.forward = new Vector3(horizontalDirection.x, 0, horizontalDirection.y);
            // Point gun
            GunOrigin.LookAt(ShootingTarget.position, transform.up);
            Fire();
        }
        if (!DestinationTarget || !agent.enabled)
            return;
#if UNITY_EDITOR
        Debug.DrawLine(transform.position, DestinationTarget.position, Color.green);
#endif
        animator.SetFloat("Forward", Vector3.Dot(agent.velocity, transform.forward) / agent.speed);
        animator.SetFloat("Right", Vector3.Dot(agent.velocity, transform.right) / agent.speed);
        if (agent.isOnOffMeshLink)
            onLinkStart?.Invoke();
    }

    private void FixedUpdate()
    {
        if (!IsAlive || !agent.enabled)
            return;
        var nextPosition = agent.nextPosition;
        transform.position = Vector3.Lerp(transform.position, nextPosition, agent.speed * Time.fixedDeltaTime);

        if (!updateRotation)
            return;
        var targetPosition = ShootingTarget ? ShootingTarget.position : nextPosition;
        transform.LookAt(new Vector3(targetPosition.x, transform.position.y, targetPosition.z), transform.up);
    }

    private void Fire()
    {
        if (!gunController)
            return;
        gunController.triggerHeld = true;
        gunController.triggerPressed = true;
        StartCoroutine(UnpressTrigger());
    }

    private IEnumerator UnpressTrigger()
    {
        yield return new WaitForFixedUpdate();
        gunController.triggerHeld = false;
        gunController.triggerPressed = false;
    }
}
