using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class CreateAndJoin : MonoBehaviourPunCallbacks
{
    public TMP_InputField createInput;
    public TMP_InputField joinInput;
    public TMP_InputField traineeName;
    public TMP_InputField instructorName;

    public void createRoom()
    {
        // Use instructorName for instructor
        if (instructorName == null || string.IsNullOrEmpty(instructorName.text))
        {
            Debug.LogWarning("Instructor username cannot be empty.");
            return;
        }
        PhotonNetwork.NickName = instructorName.text;

        if (!string.IsNullOrEmpty(createInput.text))
            PhotonNetwork.CreateRoom(createInput.text);
        else
            Debug.LogWarning("Room name cannot be empty.");
    }

    public void joinRoom()
    {
        // Use traineeName for trainee
        if (traineeName == null || string.IsNullOrEmpty(traineeName.text))
        {
            Debug.LogWarning("Trainee username cannot be empty.");
            return;
        }
        PhotonNetwork.NickName = traineeName.text;

        if (!string.IsNullOrEmpty(joinInput.text))
            PhotonNetwork.JoinRoom(joinInput.text);
        else
            Debug.LogWarning("Room name cannot be empty.");
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Lobby");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Room creation failed: " + message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Join room failed: " + message);
    }
}