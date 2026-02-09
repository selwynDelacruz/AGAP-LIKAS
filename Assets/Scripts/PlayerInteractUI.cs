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
    [Tooltip("Enable debug logging for troubleshooting")]
    [SerializeField] private bool showDebugLogs = true;

    private PlayerInteract playerInteract;
    private PlayerInteract boatInteract;
    private PlayerInteract currentActiveInteract;

    private float nextSearchTime = 0f;
    private const float searchInterval = 1f; // Search every second if not found

    private void Start()
    {
        // Try to find player interact components on start
        FindPlayerInteractComponents();
    }

    private void Update()
    {
        // If components not found yet, try to find them (but not every frame to avoid performance issues)
        if (playerInteract == null && boatInteract == null && Time.time >= nextSearchTime)
        {
            nextSearchTime = Time.time + searchInterval;
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
            if (showDebugLogs)
            {
                Debug.LogWarning("[PlayerInteractUI] Local player not found yet. Will retry...");
            }
            return;
        }

        // Get all PlayerInteract components on the player and its children
        PlayerInteract[] interacts = localPlayer.GetComponentsInChildren<PlayerInteract>(true);

        if (interacts.Length == 0)
        {
            Debug.LogWarning($"[PlayerInteractUI] No PlayerInteract components found on local player: {localPlayer.name}!");
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
                if (showDebugLogs)
                {
                    Debug.Log($"[PlayerInteractUI] Found boat interact: {interact.gameObject.name}");
                }
            }
            else
            {
                playerInteract = interact;
                if (showDebugLogs)
                {
                    Debug.Log($"[PlayerInteractUI] Found player interact: {interact.gameObject.name}");
                }
            }
        }

        // If we only found one, use it as playerInteract
        if (interacts.Length == 1)
        {
            playerInteract = interacts[0];
        }

        if (showDebugLogs)
        {
            Debug.Log($"[PlayerInteractUI] Successfully found {interacts.Length} PlayerInteract component(s) on local player!");
        }
    }

    /// <summary>
    /// Finds the local player GameObject
    /// </summary>
    private GameObject FindLocalPlayer()
    {
        // Wait for NetworkManager to be ready
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            // Network not ready yet or single player mode
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                Debug.Log($"[PlayerInteractUI] Found player (non-networked or network not ready): {player.name}");
                return player;
            }
            return null;
        }

        // Method 1: Find all players and check for local ownership
        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);
        foreach (GameObject p in players)
        {
            NetworkObject networkObject = p.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsOwner)
            {
                Debug.Log($"[PlayerInteractUI] Found local player by tag: {p.name} (ClientId: {NetworkManager.Singleton.LocalClientId})");
                return p;
            }
        }

        // Method 2: Look for any PlayerInteract component owned by local player
        PlayerInteract[] allInteracts = Object.FindObjectsByType<PlayerInteract>(FindObjectsSortMode.None);
        foreach (var interact in allInteracts)
        {
            NetworkObject networkObject = interact.GetComponentInParent<NetworkObject>();
            if (networkObject != null && networkObject.IsOwner)
            {
                Debug.Log($"[PlayerInteractUI] Found local player via PlayerInteract: {interact.gameObject.name}");
                return networkObject.gameObject;
            }
        }

        // Method 3: Search in parent hierarchy
        NetworkObject[] allNetworkObjects = Object.FindObjectsByType<NetworkObject>(FindObjectsSortMode.None);
        foreach (var netObj in allNetworkObjects)
        {
            if (netObj.IsOwner && netObj.CompareTag(playerTag))
            {
                Debug.Log($"[PlayerInteractUI] Found local player via NetworkObject search: {netObj.gameObject.name}");
                return netObj.gameObject;
            }
        }

        Debug.LogWarning($"[PlayerInteractUI] Could not find local player. IsClient: {NetworkManager.Singleton.IsClient}, IsServer: {NetworkManager.Singleton.IsServer}, LocalClientId: {NetworkManager.Singleton.LocalClientId}");
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
