using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class DrunkDrivingEffects : MonoBehaviour
{
    public PostProcessVolume postProcessVolume;  // Reference to the Post-Processing volume
    public float maxBlur = 5f;                  // Maximum blur strength
    public float maxDistortion = 0.1f;          // Maximum distortion for blurry vision
    public float maxMotionBlur = 1f;            // Maximum motion blur intensity

    private Camera carCamera;
    private float currentSpeed;
    private MotionBlur motionBlur;
    private DepthOfField depthOfField;

    private void Start()
    {
        // Get the car camera
        carCamera = GetComponent<Camera>();

        // Ensure Post-Processing effects are correctly referenced
        if (postProcessVolume != null)
        {
            // Try to get MotionBlur and DepthOfField settings
            if (!postProcessVolume.profile.TryGetSettings(out motionBlur) || !postProcessVolume.profile.TryGetSettings(out depthOfField))
            {
                Debug.LogError("Post-processing effects are not found in the PostProcessVolume profile!");
            }
        }
        else
        {
            Debug.LogError("PostProcessVolume is not assigned!");
        }
    }

    private void Update()
    {
        // Get the car's current speed (replace this with actual speed calculation)
        currentSpeed = GetComponent<Rigidbody>().linearVelocity.magnitude;

        // Apply motion blur effect based on speed
        ApplyMotionBlur(currentSpeed);

        // Apply blurry vision (depth of field) effect based on speed
        ApplyBlurryVision(currentSpeed);

        // Apply pixel distortion effect (random jitter)
        ApplyPixelDistortion();
    }

    // Adjust the motion blur intensity based on car speed
    private void ApplyMotionBlur(float speed)
    {
        if (motionBlur != null)
        {
            // Adjust motion blur based on speed, clamp to maxMotionBlur
            motionBlur.shutterAngle.value = Mathf.Clamp(speed / 20f, 0f, maxMotionBlur);
        }
    }

    // Apply blurry vision effect (depth of field) based on speed
    private void ApplyBlurryVision(float speed)
    {
        if (depthOfField != null)
        {
            // Increase the blur strength based on speed, capped at maxDistortion
            depthOfField.focusDistance.value = Mathf.Lerp(10f, 0f, Mathf.Clamp(speed / 20f, 0f, 1f));
            depthOfField.aperture.value = Mathf.Lerp(10f, 50f, Mathf.Clamp(speed / 20f, 0f, 1f));
        }
    }

    // Apply random pixel distortion for the drunk effect
    private void ApplyPixelDistortion()
    {
        // Randomly jitter the camera position to simulate drunken movement
        float jitterX = Random.Range(-0.1f, 0.1f);
        float jitterY = Random.Range(-0.1f, 0.1f);

        // Apply the jitter to the camera's position
        carCamera.transform.position += new Vector3(jitterX, jitterY, 0f);
    }
}