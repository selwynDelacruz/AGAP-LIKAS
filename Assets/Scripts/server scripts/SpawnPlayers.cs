using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject instructorPrefab; // Assign in Inspector
    public GameObject traineePrefab;    // Assign in Inspector
    public Transform traineeSpawnPoint; // Assign in Inspector
    public Transform instructorSpawnPoint; // Assign in Inspector

    public GameObject instructorScreenGUI; // Assign in Inspector
    public GameObject traineeScreenGUI;    // Assign in Inspector

    void Start()
    {
        bool isInstructor = PhotonNetwork.IsMasterClient;

        if (isInstructor)
        {
            // Spawn Instructor
            GameObject instructor = PhotonNetwork.Instantiate(instructorPrefab.name, instructorSpawnPoint.position, instructorSpawnPoint.rotation);
            // Enable Instructor GUI, disable Trainee GUI
            if (instructorScreenGUI) instructorScreenGUI.SetActive(true);
            if (traineeScreenGUI) traineeScreenGUI.SetActive(false);
        }
        else
        {
            // Spawn Trainee
            GameObject trainee = PhotonNetwork.Instantiate(traineePrefab.name, traineeSpawnPoint.position, traineeSpawnPoint.rotation);
            // Enable Trainee GUI, disable Instructor GUI
            if (instructorScreenGUI) instructorScreenGUI.SetActive(false);
            if (traineeScreenGUI) traineeScreenGUI.SetActive(true);

            // Set camera to follow the trainee
            BoatCameraFollow cameraFollow = Camera.main.GetComponent<BoatCameraFollow>();
            if (cameraFollow != null)
            {
                cameraFollow.boat = trainee.transform;
            }
        }
    }
}
