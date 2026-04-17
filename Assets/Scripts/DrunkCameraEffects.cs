using UnityEngine;

public class DrunkCameraEffect : MonoBehaviour
{
    [Header("Toggle")]
    public bool drunkEnabled = false;

    [Range(0f, 1f)]
    public float currentIntensity = 0f; // shows current drunkenness in Inspector

    [Header("Build Up / Recovery")]
    public float rampUpTime = 6f;      // seconds to reach full effect
    public float recoveryTime = 3f;    // seconds to recover back to normal

    [Header("Maximum Position Sway")]
    public float maxSwayAmount = 0.12f;
    public float maxSwaySpeed = 2.5f;

    [Header("Maximum Random Jitter")]
    public float maxJitterAmount = 0.03f;
    public float maxJitterSpeed = 15f;

    [Header("Maximum Tilt")]
    public float maxRollAmount = 10f;

    [Header("Smoothing")]
    public float smoothSpeed = 6f;

    private Vector3 startLocalPos;
    private Quaternion startLocalRot;

    void Start()
    {
        startLocalPos = transform.localPosition;
        startLocalRot = transform.localRotation;
    }

    void LateUpdate()
    {
        // Build up or recover over time
        float targetIntensity = drunkEnabled ? 1f : 0f;
        float changeSpeed = drunkEnabled
            ? (1f / Mathf.Max(rampUpTime, 0.01f))
            : (1f / Mathf.Max(recoveryTime, 0.01f));

        currentIntensity = Mathf.MoveTowards(
            currentIntensity,
            targetIntensity,
            changeSpeed * Time.deltaTime
        );

        // Scale all effects by current intensity
        float swayAmount = maxSwayAmount * currentIntensity;
        float swaySpeed = Mathf.Lerp(0f, maxSwaySpeed, currentIntensity);

        float jitterAmount = maxJitterAmount * currentIntensity;
        float jitterSpeed = Mathf.Lerp(0f, maxJitterSpeed, currentIntensity);

        float rollAmount = maxRollAmount * currentIntensity;

        // If almost sober, return to normal
        if (currentIntensity <= 0.001f)
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                startLocalPos,
                Time.deltaTime * smoothSpeed
            );

            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                startLocalRot,
                Time.deltaTime * smoothSpeed
            );

            return;
        }

        float t = Time.time;

        Vector3 sway = new Vector3(
            Mathf.Sin(t * swaySpeed) * swayAmount,
            Mathf.Cos(t * swaySpeed * 0.7f) * swayAmount * 0.5f,
            0f
        );

        Vector3 jitter = new Vector3(
            (Mathf.PerlinNoise(t * jitterSpeed, 0f) - 0.5f) * jitterAmount,
            (Mathf.PerlinNoise(0f, t * jitterSpeed) - 0.5f) * jitterAmount,
            0f
        );

        Vector3 targetPos = startLocalPos + sway + jitter;
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPos,
            Time.deltaTime * smoothSpeed
        );

        float roll = Mathf.Sin(t * swaySpeed * 0.8f) * rollAmount;
        Quaternion targetRot = startLocalRot * Quaternion.Euler(0f, 0f, roll);

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRot,
            Time.deltaTime * smoothSpeed
        );
    }

    public void SetDrunk(bool enabled)
    {
        drunkEnabled = enabled;
    }

    public void SetIntensity(float value)
    {
        currentIntensity = Mathf.Clamp01(value);
    }
}