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

    [Header("UI References")]
    [Tooltip("Reference to the TextMeshProUGUI component that displays medkit count")]
    [SerializeField] private TextMeshProUGUI medkitCountText;

    [Header("Blink Settings")]
    [SerializeField] private float blinkDuration = 1f;
    [SerializeField] private float blinkInterval = 0.2f;
    [SerializeField] private Color blinkColor = Color.red;

    // Singleton for easy access
    public static MedkitManager Instance { get; private set; }

    // Public properties
    public int CurrentMedkits => currentMedkits;
    public int MaxMedkits => maxMedkits;

    private Color defaultColor;

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
    }

    public bool UseMedkit()
    {
        if (currentMedkits <= 0)
            return false;

        currentMedkits--;
        updateMedkit();
        return true;
    }

    void updateMedkit()
    {
        medkitCountText.text = "Medkit: " + currentMedkits + "/" + maxMedkits;
    }

    /// <summary>
    /// Triggers a blink effect on the medkit count text
    /// </summary>
    public void TriggerBlinkEffect()
    {
        StartCoroutine(BlinkText());
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
}