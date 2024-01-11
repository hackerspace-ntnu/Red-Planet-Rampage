using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIManager : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform mainPlayer;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private GunFactory gunFactory;
    [SerializeField]
    private GunController gun;
    public PlayerIdentity Identity;
    private Rigidbody body;
    [SerializeField]
    private GameObject meshBase;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        body = GetComponent<Rigidbody>();
        Identity = GetComponent<PlayerIdentity>();
        gunFactory.Body = StaticInfo.Singleton.StartingBody;
        gunFactory.Barrel = StaticInfo.Singleton.StartingBarrel;
        gun.triggerPressed = true;
        gun.triggerHeld = true;
        gunFactory.InitializeGun();
        meshBase.GetComponentInChildren<SkinnedMeshRenderer>().material.color = Identity.color;
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
