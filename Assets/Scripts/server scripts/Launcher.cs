using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class Launcher : MonoBehaviourPunCallbacks
{
    public PhotonView traineePrefab;
    public Transform traineeSpawnPoint; // The spawn point for the trainee prefab
    public PhotonView instructorPrefab;
    // This script is responsible foronnecting to the Photon server when the game starts.
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
        PhotonNetwork.JoinRandomOrCreateRoom();
    }

    public override void OnJoinedRoom()
    {
        // This method is called when the client successfully joins a lobby on the Photon server.
        Debug.Log("Joined Lobby");
        // Load the main game scene after joining the lobby
        // You can replace "GameScene" with the name of your actual game scene
        // Use the spawn point's position and rotation
        GameObject boatInstance = PhotonNetwork.Instantiate(traineePrefab.name, traineeSpawnPoint.position, traineeSpawnPoint.rotation);
        //instantiated the boat prefab and get the instance references
        // Find the camera and set its follow target
        BoatCameraFollow cameraFollow = Camera.main.GetComponent<BoatCameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.boat = boatInstance.transform;
        }
    }
}
