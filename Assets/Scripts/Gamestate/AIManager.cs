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
    private Rigidbody body;
    private float maxVelocityBeforeExtraDamping = 1f;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        body = GetComponent<Rigidbody>();
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
