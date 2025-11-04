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

    [Header("Interaction Settings")]
    [SerializeField] private string interactText = "Press E to pick up Medkit";
    [SerializeField] private string inventoryFullText = "Medkit inventory is full!";

    // Singleton for easy access
    public static MedkitManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        medkitCountText.text = "Medkit: " + currentMedkits + "/" + maxMedkits;
    }

    public void Interact(Transform interactorTransform)
    {
        bool success = AddMedkit(2);
        
        if (success)
        {
            Debug.Log("Picked up 2 medkit!");
            gameObject.SetActive(false); // Disable the medkit pickup object
        }
        else
        {
            Debug.Log("Cannot pick up medkit - inventory full!");
        }
    }

    public string GetInteractText()
    {
        if (currentMedkits >= maxMedkits)
        {
            return inventoryFullText;
        }
        return interactText;
    }

    public bool AddMedkit(int amount = 2)
    {
        if (currentMedkits >= maxMedkits)
            return false;

        currentMedkits += amount;
        medkitCountText.text = "Medkit: " + currentMedkits + "/" + maxMedkits;
        return true;
    }

    public bool UseMedkit()
    {
        if (currentMedkits <= 0)
            return false;

        currentMedkits--;
        medkitCountText.text = "Medkit: " + currentMedkits + "/" + maxMedkits;
        return true;
    }
}