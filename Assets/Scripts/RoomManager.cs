
using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviourPunCallbacks
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
				Object.Destroy(base.gameObject);
			}
			if (isFromMainGame)
			{
				AuthManager.Instance.RefreshData();
				Object.Destroy(base.gameObject);
			}
			else
			{
				Object.DontDestroyOnLoad(base.gameObject);
				Instance = this;
			}
		}
	}

	private void Start()
	{
		PlayerPrefs.GetString("Type_Of_User");
		if (!isFromMainGame)
		{
			Object.DontDestroyOnLoad(base.gameObject);
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
				Object.Destroy(base.gameObject);
			}
			if (isFromMainGameAndRefreshSceneOnly)
			{
				AuthManager.Instance.RefreshData();
				Object.Destroy(base.gameObject);
			}
		}
	}

	public bool checkInstructor()
	{
		if (!(PlayerPrefs.GetString("Type_Of_User") == "instructor"))
		{
			return false;
		}
		return true;
	}

	public override void OnEnable()
	{
		base.OnEnable();
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	public override void OnDisable()
	{
		base.OnDisable();
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		if (scene.name == "MainGame")
		{
			PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity, 0);
		}
		else if (scene.name == "ClearingUp_MainGameScene")
		{
			PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity, 0);
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
