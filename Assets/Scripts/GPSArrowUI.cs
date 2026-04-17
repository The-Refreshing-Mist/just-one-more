using UnityEngine;
using TMPro;

public class GPSArrowUI : MonoBehaviour
{
    [Header("References")]
    public Transform playerCar;
    public Transform destination;
    public Collider destinationCollider;   // drag endpoint collider here
    public Camera playerCamera;
    public RectTransform arrowUI;
    public TMP_Text distanceText;

    [Header("Settings")]
    public float smoothRotateSpeed = 5f;
    public bool hideWhenReached = true;
    public float reachedDistance = 2f;

    private bool destinationReached = false;

    void Update()
    {
        if (playerCar == null || destination == null || playerCamera == null || arrowUI == null)
            return;

        if (destinationReached)
        {
            if (hideWhenReached)
            {
                arrowUI.gameObject.SetActive(false);

                if (distanceText != null)
                    distanceText.gameObject.SetActive(false);
            }
            return;
        }

        Vector3 closestPoint;

        if (destinationCollider != null)
        {
            // closest point on the actual collider surface
            closestPoint = destinationCollider.ClosestPoint(playerCar.position);
        }
        else
        {
            // fallback if no collider is assigned
            closestPoint = destination.position;
        }

        Vector3 toTarget = closestPoint - playerCar.position;
        float distance = toTarget.magnitude;

        Vector3 flatToTarget = new Vector3(toTarget.x, 0f, toTarget.z).normalized;
        Vector3 flatForward = new Vector3(playerCamera.transform.forward.x, 0f, playerCamera.transform.forward.z).normalized;

        if (flatToTarget.sqrMagnitude > 0.001f && flatForward.sqrMagnitude > 0.001f)
        {
            float angle = Vector3.SignedAngle(flatForward, flatToTarget, Vector3.up);

            Quaternion targetRotation = Quaternion.Euler(0f, 0f, -angle);
            arrowUI.localRotation = Quaternion.Lerp(
                arrowUI.localRotation,
                targetRotation,
                Time.deltaTime * smoothRotateSpeed
            );
        }

        if (distanceText != null)
        {
            distanceText.text = Mathf.CeilToInt(distance) + " m";
        }

        if (distance <= reachedDistance)
        {
            destinationReached = true;
        }
    }

    public void SetDestinationReached()
    {
        destinationReached = true;
    }
}