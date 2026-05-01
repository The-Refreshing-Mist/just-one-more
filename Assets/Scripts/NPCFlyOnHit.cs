using UnityEngine;
using UnityEngine.AI;

public class NPCFlyOnHit : MonoBehaviour
{
    public string carTag = "Player";
    public float hitForce = 20f;
    public float upwardForce = 8f;
    public float spinForce = 10f;

    private Rigidbody rb;
    private Animator animator;
    private NavMeshAgent agent;
    private bool hasBeenHit = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Keep NPC controlled by NavMesh before being hit
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasBeenHit)
        {
            return;
        }

        if (!collision.gameObject.CompareTag(carTag) && !collision.transform.root.CompareTag(carTag))
        {
            return;
        }

        hasBeenHit = true;

        // Stop walking/navigation
        if (agent != null)
        {
            agent.enabled = false;
        }

        // Stop animation
        if (animator != null)
        {
            animator.enabled = false;
        }

        // Turn physics on
        rb.isKinematic = false;
        rb.useGravity = true;

        // Direction away from the car
        Vector3 hitDirection = (transform.position - collision.transform.position).normalized;
        hitDirection.y = 0f;

        Vector3 finalForce = hitDirection * hitForce + Vector3.up * upwardForce;

        rb.AddForce(finalForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * spinForce, ForceMode.Impulse);
    }
}