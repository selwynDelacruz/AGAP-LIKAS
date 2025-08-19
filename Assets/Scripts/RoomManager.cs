using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    public int myNumber;

    public int myTraineeNumber;

    public bool isInstructor;

    public int playersCount;

    public bool isFromMainGame;

    public bool isFromMainGameAndRefreshSceneOnly;

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            if (isFromMainGameAndRefreshSceneOnly)
            {
                AuthManager.Instance.RefreshData();
                Object.Destroy(gameObject);
            }
            if (isFromMainGame)
            {
                AuthManager.Instance.RefreshData();
                Object.Destroy(gameObject);
            }
            else
            {
                Object.DontDestroyOnLoad(gameObject);
                Instance = this;
            }
        }
    }

    private void Start()
    {
        PlayerPrefs.GetString("Type_Of_User");
        if (!isFromMainGame)
        {
            Object.DontDestroyOnLoad(gameObject);
            Instance = this;
        }
    }

    private void LateUpdate()
    {
        isInstructor = checkInstructor();
    }

    private void FixedUpdate()
    {
        if (SceneManager.GetActiveScene().name == "MainGame" && !isFromMainGame)
        {
            isFromMainGame = true;
            isFromMainGameAndRefreshSceneOnly = true;
        }
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            if (isFromMainGame)
            {
                AuthManager.Instance.RefreshData();
                Object.Destroy(gameObject);
            }
            if (isFromMainGameAndRefreshSceneOnly)
            {
                AuthManager.Instance.RefreshData();
                Object.Destroy(gameObject);
            }
        }
    }

    public bool checkInstructor()
    {
        return PlayerPrefs.GetString("Type_Of_User") == "instructor";
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.name == "MainGame")
        {
            // Replace PhotonNetwork.Instantiate with local Instantiate if needed
            // Example: Instantiate(Resources.Load<GameObject>(Path.Combine("PhotonPrefabs", "PlayerManager")), Vector3.zero, Quaternion.identity);
        }
        else if (scene.name == "ClearingUp_MainGameScene")
        {
            // Replace PhotonNetwork.Instantiate with local Instantiate if needed
            // Example: Instantiate(Resources.Load<GameObject>(Path.Combine("PhotonPrefabs", "PlayerManager")), Vector3.zero, Quaternion.identity);
        }
    }

    // private void Update()
    // {
    // 	if (!(SceneManager.GetActiveScene().name == "MainMenu") || !Launcher.Instance.isLocalPlayerAllSet)
    // 	{
    // 		return;
    // 	}
    // 	for (myNumber = 0; myNumber < GetPlayerNumber.Instance.newListPlayers.Count; myNumber++)
    // 	{
    // 		if (GetPlayerNumber.Instance.newListPlayers[myNumber] == GetPlayerNumber.Instance.myName)
    // 		{
    // 			myNumber++;
    // 		}
    // 	}
    // }
}
