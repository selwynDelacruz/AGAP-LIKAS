using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("Panels")]
    public GameObject instructorPanel; // "Instructor Disaster Setup"
    public GameObject traineePanel;    // "Trainee Lobby"

    [Header("Instructor Disaster Setup Fields")]
    public TMP_Text instructorNameText;
    public TMP_Text traineeNameText;
    public TMP_Text roomCodeText;

    [Header("Trainee Lobby Fields")]
    public TMP_Text traineePanelInstructorNameText;
    public TMP_Text traineePanelTraineeNameText;
    public TMP_Text traineePanelRoomCodeText;

    [Header("Start Button")]
    public Button startButton; // Only visible to Instructor

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            instructorPanel.SetActive(true);
            traineePanel.SetActive(false);
            startButton.gameObject.SetActive(true);

            // Optionally update instructor panel fields here if needed
        }
        else
        {
            instructorPanel.SetActive(false);
            traineePanel.SetActive(true);
            startButton.gameObject.SetActive(false);

            // Update Trainee Lobby TMP_Text fields
            UpdateTraineePanelFields();
        }

        UpdatePlayerNames();

        // Set the room code text for the main UI (if needed)
        if (roomCodeText != null && PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null)
        {
            roomCodeText.text = "Lobby Code: " + PhotonNetwork.CurrentRoom.Name;
        }
    }

    void UpdateTraineePanelFields()
    {
        string instructorName = "";
        string traineeName = PhotonNetwork.NickName;

        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            if (player.IsMasterClient)
                instructorName = player.NickName;
        }

        if (traineePanelInstructorNameText != null)
            traineePanelInstructorNameText.text = instructorName;

        if (traineePanelTraineeNameText != null)
            traineePanelTraineeNameText.text = traineeName;

        if (traineePanelRoomCodeText != null && PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null)
            traineePanelRoomCodeText.text = "Lobby Code: " + PhotonNetwork.CurrentRoom.Name;
    }

    void UpdatePlayerNames()
    {
        string instructorName = "";
        string traineeName = "";

        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            if (player.IsMasterClient)
                instructorName = player.NickName;
            else
                traineeName = player.NickName;
        }

        instructorNameText.text = instructorName;
        traineeNameText.text = traineeName;
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player otherPlayer)
    {
        UpdatePlayerNames();
        if (!PhotonNetwork.IsMasterClient)
            UpdateTraineePanelFields();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        UpdatePlayerNames();
        if (!PhotonNetwork.IsMasterClient)
            UpdateTraineePanelFields();
    }

    // Called by Instructor's Start Button
    public void OnStartButtonPressed()
    {
        photonView.RPC("StartDisasterScene", RpcTarget.All);
    }

    [PunRPC]
    void StartDisasterScene()
    {
        // Use the selected disaster from DropdownList
        string sceneName = DropdownList.SelectedDisaster;
        if (!string.IsNullOrEmpty(sceneName))
        {
            PhotonNetwork.LoadLevel(sceneName);
        }
    }
}
