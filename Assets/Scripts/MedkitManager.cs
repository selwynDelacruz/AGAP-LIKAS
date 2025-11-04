using System;
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

    // Singleton for easy access
    public static MedkitManager Instance { get; private set; }

    // Public properties
    public int CurrentMedkits => currentMedkits;
    public int MaxMedkits => maxMedkits;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
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
}