using UnityEngine;

public class movement : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 1.5f;

    private Transform target;

    void Start()
    {
        target = pointB;
    }

    void Update()
    {
        if (pointA == null || pointB == null)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        Vector3 direction = target.position - transform.position;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            if (target == pointA)
            {
                target = pointB;
            }
            else
            {
                target = pointA;
            }
        }
    }
}