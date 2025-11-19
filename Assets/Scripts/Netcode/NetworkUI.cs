using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


public class NetworkUI : MonoBehaviour
{

    [SerializeField] private Button instructorBTN; //HostBTN
    [SerializeField] private Button traineeBTN; //ClientBTN
    [SerializeField] private Button serverBTN; //ServerBTN

    private void Awake()
    {
        instructorBTN.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });
        traineeBTN.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });
        serverBTN.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });
    }
}
