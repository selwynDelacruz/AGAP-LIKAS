using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        // Find by tag
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        
        if (player != null)
        {
            return player;
        }

        // Fallback: Look for any PlayerInteract component
        PlayerInteract[] allInteracts = Object.FindObjectsByType<PlayerInteract>(FindObjectsSortMode.None);
        if (allInteracts.Length > 0)
        {
            return allInteracts[0].gameObject;
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
