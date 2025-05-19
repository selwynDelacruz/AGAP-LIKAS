using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject traineePrefab;
    public Transform traineeSpawnPoint;

    public GameObject instructorScreenGUI;
    public GameObject traineeScreenGUI;

    // Cache the last followed trainee for instructor
    private Transform lastTraineeTransform;

    void Start()
    {
        bool isInstructor = PhotonNetwork.IsMasterClient;

        if (isInstructor)
        {
            if (instructorScreenGUI) instructorScreenGUI.SetActive(true);
            if (traineeScreenGUI) traineeScreenGUI.SetActive(false);

            StartCoroutine(SetupSpectatorCamera());
        }
        else
        {
            GameObject trainee = PhotonNetwork.Instantiate(traineePrefab.name, traineeSpawnPoint.position, traineeSpawnPoint.rotation);
            if (instructorScreenGUI) instructorScreenGUI.SetActive(false);
            if (traineeScreenGUI) traineeScreenGUI.SetActive(true);

            BoatCameraFollow cameraFollow = Camera.main.GetComponent<BoatCameraFollow>();
            if (cameraFollow != null)
            {
                cameraFollow.boat = trainee.transform;
            }
        }
    }

    void Update()
    {
        // Only instructor needs to update the camera target
        if (PhotonNetwork.IsMasterClient)
        {
            Transform currentTrainee = FindTraineeTransform();
            if (currentTrainee != null && currentTrainee != lastTraineeTransform)
            {
                BoatCameraFollow cameraFollow = Camera.main.GetComponent<BoatCameraFollow>();
                if (cameraFollow != null)
                {
                    cameraFollow.boat = currentTrainee;
                    cameraFollow.mouseSensitivity = 0f;
                }
                lastTraineeTransform = currentTrainee;
            }
        }
    }

    // Coroutine to find the trainee and set as camera target (initial setup)
    System.Collections.IEnumerator SetupSpectatorCamera()
    {
        yield return new WaitForSeconds(1f);

        Transform traineeTransform = FindTraineeTransform();
        if (traineeTransform != null)
        {
            BoatCameraFollow cameraFollow = Camera.main.GetComponent<BoatCameraFollow>();
            if (cameraFollow != null)
            {
                cameraFollow.boat = traineeTransform;
                cameraFollow.mouseSensitivity = 0f;
            }
            lastTraineeTransform = traineeTransform;
        }
    }

    // Helper to find the trainee's transform
    private Transform FindTraineeTransform()
    {
        PhotonView[] allViews = GameObject.FindObjectsOfType<PhotonView>();
        foreach (var view in allViews)
        {
            if (!view.IsMine && view.gameObject.CompareTag("Trainee"))
            {
                return view.transform;
            }
        }
        return null;
    }
}
