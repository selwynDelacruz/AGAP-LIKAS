using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
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
            Debug.LogWarning("[ClientJoinUI] No IP entered!");
            return;
        }

        // Setup connection
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, port);

        // Start client
        bool success = NetworkManager.Singleton.StartClient();

        if (success)
        {
            Debug.Log($"[ClientJoinUI] Attempting to connect to host at: {ip}:{port}");
            
            // ? DON'T LOAD SCENE HERE!
            // The client will automatically follow when the host loads a scene
            // The host is responsible for loading scenes via NetworkSceneManager
            
            Debug.Log("[ClientJoinUI] Client started. Waiting for host to load scene...");
        }
        else
        {
            Debug.LogError("[ClientJoinUI] Client failed to start.");
        }
    }
}
