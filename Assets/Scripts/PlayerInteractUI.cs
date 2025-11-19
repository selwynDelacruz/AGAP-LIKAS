using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class PlayerInteractUI : MonoBehaviour
{
    [SerializeField] private GameObject containerGO;
    [SerializeField] private TextMeshProUGUI interactTextMeshProUGUI;

    [Header("Hold Progress Bar")]
    [SerializeField] private GameObject progressBarContainer;
    [SerializeField] private Image progressBarFill;

    [Header("Auto-Find Settings")]
    [Tooltip("Tag used to identify the local player")]
    [SerializeField] private string playerTag = "Player";

    private PlayerInteract playerInteract;
    private PlayerInteract boatInteract;
    private PlayerInteract currentActiveInteract;

    private void Start()
    {
        // Try to find player interact components on start
        FindPlayerInteractComponents();
    }

    private void Update()
    {
        // If components not found yet, try to find them
        if (playerInteract == null && boatInteract == null)
        {
            FindPlayerInteractComponents();
        }

        // Determine which interact mode is active
        currentActiveInteract = GetActivePlayerInteract();

        if (currentActiveInteract != null && currentActiveInteract.GetInteractableObject() != null)
        {
            Show(currentActiveInteract.GetInteractableObject());

            // Show progress bar when holding
            if (currentActiveInteract.IsHolding())
            {
                ShowProgressBar();
                UpdateProgressBar(currentActiveInteract.GetHoldProgress());
            }
            else
            {
                HideProgressBar();
            }
        }
        else
        {
            Hide();
            HideProgressBar();
        }
    }

    /// <summary>
    /// Finds PlayerInteract components from the local player
    /// </summary>
    private void FindPlayerInteractComponents()
    {
        // Find the local player
        GameObject localPlayer = FindLocalPlayer();

        if (localPlayer == null)
        {
            // Player not spawned yet
            return;
        }

        // Get all PlayerInteract components on the player and its children
        PlayerInteract[] interacts = localPlayer.GetComponentsInChildren<PlayerInteract>(true);

        if (interacts.Length == 0)
        {
            Debug.LogWarning("[PlayerInteractUI] No PlayerInteract components found on local player!");
            return;
        }

        // Assign the found components
        // Assuming the first one is player/swim mode and second is boat mode
        // You can adjust this logic based on your naming convention
        foreach (var interact in interacts)
        {
            if (interact.gameObject.name.Contains("Boat") || interact.gameObject.name.Contains("boat"))
            {
                boatInteract = interact;
            }
            else
            {
                playerInteract = interact;
            }
        }

        // If we only found one, use it as playerInteract
        if (interacts.Length == 1)
        {
            playerInteract = interacts[0];
        }

        Debug.Log($"[PlayerInteractUI] Found {interacts.Length} PlayerInteract component(s)");
    }

    /// <summary>
    /// Finds the local player GameObject
    /// </summary>
    private GameObject FindLocalPlayer()
    {
        // Method 1: Try to find by tag
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        
        if (player != null)
        {
            // Check if this is the local player in networked game
            NetworkObject networkObject = player.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                if (networkObject.IsOwner)
                {
                    return player;
                }
                // Not the local player, keep searching
                player = null;
            }
            else
            {
                // No networking, this is the player
                return player;
            }
        }

        // Method 2: Find all players and check for local ownership
        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);
        foreach (GameObject p in players)
        {
            NetworkObject networkObject = p.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsOwner)
            {
                return p;
            }
        }

        // Method 3: Look for any PlayerInteract component owned by local player
        PlayerInteract[] allInteracts = Object.FindObjectsByType<PlayerInteract>(FindObjectsSortMode.None);
        foreach (var interact in allInteracts)
        {
            NetworkObject networkObject = interact.GetComponentInParent<NetworkObject>();
            if (networkObject != null && networkObject.IsOwner)
            {
                return interact.gameObject;
            }
        }

        return null;
    }

    /// <summary>
    /// Determines which PlayerInteract component is currently active based on enabled state
    /// </summary>
    /// <returns>The active PlayerInteract component, or null if none are active</returns>
    private PlayerInteract GetActivePlayerInteract()
    {
        // Check boat mode first
        if (boatInteract != null && boatInteract.enabled && boatInteract.gameObject.activeInHierarchy)
        {
            return boatInteract;
        }

        // Check player/swim mode
        if (playerInteract != null && playerInteract.enabled && playerInteract.gameObject.activeInHierarchy)
        {
            return playerInteract;
        }

        return null;
    }

    private void Show(IInteractable interactable)
    {
        containerGO.SetActive(true);
        interactTextMeshProUGUI.text = interactable.GetInteractText();
    }

    private void Hide()
    {
        containerGO.SetActive(false);
    }

    private void ShowProgressBar()
    {
        if (progressBarContainer != null)
        {
            progressBarContainer.SetActive(true);
        }
    }

    private void HideProgressBar()
    {
        if (progressBarContainer != null)
        {
            progressBarContainer.SetActive(false);
        }
    }

    private void UpdateProgressBar(float progress)
    {
        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = progress;
        }
    }
}
