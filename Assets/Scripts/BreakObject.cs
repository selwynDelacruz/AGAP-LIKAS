using UnityEngine;

public class BreakObject : MonoBehaviour
{
    [Header("Prefab Settings")]
    [Tooltip("The broken/destroyed version of this object")]
    public GameObject breakedObjectPrefab;

    [Header("Collapse Settings")]
    [Tooltip("Downward force applied to simulate gravity-driven collapse")]
    public float downwardForce = 50f;
    
    [Tooltip("Random horizontal spread (lower = more vertical collapse)")]
    public float horizontalSpread = 2f;
    
    [Tooltip("Slight rotation force for realistic tumbling")]
    public float torqueAmount = 10f;

    [Header("Optional Effects")]
    [Tooltip("Delay before pieces start falling (seconds)")]
    public float collapseDelay = 0.1f;

    [Header("Camera Shake")]
    [Tooltip("Reference to the EarthquakeManager for camera shake")]
    public EarthquakeManager earthquakeManager;

    [Tooltip("Trigger camera shake when object breaks")]
    public bool triggerCameraShake = true;

    [Tooltip("Custom shake duration (overrides EarthquakeManager default if > 0)")]
    public float customShakeDuration = 0f;

    private BoxCollider triggerCollider;
    private bool hasTriggered = false;

    void Start()
    {
        // Get the trigger collider on this parent object
        triggerCollider = GetComponent<BoxCollider>();
        
        if (triggerCollider == null)
        {
            Debug.LogError($"BreakObject on {gameObject.name}: No BoxCollider found!", this);
        }
        else
        {
            triggerCollider.isTrigger = true;
        }

        if (breakedObjectPrefab == null)
        {
            Debug.LogError($"BreakObject on {gameObject.name}: No breakedObjectPrefab assigned!", this);
        }

        // Try to find EarthquakeManager if not assigned
        if (earthquakeManager == null && triggerCameraShake)
        {
            earthquakeManager = FindFirstObjectByType<EarthquakeManager>();
            
            if (earthquakeManager == null)
            {
                Debug.LogWarning($"BreakObject on {gameObject.name}: No EarthquakeManager found in scene. Camera shake will be disabled.");
                triggerCameraShake = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            
            // Trigger camera shake immediately when player enters
            if (triggerCameraShake && earthquakeManager != null)
            {
                if (customShakeDuration > 0)
                {
                    earthquakeManager.TriggerEarthquake(customShakeDuration);
                }
                else
                {
                    earthquakeManager.TriggerEarthquake();
                }
            }
            
            if (collapseDelay > 0)
            {
                Invoke(nameof(BreakAndCollapse), collapseDelay);
            }
            else
            {
                BreakAndCollapse();
            }
        }
    }

    /// <summary>
    /// Instantiates the broken prefab and simulates earthquake collapse
    /// </summary>
    private void BreakAndCollapse()
    {
        if (breakedObjectPrefab == null)
        {
            Debug.LogError($"Cannot break {gameObject.name}: breakedObjectPrefab is not assigned!");
            return;
        }

        // Store position and rotation before destroying
        Vector3 spawnPosition = transform.position;
        Quaternion spawnRotation = transform.rotation;

        // Instantiate the broken version at the same location
        GameObject brokenObject = Instantiate(breakedObjectPrefab, spawnPosition, spawnRotation);

        // Apply collapse forces to all child Rigidbodies
        Rigidbody[] rubblePieces = brokenObject.GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in rubblePieces)
        {
            if (rb != null)
            {
                // Make sure rigidbody is not kinematic
                rb.isKinematic = false;

                // Add primarily downward force with slight random horizontal spread
                Vector3 collapseForce = new Vector3(
                    Random.Range(-horizontalSpread, horizontalSpread),  // Small random X
                    -downwardForce,                                      // Strong downward force
                    Random.Range(-horizontalSpread, horizontalSpread)   // Small random Z
                );

                rb.AddForce(collapseForce, ForceMode.Impulse);

                // Add slight random torque for realistic tumbling
                Vector3 randomTorque = new Vector3(
                    Random.Range(-torqueAmount, torqueAmount),
                    Random.Range(-torqueAmount, torqueAmount),
                    Random.Range(-torqueAmount, torqueAmount)
                );

                rb.AddTorque(randomTorque, ForceMode.Impulse);
            }
        }

        Debug.Log($"{gameObject.name} collapsed! {rubblePieces.Length} pieces falling.");

        // Destroy the original intact object
        Destroy(gameObject);
    }

    /// <summary>
    /// Editor helper: Visualize the collapse area in the scene view
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // Orange transparent
        
        // Draw box showing horizontal spread area
        Gizmos.DrawCube(transform.position, new Vector3(horizontalSpread * 2, 0.5f, horizontalSpread * 2));
        
        // Draw downward arrow
        Gizmos.color = Color.red;
        Vector3 arrowStart = transform.position;
        Vector3 arrowEnd = transform.position + Vector3.down * 2f;
        Gizmos.DrawLine(arrowStart, arrowEnd);
    }
}
