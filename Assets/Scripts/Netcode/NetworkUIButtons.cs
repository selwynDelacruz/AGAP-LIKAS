using UnityEngine;
using Unity.Netcode;
using TMPro;

/// <summary>
/// UI Buttons for network operations - updated to work with NetworkLobbyManager
/// </summary>
public class NetworkUIButtons : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField lobbyCodeInput;
    [SerializeField] private TMP_Text statusText;

    /// <summary>
    /// Start as Instructor/Host - generates a lobby code and starts hosting
    /// </summary>
    public void StartAsInstructorHost()
    {
        if (NetworkLobbyManager.Instance != null)
        {
            // Generate lobby code first
            string lobbyCode = NetworkLobbyManager.Instance.GenerateLobbyCode();
            
            if (!string.IsNullOrEmpty(lobbyCode))
            {
                // Start hosting with the generated code
                bool success = NetworkLobbyManager.Instance.StartHostWithLobbyCode();
                
                if (success)
                {
                    UpdateStatus($"Hosting with code: {lobbyCode}");
                }
                else
                {
                    UpdateStatus("Failed to start hosting");
                }
            }
            else
            {
                UpdateStatus("Failed to generate lobby code");
            }
        }
        else
        {
            // Fallback to direct NetworkManager if NetworkLobbyManager is not available
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.StartHost();
                UpdateStatus("Started as Host (no lobby code)");
            }
        }
    }

    /// <summary>
    /// Start as Trainee/Client - joins using the lobby code
    /// </summary>
    public void StartAsTraineeClient()
    {
        if (NetworkLobbyManager.Instance != null && lobbyCodeInput != null)
        {
            string code = lobbyCodeInput.text.Trim();
            
            if (!string.IsNullOrEmpty(code))
            {
                NetworkLobbyManager.Instance.JoinLobbyWithCode(code);
            }
            else
            {
                UpdateStatus("Please enter a lobby code");
            }
        }
        else if (NetworkManager.Singleton != null)
        {
            // Fallback to direct NetworkManager if NetworkLobbyManager is not available
            NetworkManager.Singleton.StartClient();
            UpdateStatus("Started as Client (no lobby code)");
        }
    }

    /// <summary>
    /// Disconnect from the current session
    /// </summary>
    public void Disconnect()
    {
        if (NetworkLobbyManager.Instance != null)
        {
            NetworkLobbyManager.Instance.LeaveLobby();
            UpdateStatus("Disconnected");
        }
        else if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            UpdateStatus("Disconnected");
        }
    }

    /// <summary>
    /// Updates the status text if available
    /// </summary>
    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"Network Status: {message}");
    }
}
