using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages the player's medkit inventory system
/// </summary>
public class MedkitManager : MonoBehaviour
{
    [Header("Medkit Settings")]
    [SerializeField] private int maxMedkits = 2;
    [SerializeField] private int currentMedkits = 2;

    [Header("Safe Zone")]
    [Tooltip("Safe zone GameObject that replenishes medkits when player enters")]
    [SerializeField] private GameObject safeZone;

    [Header("UI References")]
    [Tooltip("Reference to the TextMeshProUGUI component that displays medkit count")]
    [SerializeField] private TextMeshProUGUI medkitCountText;

    [Header("Blink Settings")]
    [SerializeField] private float blinkDuration = 1f;
    [SerializeField] private float blinkInterval = 0.2f;
    [SerializeField] private Color blinkColor = Color.red;

    [Header("Replenish Effect")]
    [SerializeField] private Color replenishColor = Color.green;
    [SerializeField] private float replenishBlinkDuration = 0.5f;

    // Singleton for easy access
    public static MedkitManager Instance { get; private set; }

    // Public properties
    public int CurrentMedkits => currentMedkits;
    public int MaxMedkits => maxMedkits;

    private Color defaultColor;
    private SafeZoneTrigger safeZoneTrigger;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (medkitCountText != null)
        {
            defaultColor = medkitCountText.color;
        }
        updateMedkit();

        // Setup safe zone trigger
        SetupSafeZone();
    }

    private void SetupSafeZone()
    {
        if (safeZone != null)
        {
            // Check if SafeZoneTrigger component exists, if not add it
            safeZoneTrigger = safeZone.GetComponent<SafeZoneTrigger>();
            if (safeZoneTrigger == null)
            {
                safeZoneTrigger = safeZone.AddComponent<SafeZoneTrigger>();
            }

            // Subscribe to the safe zone entry event
            safeZoneTrigger.OnPlayerEnterSafeZone += ReplenishMedkits;

            Debug.Log("[MedkitManager] Safe zone configured successfully");
        }
        else
        {
            Debug.LogWarning("[MedkitManager] No safe zone GameObject assigned!");
        }
    }

    public bool UseMedkit()
    {
        if (currentMedkits <= 0)
            return false;

        currentMedkits--;
        updateMedkit();
        return true;
    }

    /// <summary>
    /// Replenishes medkits to maximum capacity
    /// </summary>
    public void ReplenishMedkits()
    {
        int medkitsToAdd = maxMedkits - currentMedkits;

        if (medkitsToAdd > 0)
        {
            currentMedkits = maxMedkits;
            updateMedkit();
            TriggerReplenishEffect();
            Debug.Log($"[MedkitManager] Replenished {medkitsToAdd} medkit(s). Current: {currentMedkits}/{maxMedkits}");
        }
        else
        {
            Debug.Log("[MedkitManager] Medkits already at maximum capacity");
        }
    }

    void updateMedkit()
    {
        if (medkitCountText != null)
        {
            medkitCountText.text = "Medkit: " + currentMedkits + "/" + maxMedkits;
        }
    }

    /// <summary>
    /// Triggers a blink effect on the medkit count text
    /// </summary>
    public void TriggerBlinkEffect()
    {
        StartCoroutine(BlinkText());
    }

    /// <summary>
    /// Triggers a green blink effect when medkits are replenished
    /// </summary>
    private void TriggerReplenishEffect()
    {
        StartCoroutine(ReplenishBlinkText());
    }

    private IEnumerator BlinkText()
    {
        float elapsedTime = 0f;

        while (elapsedTime < blinkDuration)
        {
            // Red
            medkitCountText.color = blinkColor;
            yield return new WaitForSeconds(blinkInterval);

            // Default
            medkitCountText.color = defaultColor;
            yield return new WaitForSeconds(blinkInterval);

            elapsedTime += blinkInterval * 2;
        }

        // Ensure it ends on default color
        medkitCountText.color = defaultColor;
    }

    private IEnumerator ReplenishBlinkText()
    {
        float elapsedTime = 0f;

        while (elapsedTime < replenishBlinkDuration)
        {
            // Green
            medkitCountText.color = replenishColor;
            yield return new WaitForSeconds(blinkInterval);

            // Default
            medkitCountText.color = defaultColor;
            yield return new WaitForSeconds(blinkInterval);

            elapsedTime += blinkInterval * 2;
        }

        // Ensure it ends on default color
        medkitCountText.color = defaultColor;
    }

    private void OnDestroy()
    {
        // Unsubscribe from safe zone events to prevent memory leaks
        if (safeZoneTrigger != null)
        {
            safeZoneTrigger.OnPlayerEnterSafeZone -= ReplenishMedkits;
        }
    }
}

/// <summary>
/// Helper component for safe zone trigger detection
/// </summary>
public class SafeZoneTrigger : MonoBehaviour
{
    public event Action OnPlayerEnterSafeZone;

    [Header("Safe Zone Settings")]
    [Tooltip("Tag used to identify the player")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Cooldown time in seconds before player can replenish again")]
    [SerializeField] private float replenishCooldown = 2f;

    private float lastReplenishTime = -999f;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag(playerTag))
        {
            // Check cooldown to prevent spam
            if (Time.time - lastReplenishTime >= replenishCooldown)
            {
                lastReplenishTime = Time.time;
                OnPlayerEnterSafeZone?.Invoke();
                Debug.Log("[SafeZoneTrigger] Player entered safe zone");
            }
        }
    }

    private void OnValidate()
    {
        // Ensure this GameObject has a trigger collider
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning("[SafeZoneTrigger] Collider on safe zone should be set as Trigger!");
        }
    }
}