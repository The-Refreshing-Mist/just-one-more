using UnityEngine;
using UnityEngine.AI;

public class Wandering : MonoBehaviour
{
    public float wanderRadius = 20f;
    public float waitTime = 2f;


    private NavMeshAgent agent;
    private Animator animator;
    private float waitTimer;
    private bool isWalking;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (agent == null)
        {
            Debug.LogError("Missing NavMeshAgent on NPC.");
            return;
        }

        if (animator == null)
        {
            Debug.LogError("Missing Animator on NPC.");
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogError("NPC is not on the NavMesh. Move it onto the blue NavMesh area.");
            return;
        }

        PickNewDestination();
    }

    void Update()
    {
        if (agent == null || animator == null || !agent.isOnNavMesh)
        {
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTime)
            {
                PickNewDestination();
                waitTimer = 0f;
            }
        }
        else
        {
        }
    }

    void PickNewDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

}