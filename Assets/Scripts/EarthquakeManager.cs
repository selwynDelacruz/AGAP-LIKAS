using UnityEngine;
using Unity.Cinemachine;

public class EarthquakeManager : MonoBehaviour
{
    [Header("Cinemachine Camera")]
    [Tooltip("Reference to the Cinemachine Virtual Camera with BasicMultiChannelPerlin")]
    public CinemachineCamera virtualCamera;

    [Header("Noise Profiles")]
    [Tooltip("Earthquake noise profile (6D Shake)")]
    public NoiseSettings earthquakeProfile;

    [Tooltip("Default noise profile (Handheld_normal_mild)")]
    public NoiseSettings defaultProfile;

    [Header("Earthquake Settings")]
    [Tooltip("Time interval between earthquakes (in seconds)")]
    public float earthquakeInterval = 20f;

    [Tooltip("Duration of the earthquake shake (in seconds)")]
    public float earthquakeDuration = 5f;

    private CinemachineBasicMultiChannelPerlin noiseComponent;
    private float timer = 0f;
    private bool isShaking = false;
    private float shakeDurationTimer = 0f;

    void Start()
    {
        // Get the BasicMultiChannelPerlin component from the virtual camera
        if (virtualCamera != null)
        {
            noiseComponent = virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
            
            if (noiseComponent == null)
            {
                Debug.LogError("CinemachineBasicMultiChannelPerlin component not found on the virtual camera!");
            }
            else
            {
                // Set default settings on start
                SetDefaultSettings();
            }
        }
        else
        {
            Debug.LogError("Virtual Camera reference is not assigned in EarthquakeManager!");
        }
    }

    void Update()
    {
        if (noiseComponent == null) return;

        // If currently shaking, track shake duration
        if (isShaking)
        {
            shakeDurationTimer += Time.deltaTime;
            
            if (shakeDurationTimer >= earthquakeDuration)
            {
                StopEarthquake();
            }
        }
        else
        {
            // Count up to the next earthquake
            timer += Time.deltaTime;

            if (timer >= earthquakeInterval)
            {
                StartEarthquake();
                timer = 0f; // Reset timer
            }
        }
    }

    /// <summary>
    /// Manually trigger an earthquake shake (can be called from other scripts)
    /// </summary>
    public void TriggerEarthquake()
    {
        StartEarthquake();
    }

    /// <summary>
    /// Trigger earthquake with custom duration
    /// </summary>
    public void TriggerEarthquake(float customDuration)
    {
        earthquakeDuration = customDuration;
        StartEarthquake();
    }

    void StartEarthquake()
    {
        if (noiseComponent != null && earthquakeProfile != null)
        {
            noiseComponent.NoiseProfile = earthquakeProfile;
            noiseComponent.AmplitudeGain = 1f;
            noiseComponent.FrequencyGain = 0.07f;
            
            isShaking = true;
            shakeDurationTimer = 0f;
            
            Debug.Log("Earthquake started! (6D Shake - Amplitude: 1, Frequency: 0.07)");
        }
        else if (earthquakeProfile == null)
        {
            Debug.LogError("Earthquake noise profile is not assigned!");
        }
    }

    void StopEarthquake()
    {
        if (noiseComponent != null)
        {
            SetDefaultSettings();
            isShaking = false;
            
            Debug.Log("Earthquake stopped! (Handheld_normal_mild - Amplitude: 0.5, Frequency: 0.3)");
        }
    }

    void SetDefaultSettings()
    {
        if (noiseComponent != null && defaultProfile != null)
        {
            noiseComponent.NoiseProfile = defaultProfile;
            noiseComponent.AmplitudeGain = 0.5f;
            noiseComponent.FrequencyGain = 0.3f;
        }
        else if (defaultProfile == null)
        {
            Debug.LogError("Default noise profile is not assigned!");
        }
    }
}
