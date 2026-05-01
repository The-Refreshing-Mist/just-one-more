using UnityEngine;
using UnityEngine.AI;

public class Wandering : MonoBehaviour
{
    public float wanderRadius = 20f;
    public float waitTime = 2f;

    public string walkAnimationName = "locom_m_slowWalk_40f";

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

        PlayWalkAnimation();
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
                PlayWalkAnimation();
            }
        }
        else
        {
            PlayWalkAnimation();
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

    void PlayWalkAnimation()
    {
        if (isWalking)
        {
            return;
        }

        int stateHash = Animator.StringToHash(walkAnimationName);

        if (animator.HasState(0, stateHash))
        {
            animator.Play(stateHash, 0, 0f);
            isWalking = true;
        }
        else
        {
            Debug.LogError("Animator does not have a state named: " + walkAnimationName);
        }
    }
}