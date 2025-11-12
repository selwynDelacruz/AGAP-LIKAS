using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


public class NetworkUI : MonoBehaviour
{

    [SerializeField] private Button instructorBTN; //HostBTN
    [SerializeField] private Button traineeBTN; //ClientBTN

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
    }
}
