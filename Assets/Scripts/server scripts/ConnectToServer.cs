using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    // This script is responsible for connecting to the Photon server when the game starts.
    // It uses the Photon Unity Networking (PUN) library to handle the connection.

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Connect to the Photon server using the settings defined in the PhotonServerSettings file
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        // This method is called when the client successfully connects to the Photon server.
        Debug.Log("Connected to Master Server");
        // Join a lobby after connecting to the master server
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        // This method is called when the client successfully joins a lobby on the Photon server.
        Debug.Log("Joined Lobby");
        // Load the main game scene after joining the lobby
        // You can replace "GameScene" with the name of your actual game scene
        SceneManager.LoadScene("Create Scenario Lobby");
    }
}
