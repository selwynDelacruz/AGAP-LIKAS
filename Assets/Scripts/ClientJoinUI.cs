using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using TMPro;

public class ClientJoinUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private Button joinButton;

    [Header("Port used by the host")]
    public ushort port = 7777;

    private void Awake()
    {
        joinButton.onClick.AddListener(JoinAsClient);
    }

    private void JoinAsClient()
    {
        string ip = ipInputField.text.Trim();

        if (string.IsNullOrEmpty(ip))
        {
            Debug.LogWarning("No IP entered!");
            return;
        }

        // Setup connection
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, port);

        // Start client
        bool success = NetworkManager.Singleton.StartClient();

        if (success)
        {
            Debug.Log("Attempting to connect to host at: " + ip);
            // Load your game scene
            SceneManager.LoadScene("TestKen");
        }
        else
        {
            Debug.LogError("Client failed to start.");
        }
    }
}
