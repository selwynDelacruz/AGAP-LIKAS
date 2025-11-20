using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode.Transports.UTP;

public class NetworkUI : MonoBehaviour
{
    [SerializeField] private Button instructorBTN;   // HostBTN
    [SerializeField] private Button traineeBTN;      // ClientBTN
    [SerializeField] private Button serverBTN;       // ServerBTN

    [Header("Hardcoded Host IP (Instructor PC / Hotspot IP)")]
    public string hostIPAddress = "172.16.86.182";   // <-- change this to your hotspot/PC IP
    public ushort port = 7777;

    private void Awake()
    {
        instructorBTN.onClick.AddListener(StartAsInstructor);
        traineeBTN.onClick.AddListener(StartAsTrainee);
        serverBTN.onClick.AddListener(StartAsServer);
    }

    private void StartAsInstructor()
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        // Host binds to all interfaces
        transport.SetConnectionData("0.0.0.0", port);

        NetworkManager.Singleton.StartHost();
        Debug.Log("Instructor started as HOST.");
    }

    private void StartAsTrainee()
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        // Client connects to hard-coded host IP
        transport.SetConnectionData(hostIPAddress, port);

        NetworkManager.Singleton.StartClient();
        Debug.Log($"Trainee started as CLIENT. Connecting to {hostIPAddress}:{port}");
    }

    private void StartAsServer()
    {
        NetworkManager.Singleton.StartServer();
        Debug.Log("Server only started.");
    }
}
