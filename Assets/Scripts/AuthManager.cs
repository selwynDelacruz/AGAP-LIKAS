// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// AuthManager
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AuthManager : MonoBehaviour
{
	public static AuthManager Instance;

	[Header("Firebase")]
	public DependencyStatus dependencyStatus;

	public FirebaseAuth auth;

	public FirebaseUser User;

	public DatabaseReference DBreference;

	[Header("Login Instructor")]
	public TMP_InputField Instructor_emailLoginField;

	public TMP_InputField Instructor_passwordLoginField;

	public TMP_Text Instructor_confirmLoginText;

	public Button Login_InstructorButton;

	[Header("Login Trainee")]
	public TMP_InputField Trainee_emailLoginField;

	public TMP_InputField Trainee_passwordLoginField;

	public TMP_Text Trainee_confirmLoginText;

	public Button Login_TraineeButton;

	[Header("Login Super Admin")]
	public TMP_InputField SuperAdmin_emailLoginField;

	public TMP_InputField SuperAdmin_passwordLoginField;

	public TMP_Text SuperAdmin_confirmLoginText;

	public Button Login_SuperAdminButton;

	[Header("Super Admin Account Register")]
	public TMP_InputField SuperAdmin_usernameRegisterField;

	public TMP_InputField SuperAdmin_emailRegisterField;

	public TMP_InputField SuperAdmin_passwordRegisterField;

	public TMP_Text SuperAdminCreateAccount_warningRegisterText;

	[Header("Instructor")]
	public GameObject Login_Instructor_Panel;

	public GameObject MenuPanel_Instructor;

	[Header("Trainee")]
	public GameObject Login_Trainee_Panel;

	public GameObject Register_Trainee_Panel;

	public GameObject MenuPanel_Trainee;

	[Header("Super Admin")]
	public GameObject Login_SuperAdmin_Panel;

	public GameObject Register_SuperAdmin_Panel;

	public GameObject MenuPanel_SuperAdmin;

	public GameObject CreateAccount_RegisterPanel;

	public Button ManageAccount_RegisterBTN_UI;

	public Button CreateAccount_RegisterBTN_UI;

	public TMP_InputField CreateAccount_Username;

	public TMP_InputField CreateAccount_Name;

	public TMP_InputField CreateAccount_Password;

	public TMP_Dropdown CreateAccount_Gender;

	public TMP_InputField CreateAccount_Age;

	public TMP_Text CreateAccount_warningRegisterText;

	public TMP_InputField SuperAdmin_Username;

	public TMP_InputField SuperAdmin_Name;

	public TMP_InputField SuperAdmin_Password;

	public TMP_Dropdown SuperAdmin_Gender;

	public TMP_Dropdown SuperAdmin_Age;

	public TMP_Text SuperAdminManageAccount_warningRegisterText;

	public TMP_InputField AdminNameText;

	public GameObject LoadingPanel;

	public GameObject SuccessfullyCreatedAccount;

	public GameObject ChooseTypeOfUserPanel;

	[Header("Get Player Data from Input Fields")]
	private string UsernameNew_ToSet;

	private string User_Name;

	private string User_Gender;

	private string User_Password;

	private string User_TypeOfUser;

	private int User_Age;

	public int minAge;

	private bool isClickedAddNewUser;

	public bool isOnLoadingPanel;

	public bool isOpenRegisterUser;

	private bool isManagerCreatingAccounts;

	public string typeOfUserToManage;

	[Header("Get User Data from Database")]
	public string Current_Name;

	public string Current_Gender;

	public int Current_Age;

	public int Current_Score;

	public string Current_Username;

	public GameObject RoomManagerGO;

	public GameObject playerData;

	public Transform UsersListContent;

	public GameObject ManageAccountPanel;

	public TMP_InputField[] ManageAccount_InputFields;

	public string AccountToManage_Name;

	public string AccountToManage_Age;

	public string AccountToManage_Gender;

	public string AccountToManage_Username;

	public string AccountToManage_Password;

	public string AccountToManage_Usertype;

	public GameObject LeaderboardPanel;

	public GameObject PlayerdataLeaderboard;

	public Transform Leaderboardcontent;

	public bool isFromMainGame;

	public TMP_Dropdown SelectedUserTypeToShow;

	public TMP_InputField SearchBarInputField;

	private void Start()
	{
		// // Add debug logs to verify Firebase initialization
		// Debug.Log("Checking Firebase setup...");
		
		// // Verify UI references
		// if (Login_InstructorButton == null || Login_TraineeButton == null || Login_SuperAdminButton == null) {
		// 	Debug.LogError("Missing UI button references");
		// }
		
		Time.timeScale = 1f;
		isOnLoadingPanel = true;
		isOpenRegisterUser = false;
		isManagerCreatingAccounts = false;
		if (PlayerPrefs.GetString("LoginAlready") == "true")
		{
			InvokeRepeating("Delay", 10f, 10f);
			if (!string.IsNullOrEmpty(Current_Name))
			{
				CancelInvoke("Delay");
			}
		}
		else
		{
			CancelInvoke("Delay");
		}
		if (PlayerPrefs.GetString("LoginAlready") != "true" && string.IsNullOrEmpty(Current_Name))
		{
			isOnLoadingPanel = false;
		}
		Invoke("Load_Instructor_AllData_ByAdmin", 1f);
	}

	private void Delay()
	{
		if (string.IsNullOrEmpty(Current_Name))
		{
			Debug.Log("Refresh Data!");
			RefreshData();
		}
	}

	private void Awake()
	{
		Instance = this;
		FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(delegate(Task<DependencyStatus> task)
		{
			dependencyStatus = task.Result;
			if (dependencyStatus == DependencyStatus.Available)
			{
				InitializeFirebase();
			}
			else
			{
				Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
				LoadingPanel.SetActive(value: true);
			}
		});
		Invoke("LoginAutomatic", 0.5f);
	}

	private void SetupScoreBeforeLoggingOut()
	{
		if (PlayerPrefs.GetString("isLogoutAllPlayerNow") == "true")
		{
			PlayerPrefs.SetString("isLogoutAllPlayerNow", "");
			StartCoroutine(setNewScore());
		}
	}

	private IEnumerator setNewScore()
	{
		isOnLoadingPanel = true;
		LoadingPanel.SetActive(value: true);
		yield return new WaitForSeconds(3f);
		isOnLoadingPanel = false;
		LoadingPanel.SetActive(value: false);
		LogoutButton();
	}

	public void LogoutButton()
	{
		Object.DestroyImmediate(RoomManagerGO);
		PlayerPrefs.DeleteAll();
		isOnLoadingPanel = true;
		Invoke("DelayRefreshScene", 0.2f);
	}

	private void DelayRefreshScene()
	{
		SceneManager.LoadScene("MainMenu");
	}

	private void InitializeFirebase()
	{
		auth = FirebaseAuth.DefaultInstance;
		DBreference = FirebaseDatabase.DefaultInstance.RootReference;
	}

	private void Update()
	{
		LoadingPanelControl();
	}

	private void LateUpdate()
	{
		if (isOpenRegisterUser && isClickedAddNewUser)
		{
			SuperAdmin_RegisterFieldChecker();
			isClickedAddNewUser = false;
		}
		if (isManagerCreatingAccounts && isClickedAddNewUser)
		{
			CreateAccount_RegisterFieldChecker();
			isClickedAddNewUser = false;
		}
	}

	public void SuperAdmin_OpenRegisterPanel()
	{
		if (!isOpenRegisterUser)
		{
			Register_SuperAdmin_Panel.SetActive(value: true);
			Login_SuperAdmin_Panel.SetActive(value: false);
			isOpenRegisterUser = true;
		}
		else
		{
			Register_SuperAdmin_Panel.SetActive(value: false);
			Login_SuperAdmin_Panel.SetActive(value: true);
			isOpenRegisterUser = false;
		}
	}

	public void CreateAccount_OpenRegisterPanel()
	{
		if (!isManagerCreatingAccounts)
		{
			CreateAccount_RegisterPanel.SetActive(value: true);
			MenuPanel_SuperAdmin.SetActive(value: false);
			isManagerCreatingAccounts = true;
		}
		else
		{
			CreateAccount_RegisterPanel.SetActive(value: false);
			MenuPanel_SuperAdmin.SetActive(value: true);
			isManagerCreatingAccounts = false;
		}
	}

	public void NewUsernameToSet(string UsernameNew_)
	{
		UsernameNew_ToSet = UsernameNew_;
	}

	public void GetPlayerName(string _name)
	{
		User_Name = _name;
	}

	public void GetPlayerPassword(string _password)
	{
		User_Password = _password;
	}

	public void GetPlayerTypeUser(string _userType)
	{
		User_TypeOfUser = _userType;
	}

	public void GetPlayerGender(int _genderNum)
	{
		switch (_genderNum)
		{
		case 0:
			User_Gender = "";
			break;
		case 1:
			User_Gender = "male";
			break;
		case 2:
			User_Gender = "female";
			break;
		default:
			User_Gender = "";
			break;
		}
	}

	public void GetPlayerAge(string _age)
	{
		User_Age = int.Parse(_age);
	}

	public void LoginButton()
	{
		switch (PlayerPrefs.GetString("Type_Of_User"))
		{
		case "instructor":
			if (Login_Instructor_Panel.activeSelf)
			{
				StartCoroutine(Login(Instructor_emailLoginField.text + "@gmail.com", Instructor_passwordLoginField.text));
			}
			break;
		case "trainee":
			if (Login_Trainee_Panel.activeSelf)
			{
				StartCoroutine(Login(Trainee_emailLoginField.text + "@gmail.com", Trainee_passwordLoginField.text));
			}
			break;
		case "super_admin":
			if (Login_SuperAdmin_Panel.activeSelf)
			{
				StartCoroutine(Login(SuperAdmin_emailLoginField.text + "@gmail.com", SuperAdmin_passwordLoginField.text));
			}
			break;
		}
	}

	public void SuperAdminRegisterButton()
	{
		isClickedAddNewUser = true;
		StartCoroutine(Register_SuperAdmin(SuperAdmin_emailRegisterField.text + "@gmail.com", SuperAdmin_passwordRegisterField.text, SuperAdmin_emailRegisterField.text, User_Gender, User_Name, User_Age));
	}

	public void CreateAccount_ManageAccount_RegisterButton()
	{
		isClickedAddNewUser = true;
		string text = typeOfUserToManage;
		if (!(text == "instructor"))
		{
			if (text == "trainee")
			{
				StartCoroutine(Register_Trainee(CreateAccount_Username.text + "@gmail.com", CreateAccount_Password.text, CreateAccount_Username.text, User_Gender, User_Name, User_Age));
			}
		}
		else
		{
			StartCoroutine(Register_Instructor(CreateAccount_Username.text + "@gmail.com", CreateAccount_Password.text, CreateAccount_Username.text, User_Gender, User_Name, User_Age));
		}
	}

	private IEnumerator Login(string _email, string _password)
	{
		Task<AuthResult> LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
		yield return new WaitUntil(() => LoginTask.IsCompleted);
		if (LoginTask.Exception != null)
		{
			isOnLoadingPanel = false;
			Debug.LogWarning($"Failed to register task with {LoginTask.Exception}");
			AuthError errorCode = (AuthError)(LoginTask.Exception.GetBaseException() as FirebaseException).ErrorCode;
			string messsage = "Login Failed!";
			switch (errorCode)
			{
			case AuthError.MissingEmail:
				messsage = "Missing Email!";
				break;
			case AuthError.MissingPassword:
				messsage = "Missing Password!";
				break;
			case AuthError.WrongPassword:
				messsage = "Wrong Password";
				break;
			case AuthError.InvalidEmail:
				messsage = "Invalid Email!";
				break;
			case AuthError.UserNotFound:
				messsage = "Account does not exist!";
				break;
			}
			switch (PlayerPrefs.GetString("Type_Of_User"))
			{
			case "instructor":
				if (Login_Instructor_Panel.activeSelf)
				{
					SetWarning_LoginInfoText(messsage, "red");
				}
				break;
			case "trainee":
				if (Login_Trainee_Panel.activeSelf)
				{
					SetWarning_LoginInfoText(messsage, "red");
				}
				break;
			case "super_admin":
				if (Login_SuperAdmin_Panel.activeSelf)
				{
					SetWarning_LoginInfoText(messsage, "red");
				}
				break;
			}
			yield break;
		}
		User = LoginTask.Result.User;
		Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
		StartCoroutine(LoadUserData(PlayerPrefs.GetString("Type_Of_User")));
		PlayerPrefs.SetString("LoginAlready", "true");
		SetWarning_LoginInfoText("", "white");
		switch (PlayerPrefs.GetString("Type_Of_User"))
		{
		case "instructor":
			if (Login_Instructor_Panel.activeSelf)
			{
				PlayerPrefs.SetString("LoginEmail", _email);
				PlayerPrefs.SetString("LoginPassword", _password);
				PlayerPrefs.Save();
				Instructor_confirmLoginText.text = "Logged In";
				Login_InstructorButton.interactable = false;
			}
			break;
		case "trainee":
			if (Login_Trainee_Panel.activeSelf)
			{
				PlayerPrefs.SetString("LoginEmail", _email);
				PlayerPrefs.SetString("LoginPassword", _password);
				PlayerPrefs.Save();
				Trainee_confirmLoginText.text = "Logged In";
				Login_TraineeButton.interactable = false;
			}
			break;
		case "super_admin":
			if (Login_SuperAdmin_Panel.activeSelf)
			{
				PlayerPrefs.SetString("LoginEmail", _email);
				PlayerPrefs.SetString("LoginPassword", _password);
				PlayerPrefs.Save();
				SuperAdmin_confirmLoginText.text = "Logged In";
				Login_SuperAdminButton.interactable = false;
			}
			break;
		}
		yield return new WaitForSeconds(3f);
		switch (PlayerPrefs.GetString("Type_Of_User"))
		{
		case "instructor":
			if (!Login_Instructor_Panel.activeSelf)
			{
				break;
			}
			SetWarning_LoginInfoText("Successfully Login!", "white");
			Login_Instructor_Panel.SetActive(value: false);
			MenuPanel_Instructor.SetActive(value: true);
			if (PlayerPrefs.GetString("LoginAlready") == "true")
			{
				if (Current_Name == "")
				{
					InvokeRepeating("RepeatSettingUpLoadingAfterLogin", 0.3f, 0.3f);
					break;
				}
				CancelInvoke("RepeatSettingUpLoadingAfterLogin");
				isOnLoadingPanel = false;
			}
			else
			{
				CancelInvoke("RepeatSettingUpLoadingAfterLogin");
				isOnLoadingPanel = false;
			}
			break;
		case "trainee":
			if (!Login_Trainee_Panel.activeSelf)
			{
				break;
			}
			SetWarning_LoginInfoText("Successfully Login!", "white");
			Login_Trainee_Panel.SetActive(value: false);
			MenuPanel_Trainee.SetActive(value: true);
			if (PlayerPrefs.GetString("LoginAlready") == "true")
			{
				if (Current_Name == "")
				{
					InvokeRepeating("RepeatSettingUpLoadingAfterLogin", 0.3f, 0.3f);
					break;
				}
				CancelInvoke("RepeatSettingUpLoadingAfterLogin");
				isOnLoadingPanel = false;
			}
			else
			{
				CancelInvoke("RepeatSettingUpLoadingAfterLogin");
				isOnLoadingPanel = false;
			}
			break;
		case "super_admin":
			if (!Login_SuperAdmin_Panel.activeSelf)
			{
				break;
			}
			SetWarning_LoginInfoText("Successfully Login!", "white");
			Login_SuperAdmin_Panel.SetActive(value: false);
			MenuPanel_SuperAdmin.SetActive(value: true);
			if (PlayerPrefs.GetString("LoginAlready") == "true")
			{
				if (string.IsNullOrEmpty(Current_Name))
				{
					InvokeRepeating("RepeatSettingUpLoadingAfterLogin", 0.1f, 0.1f);
					break;
				}
				CancelInvoke("RepeatSettingUpLoadingAfterLogin");
				isOnLoadingPanel = false;
			}
			else
			{
				CancelInvoke("RepeatSettingUpLoadingAfterLogin");
				isOnLoadingPanel = false;
			}
			break;
		}
	}

	private IEnumerator Login_ManageAccount(string _email, string _password)
	{
		Task<AuthResult> LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
		yield return new WaitUntil(() => LoginTask.IsCompleted);
		if (LoginTask.Exception != null)
		{
			isOnLoadingPanel = true;
			Debug.LogWarning($"Failed to register task with {LoginTask.Exception}");
			switch ((AuthError)(LoginTask.Exception.GetBaseException() as FirebaseException).ErrorCode)
			{
			}
			yield break;
		}
		User = LoginTask.Result.User;
		Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
		yield return new WaitForSeconds(3f);
		isOnLoadingPanel = false;
	}

	private void RepeatSettingUpLoadingAfterLogin()
	{
		isOnLoadingPanel = true;
	}

	private IEnumerator Register_SuperAdmin(string _email, string _password, string _username, string _gender, string _name, int _age)
	{
		if (!string.IsNullOrEmpty(_name) && !string.IsNullOrEmpty(_gender) && !string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(SuperAdmin_passwordRegisterField.text) && SuperAdmin_passwordRegisterField.text.Length > 5 && User_Age >= 1)
		{
			Task<AuthResult> RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
			yield return new WaitUntil(() => RegisterTask.IsCompleted);
			if (RegisterTask.Exception != null)
			{
				Debug.LogWarning($"Failed to register task with {RegisterTask.Exception}");
				AuthError errorCode = (AuthError)(RegisterTask.Exception.GetBaseException() as FirebaseException).ErrorCode;
				string messsage = "Register Failed! Please check your internet connection";
				switch (errorCode)
				{
				case AuthError.MissingEmail:
					messsage = "Missing Username!";
					break;
				case AuthError.MissingPassword:
					messsage = "Missing Password!";
					break;
				case AuthError.WeakPassword:
					messsage = "Weak Password";
					break;
				case AuthError.EmailAlreadyInUse:
					messsage = "Username already in use!";
					break;
				}
				SuperAdmin_SetWarning_RegisterInfoText(messsage, "red");
				CreateAccount_RegisterBTN_UI.interactable = true;
				yield break;
			}
			User = RegisterTask.Result.User;
			if (User != null)
			{
				UserProfile profile = new UserProfile
				{
					DisplayName = _username
				};
				Task ProfileTask = User.UpdateUserProfileAsync(profile);
				yield return new WaitUntil(() => ProfileTask.IsCompleted);
				if (ProfileTask.Exception != null)
				{
					Debug.LogWarning($"Failed to register task with {ProfileTask.Exception}");
					_ = (ProfileTask.Exception.GetBaseException() as FirebaseException).ErrorCode;
					SuperAdmin_SetWarning_RegisterInfoText("Username set Failed!", "red");
					CreateAccount_RegisterBTN_UI.interactable = true;
				}
				else
				{
					SetDefaultPlayerData("super_admin");
					StartCoroutine(RegisterSuccessShowPanel());
					yield return new WaitForSeconds(5f);
					isOpenRegisterUser = false;
					Login_SuperAdmin_Panel.SetActive(value: true);
					Register_SuperAdmin_Panel.SetActive(value: false);
					SuperAdmin_RegisterNewAccount();
				}
			}
		}
		else
		{
			SuperAdmin_SetWarning_RegisterInfoText("Make sure all the data is correct!", "red");
		}
	}

	private IEnumerator Register_Instructor(string _email, string _password, string _username, string _gender, string _name, int _age)
	{
		if (string.IsNullOrEmpty(_name) && string.IsNullOrEmpty(_gender) && string.IsNullOrEmpty(_username) && string.IsNullOrEmpty(CreateAccount_Password.text) && CreateAccount_Password.text.Length <= 5 && User_Age == 0)
		{
			CreateAccount_SetWarning_RegisterInfoText("Make sure all the data is correct!", "red");
		}
		else
		{
			if (string.IsNullOrEmpty(_name) || string.IsNullOrEmpty(_gender) || string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(CreateAccount_Password.text) || CreateAccount_Password.text.Length <= 5 || User_Age <= 0)
			{
				yield break;
			}
			Task<AuthResult> RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
			yield return new WaitUntil(() => RegisterTask.IsCompleted);
			if (RegisterTask.Exception != null)
			{
				Debug.LogWarning($"Failed to register task with {RegisterTask.Exception}");
				AuthError errorCode = (AuthError)(RegisterTask.Exception.GetBaseException() as FirebaseException).ErrorCode;
				string messsage = "Register Failed! Please check your internet connection";
				switch (errorCode)
				{
				case AuthError.MissingEmail:
					messsage = "Missing Username!";
					break;
				case AuthError.MissingPassword:
					messsage = "Missing Password!";
					break;
				case AuthError.WeakPassword:
					messsage = "Weak Password";
					break;
				case AuthError.EmailAlreadyInUse:
					messsage = "Username already in use!";
					break;
				}
				CreateAccount_SetWarning_RegisterInfoText(messsage, "red");
				ManageAccount_RegisterBTN_UI.interactable = true;
				yield break;
			}
			User = RegisterTask.Result.User;
			if (User != null)
			{
				UserProfile profile = new UserProfile
				{
					DisplayName = _username
				};
				Task ProfileTask = User.UpdateUserProfileAsync(profile);
				CreateAccount_SetWarning_RegisterInfoText("", "white");
				yield return new WaitUntil(() => ProfileTask.IsCompleted);
				if (ProfileTask.Exception != null)
				{
					Debug.LogWarning($"Failed to register task with {ProfileTask.Exception}");
					_ = (ProfileTask.Exception.GetBaseException() as FirebaseException).ErrorCode;
					CreateAccount_SetWarning_RegisterInfoText("Username set Failed!", "red");
					ManageAccount_RegisterBTN_UI.interactable = true;
				}
				else
				{
					SetDefaultPlayerData("instructor");
					isOnLoadingPanel = true;
					yield return new WaitForSeconds(5f);
					isOnLoadingPanel = false;
					CreateAccount_RegisterNewAccount();
				}
			}
		}
	}

	private IEnumerator Register_Trainee(string _email, string _password, string _username, string _gender, string _name, int _age)
	{
		if (string.IsNullOrEmpty(_name) && string.IsNullOrEmpty(_gender) && string.IsNullOrEmpty(_username) && string.IsNullOrEmpty(CreateAccount_Password.text) && CreateAccount_Password.text.Length <= 5 && User_Age == 0)
		{
			CreateAccount_SetWarning_RegisterInfoText("Make sure all the data is correct!", "red");
		}
		else
		{
			if (string.IsNullOrEmpty(_name) || string.IsNullOrEmpty(_gender) || string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(CreateAccount_Password.text) || CreateAccount_Password.text.Length <= 5 || User_Age <= 0)
			{
				yield break;
			}
			Task<AuthResult> RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
			yield return new WaitUntil(() => RegisterTask.IsCompleted);
			if (RegisterTask.Exception != null)
			{
				Debug.LogWarning($"Failed to register task with {RegisterTask.Exception}");
				AuthError errorCode = (AuthError)(RegisterTask.Exception.GetBaseException() as FirebaseException).ErrorCode;
				string messsage = "Register Failed! Please check your internet connection";
				switch (errorCode)
				{
				case AuthError.MissingEmail:
					messsage = "Missing Username!";
					break;
				case AuthError.MissingPassword:
					messsage = "Missing Password!";
					break;
				case AuthError.WeakPassword:
					messsage = "Weak Password";
					break;
				case AuthError.EmailAlreadyInUse:
					messsage = "Username already in use!";
					break;
				}
				CreateAccount_SetWarning_RegisterInfoText(messsage, "red");
				ManageAccount_RegisterBTN_UI.interactable = true;
				yield break;
			}
			User = RegisterTask.Result.User;
			if (User != null)
			{
				UserProfile profile = new UserProfile
				{
					DisplayName = _username
				};
				Task ProfileTask = User.UpdateUserProfileAsync(profile);
				yield return new WaitUntil(() => ProfileTask.IsCompleted);
				if (ProfileTask.Exception != null)
				{
					Debug.LogWarning($"Failed to register task with {ProfileTask.Exception}");
					_ = (ProfileTask.Exception.GetBaseException() as FirebaseException).ErrorCode;
					CreateAccount_SetWarning_RegisterInfoText("Username set Failed!", "red");
					ManageAccount_RegisterBTN_UI.interactable = true;
				}
				else
				{
					SetDefaultPlayerData("trainee");
					isOnLoadingPanel = true;
					yield return new WaitForSeconds(5f);
					isOnLoadingPanel = false;
					CreateAccount_RegisterNewAccount();
				}
			}
		}
	}

	private void SuperAdmin_RegisterFieldChecker()
	{
		if (User_Name != "" && User_Gender != "" && UsernameNew_ToSet != "" && SuperAdmin_passwordRegisterField.text != "" && SuperAdmin_passwordRegisterField.text.Length > 5 && User_Age > 0)
		{
			SuperAdmin_SetWarning_RegisterInfoText("Completed all the information! Click Register to save all information. ", "white");
			CreateAccount_RegisterBTN_UI.interactable = false;
		}
		else if (User_Name != "" && User_Gender != "" && UsernameNew_ToSet != "" && SuperAdmin_passwordRegisterField.text != "" && SuperAdmin_passwordRegisterField.text.Length > 0 && SuperAdmin_passwordRegisterField.text.Length <= 5 && User_Age > 0)
		{
			SuperAdmin_SetWarning_RegisterInfoText("Weak password!", "yellow");
		}
		else
		{
			SuperAdmin_SetWarning_RegisterInfoText("Incomplete information! Please fill all the information.", "red");
		}
	}

	private void CreateAccount_RegisterFieldChecker()
	{
		if (User_Name != "" && User_Gender != "" && UsernameNew_ToSet != "" && CreateAccount_Password.text != "" && CreateAccount_Password.text.Length > 5 && User_Age > 0)
		{
			CreateAccount_SetWarning_RegisterInfoText("Completed all the information! saving all the data. ", "yellow");
			ManageAccount_RegisterBTN_UI.interactable = false;
		}
		else if (User_Name != "" && User_Gender != "" && UsernameNew_ToSet != "" && CreateAccount_Password.text != "" && CreateAccount_Password.text.Length > 0 && CreateAccount_Password.text.Length <= 5 && User_Age > 0)
		{
			CreateAccount_SetWarning_RegisterInfoText("Weak password!", "yellow");
		}
		else
		{
			CreateAccount_SetWarning_RegisterInfoText("Incomplete information! Please fill all the information above.", "red");
		}
	}

	private void LoginAutomatic()
	{
		if (PlayerPrefs.GetString("LoginAlready") == "true")
		{
			switch (PlayerPrefs.GetString("Type_Of_User"))
			{
			case "instructor":
				StartCoroutine(Login(PlayerPrefs.GetString("LoginEmail"), PlayerPrefs.GetString("LoginPassword")));
				break;
			case "trainee":
				StartCoroutine(Login(PlayerPrefs.GetString("LoginEmail"), PlayerPrefs.GetString("LoginPassword")));
				break;
			case "super_admin":
				StartCoroutine(Login(PlayerPrefs.GetString("LoginEmail"), PlayerPrefs.GetString("LoginPassword")));
				break;
			}
		}
	}

	private void SetDefaultPlayerData(string _typeUser)
	{
		StartCoroutine(UpdateuserName(UsernameNew_ToSet, _typeUser));
		StartCoroutine(UpdateName(User_Name, _typeUser));
		StartCoroutine(UpdateGender(User_Gender, _typeUser));
		StartCoroutine(UpdateUserAge(User_Age, _typeUser));
		StartCoroutine(UpdateUserPassword(User_Password, _typeUser));
		StartCoroutine(UpdateUserType(_typeUser, _typeUser));
		SetPlayerScore(0, _typeUser);
	}

	private void SuperAdmin_SetWarning_RegisterInfoText(string _messsage, string color)
	{
		SuperAdminCreateAccount_warningRegisterText.text = _messsage;
		switch (color)
		{
		case "red":
			SuperAdminCreateAccount_warningRegisterText.color = Color.red;
			break;
		case "yellow":
			SuperAdminCreateAccount_warningRegisterText.color = Color.yellow;
			break;
		case "white":
			SuperAdminCreateAccount_warningRegisterText.color = Color.white;
			break;
		}
	}

	private void CreateAccount_SetWarning_RegisterInfoText(string _messsage, string color)
	{
		SuperAdminManageAccount_warningRegisterText.text = _messsage;
		switch (color)
		{
		case "red":
			SuperAdminManageAccount_warningRegisterText.color = Color.red;
			break;
		case "yellow":
			SuperAdminManageAccount_warningRegisterText.color = Color.yellow;
			break;
		case "white":
			SuperAdminManageAccount_warningRegisterText.color = Color.white;
			break;
		}
	}

	private void SetWarning_LoginInfoText(string _messsage, string color)
	{
		switch (PlayerPrefs.GetString("Type_Of_User"))
		{
		case "instructor":
			if (Login_Instructor_Panel.activeSelf)
			{
				Instructor_confirmLoginText.text = _messsage;
				if (color == "red")
				{
					Instructor_confirmLoginText.color = Color.red;
				}
				if (color == "yellow")
				{
					Instructor_confirmLoginText.color = Color.yellow;
				}
				if (color == "white")
				{
					Instructor_confirmLoginText.color = Color.white;
				}
			}
			break;
		case "trainee":
			if (Login_Trainee_Panel.activeSelf)
			{
				Trainee_confirmLoginText.text = _messsage;
				if (color == "red")
				{
					Trainee_confirmLoginText.color = Color.red;
				}
				if (color == "yellow")
				{
					Trainee_confirmLoginText.color = Color.yellow;
				}
				if (color == "white")
				{
					Trainee_confirmLoginText.color = Color.white;
				}
			}
			break;
		case "super_admin":
			if (Login_SuperAdmin_Panel.activeSelf)
			{
				SuperAdmin_confirmLoginText.text = _messsage;
				if (color == "red")
				{
					SuperAdmin_confirmLoginText.color = Color.red;
				}
				if (color == "yellow")
				{
					SuperAdmin_confirmLoginText.color = Color.yellow;
				}
				if (color == "white")
				{
					SuperAdmin_confirmLoginText.color = Color.white;
				}
			}
			break;
		}
	}

	public void SetPlayerScore(int _playerScore, string _typeUser)
	{
		StartCoroutine(UpdateUserScore(_playerScore, _typeUser));
	}

	private void LoadingPanelControl()
	{
		if (isOnLoadingPanel)
		{
			LoadingPanel.SetActive(value: true);
		}
		else
		{
			LoadingPanel.SetActive(value: false);
		}
	}

	public void CreateAccount_RegisterNewAccount()
	{
		SuperAdminManageAccount_warningRegisterText.text = "";
		CreateAccount_Username.text = "";
		CreateAccount_Password.text = "";
		CreateAccount_Name.text = "";
		CreateAccount_Age.text = "";
		CreateAccount_Gender.SetValueWithoutNotify(0);
		ManageAccount_RegisterBTN_UI.interactable = true;
	}

	private void ClearAllLoginField(TMP_InputField usernameField, TMP_InputField passwordField, TMP_Text infoText)
	{
		usernameField.text = "";
		passwordField.text = "";
		infoText.text = "";
	}

	public void SuperAdmin_RegisterNewAccount()
	{
		SuperAdminCreateAccount_warningRegisterText.text = "";
		SuperAdmin_Username.text = "";
		SuperAdmin_Password.text = "";
		SuperAdmin_Name.text = "";
		SuperAdmin_Gender.SetValueWithoutNotify(0);
		SuperAdmin_Age.SetValueWithoutNotify(0);
	}
	
	public void TestButtonClick()
	{
    	Debug.Log("Button clicked!");
	}

	public void Set_UserType_Select(string UserType)
	{
		switch (UserType)
		{
			case "instructor":
				Login_Instructor_Panel.SetActive(value: true);
				ChooseTypeOfUserPanel.SetActive(value: false);
				PlayerPrefs.SetString("Type_Of_User", UserType);
				PlayerPrefs.Save();
				Debug.Log("instructor clicked");
				break;
			case "trainee":
				Login_Trainee_Panel.SetActive(value: true);
				ChooseTypeOfUserPanel.SetActive(value: false);
				PlayerPrefs.SetString("Type_Of_User", UserType);
				Debug.Log("trainee clicked");
				PlayerPrefs.Save();
				break;
			case "super_admin":
				Login_SuperAdmin_Panel.SetActive(value: true);
				ChooseTypeOfUserPanel.SetActive(value: false);
				PlayerPrefs.SetString("Type_Of_User", UserType);
				PlayerPrefs.Save();
				Debug.Log("superadmin clicked");

				break;
		}
	}

	public void TypeOfUserToManager(string _type)
	{
		typeOfUserToManage = _type;
		CreateAccount_RegisterPanel.SetActive(value: true);
		MenuPanel_SuperAdmin.SetActive(value: false);
	}

	public void GoBackToMenuAdminPanel()
	{
		CreateAccount_RegisterPanel.SetActive(value: false);
		MenuPanel_SuperAdmin.SetActive(value: true);
	}

	public void ReturnToChooseUserType()
	{
		switch (PlayerPrefs.GetString("Type_Of_User"))
		{
		case "instructor":
			ClearAllLoginField(Instructor_emailLoginField, Instructor_passwordLoginField, Instructor_confirmLoginText);
			Login_Instructor_Panel.SetActive(value: false);
			ChooseTypeOfUserPanel.SetActive(value: true);
			PlayerPrefs.SetString("Type_Of_User", "");
			PlayerPrefs.Save();
			break;
		case "trainee":
			ClearAllLoginField(Trainee_emailLoginField, Trainee_passwordLoginField, Trainee_confirmLoginText);
			Login_Trainee_Panel.SetActive(value: false);
			ChooseTypeOfUserPanel.SetActive(value: true);
			PlayerPrefs.SetString("Type_Of_User", "");
			PlayerPrefs.Save();
			break;
		case "super_admin":
			ClearAllLoginField(SuperAdmin_emailLoginField, SuperAdmin_passwordLoginField, SuperAdmin_confirmLoginText);
			Login_SuperAdmin_Panel.SetActive(value: false);
			ChooseTypeOfUserPanel.SetActive(value: true);
			PlayerPrefs.SetString("Type_Of_User", "");
			PlayerPrefs.Save();
			break;
		}
	}

	public void Load_Instructor_AllData_ByAdmin()
	{
		StartCoroutine(LoadInstructorList());
	}

	public void Load_Trainee_AllData_ByAdmin()
	{
		StartCoroutine(LoadTraineeList());
	}

	public void RefreshData()
	{
		Object.Destroy(RoomManager.Instance.gameObject);
		SceneManager.LoadScene("MainMenu");
	}

	private IEnumerator RegisterSuccessShowPanel()
	{
		SuccessfullyCreatedAccount.SetActive(value: true);
		yield return new WaitForSeconds(0.5f);
		SuccessfullyCreatedAccount.SetActive(value: false);
	}

	private IEnumerator UpdateuserName(string usernamenew_, string _typeOfUser)
	{
		Task DBTask = DBreference.Child(_typeOfUser).Child(User.UserId).Child("User_Username")
			.SetValueAsync(usernamenew_);
		yield return new WaitUntil(() => DBTask.IsCompleted);
		if (DBTask.Exception != null)
		{
			Debug.LogWarning($"Failed to register task with {DBTask.Exception}");
		}
	}

	private IEnumerator UpdateName(string name_, string _typeOfUser)
	{
		Task DBTask = DBreference.Child(_typeOfUser).Child(User.UserId).Child("User_Name")
			.SetValueAsync(name_);
		yield return new WaitUntil(() => DBTask.IsCompleted);
		if (DBTask.Exception != null)
		{
			Debug.LogWarning($"Failed to register task with {DBTask.Exception}");
		}
	}

	private IEnumerator UpdateGender(string gender_, string _typeOfUser)
	{
		Task DBTask = DBreference.Child(_typeOfUser).Child(User.UserId).Child("User_Gender")
			.SetValueAsync(gender_);
		yield return new WaitUntil(() => DBTask.IsCompleted);
		if (DBTask.Exception != null)
		{
			Debug.LogWarning($"Failed to register task with {DBTask.Exception}");
		}
	}

	private IEnumerator UpdateUserAge(int _age, string _typeOfUser)
	{
		Task DBTask = DBreference.Child(_typeOfUser).Child(User.UserId).Child("User_Age")
			.SetValueAsync(_age);
		yield return new WaitUntil(() => DBTask.IsCompleted);
		if (DBTask.Exception != null)
		{
			Debug.LogWarning($"Failed to register task with {DBTask.Exception}");
		}
	}

	private IEnumerator UpdateUserScore(int _score, string _typeOfUser)
	{
		Task DBTask = DBreference.Child(_typeOfUser).Child(User.UserId).Child("User_Score")
			.SetValueAsync(_score);
		yield return new WaitUntil(() => DBTask.IsCompleted);
		if (DBTask.Exception != null)
		{
			Debug.LogWarning($"Failed to register task with {DBTask.Exception}");
		}
	}

	private IEnumerator UpdateUserPassword(string _password, string _typeOfUser)
	{
		Task DBTask = DBreference.Child(_typeOfUser).Child(User.UserId).Child("User_Password")
			.SetValueAsync(_password);
		yield return new WaitUntil(() => DBTask.IsCompleted);
		if (DBTask.Exception != null)
		{
			Debug.LogWarning($"Failed to register task with {DBTask.Exception}");
		}
	}

	private IEnumerator UpdateUserType(string _userType, string _typeOfUser)
	{
		Task DBTask = DBreference.Child(_typeOfUser).Child(User.UserId).Child("User_Type")
			.SetValueAsync(_userType);
		yield return new WaitUntil(() => DBTask.IsCompleted);
		if (DBTask.Exception != null)
		{
			Debug.LogWarning($"Failed to register task with {DBTask.Exception}");
		}
	}

	private IEnumerator LoadUserData(string typeOfUser_)
	{
		Task<DataSnapshot> DBTask = DBreference.Child(typeOfUser_).Child(User.UserId).GetValueAsync();
		yield return new WaitUntil(() => DBTask.IsCompleted);
		if (DBTask.Exception != null)
		{
			Debug.LogWarning($"Failed to register task with {DBTask.Exception}");
		}
		else if (DBTask.Result.Value != null)
		{
			DataSnapshot result = DBTask.Result;
			Current_Name = result.Child("User_Name").Value.ToString();
			Current_Username = result.Child("User_Username").Value.ToString();
			Current_Gender = result.Child("User_Gender").Value.ToString();
			Current_Age = int.Parse(result.Child("User_Age").Value.ToString());
			Current_Score = int.Parse(result.Child("User_Score").Value.ToString());
			AdminNameText.text = "Admin Name : " + Current_Name;
			// Launcher.Instance.CallForSetupNickName();
			// Launcher.Instance.StopMethodRepeating();
			if (PlayerPrefs.GetString("SetNewScoreLeaderboard") == "true")
			{
				SetPlayerScore(Current_Score + PlayerPrefs.GetInt("ScoredGet"), result.Child("User_Type").Value.ToString());
				PlayerPrefs.SetString("SetNewScoreLeaderboard", "");
				SetupScoreBeforeLoggingOut();
			}
			else
			{
				PlayerPrefs.SetInt("ScoredGet", Current_Score);
				PlayerPrefs.Save();
			}
		}
	}

	private IEnumerator LoadInstructorList()
	{
		if (DBreference == null)
		{
			yield break;
		}
		Task<DataSnapshot> DBTask = DBreference.Child("instructor").OrderByChild("User_Username").GetValueAsync();
		yield return new WaitUntil(() => DBTask.IsCompleted);
		if (DBTask.Exception != null)
		{
			Debug.LogWarning($"Failed to register task with {DBTask.Exception}");
			yield break;
		}
		DataSnapshot result = DBTask.Result;
		foreach (Transform item in UsersListContent)
		{
			Object.Destroy(item.gameObject);
		}
		if (result.Children.Reverse() == null)
		{
			yield break;
		}
		foreach (DataSnapshot item2 in result.Children.Reverse())
		{
			string value = item2.Child("User_Name").Value.ToString();
			int age = int.Parse(item2.Child("User_Age").Value.ToString());
			string gender = item2.Child("User_Gender").Value.ToString();
			string username = item2.Child("User_Username").Value.ToString();
			if (!string.IsNullOrEmpty(item2.Child("User_Password").Value.ToString()))
			{
				string password = item2.Child("User_Password").Value.ToString();
				string usertype = item2.Child("User_Type").Value.ToString();
				if (!string.IsNullOrEmpty(value))
				{
					Object.Instantiate(playerData, UsersListContent).GetComponent<UsersElement>().ListData(usertype, value, age, gender, username, password);
				}
			}
		}
	}

	public void GetSearchBarUsers(string data)
	{
		if (SelectedUserTypeToShow.value == 0)
		{
			StartCoroutine(LoadInstructorList_SearchBar(data));
		}
		else
		{
			StartCoroutine(LoadTraineeList_SearchBar(data));
		}
	}

	public void LoadUserData(int num)
	{
		SearchBarInputField.text = "";
		if (num == 0)
		{
			StartCoroutine(LoadInstructorList());
		}
		else
		{
			StartCoroutine(LoadTraineeList());
		}
	}

	private IEnumerator LoadInstructorList_SearchBar(string searchUsername)
	{
		if (DBreference == null)
		{
			yield break;
		}
		Task<DataSnapshot> DBTask = DBreference.Child("instructor").OrderByChild("User_Username").GetValueAsync();
		yield return new WaitUntil(() => DBTask.IsCompleted);
		if (DBTask.Exception != null)
		{
			Debug.LogWarning($"Failed to register task with {DBTask.Exception}");
			yield break;
		}
		DataSnapshot result = DBTask.Result;
		foreach (Transform item in UsersListContent)
		{
			Object.Destroy(item.gameObject);
		}
		if (result.Children.Reverse() == null)
		{
			yield break;
		}
		foreach (DataSnapshot item2 in result.Children.Reverse())
		{
			string value = item2.Child("User_Name").Value.ToString();
			int age = int.Parse(item2.Child("User_Age").Value.ToString());
			string gender = item2.Child("User_Gender").Value.ToString();
			string text = item2.Child("User_Username").Value.ToString();
			if (string.IsNullOrEmpty(item2.Child("User_Password").Value.ToString()))
			{
				continue;
			}
			string password = item2.Child("User_Password").Value.ToString();
			string usertype = item2.Child("User_Type").Value.ToString();
			if (!string.IsNullOrEmpty(value))
			{
				if (text.Contains(searchUsername))
				{
					Object.Instantiate(playerData, UsersListContent).GetComponent<UsersElement>().ListData(usertype, value, age, gender, text, password);
				}
				else if (text == "")
				{
					Load_Instructor_AllData_ByAdmin();
				}
			}
		}
	}

	private IEnumerator LoadTraineeList()
	{
		if (DBreference == null)
		{
			yield break;
		}
		Task<DataSnapshot> DBTask = DBreference.Child("trainee").OrderByChild("User_Name").GetValueAsync();
		yield return new WaitUntil(() => DBTask.IsCompleted);
		if (DBTask.Exception != null)
		{
			Debug.LogWarning($"Failed to register task with {DBTask.Exception}");
			yield break;
		}
		DataSnapshot result = DBTask.Result;
		foreach (Transform item in UsersListContent)
		{
			Object.Destroy(item.gameObject);
		}
		if (result.Children.Reverse() == null)
		{
			yield break;
		}
		foreach (DataSnapshot item2 in result.Children.Reverse())
		{
			string value = item2.Child("User_Name").Value.ToString();
			int age = int.Parse(item2.Child("User_Age").Value.ToString());
			string gender = item2.Child("User_Gender").Value.ToString();
			string username = item2.Child("User_Username").Value.ToString();
			if (!string.IsNullOrEmpty(item2.Child("User_Password").Value.ToString()))
			{
				string password = item2.Child("User_Password").Value.ToString();
				string usertype = item2.Child("User_Type").Value.ToString();
				if (!string.IsNullOrEmpty(value))
				{
					Object.Instantiate(playerData, UsersListContent).GetComponent<UsersElement>().ListData(usertype, value, age, gender, username, password);
				}
			}
		}
	}

	private IEnumerator LoadTraineeList_SearchBar(string searchUsername)
	{
		if (DBreference == null)
		{
			yield break;
		}
		Task<DataSnapshot> DBTask = DBreference.Child("trainee").OrderByChild("User_Name").GetValueAsync();
		yield return new WaitUntil(() => DBTask.IsCompleted);
		if (DBTask.Exception != null)
		{
			Debug.LogWarning($"Failed to register task with {DBTask.Exception}");
			yield break;
		}
		DataSnapshot result = DBTask.Result;
		foreach (Transform item in UsersListContent)
		{
			Object.Destroy(item.gameObject);
		}
		if (result.Children.Reverse() == null)
		{
			yield break;
		}
		foreach (DataSnapshot item2 in result.Children.Reverse())
		{
			string value = item2.Child("User_Name").Value.ToString();
			int age = int.Parse(item2.Child("User_Age").Value.ToString());
			string gender = item2.Child("User_Gender").Value.ToString();
			string text = item2.Child("User_Username").Value.ToString();
			if (string.IsNullOrEmpty(item2.Child("User_Password").Value.ToString()))
			{
				continue;
			}
			string password = item2.Child("User_Password").Value.ToString();
			string usertype = item2.Child("User_Type").Value.ToString();
			if (!string.IsNullOrEmpty(value))
			{
				if (text.Contains(searchUsername))
				{
					Object.Instantiate(playerData, UsersListContent).GetComponent<UsersElement>().ListData(usertype, value, age, gender, text, password);
				}
				else if (text == "")
				{
					Load_Instructor_AllData_ByAdmin();
				}
			}
		}
	}

	public void ShowLeaderboard()
	{
		StartCoroutine(LoadLeaderboardData());
	}

	private IEnumerator LoadLeaderboardData()
	{
		if (DBreference == null)
		{
			yield break;
		}
		Task<DataSnapshot> DBTask = DBreference.Child("trainee").OrderByChild("User_Score").GetValueAsync();
		yield return new WaitUntil(() => DBTask.IsCompleted);
		if (DBTask.Exception != null)
		{
			Debug.LogWarning($"Failed to register task with {DBTask.Exception}");
			yield break;
		}
		DataSnapshot result = DBTask.Result;
		LeaderboardPanel.SetActive(value: true);
		int num = 0;
		foreach (Transform item in Leaderboardcontent)
		{
			Object.Destroy(item.gameObject);
		}
		if (result.Children.Reverse() == null)
		{
			yield break;
		}
		foreach (DataSnapshot item2 in result.Children.Reverse())
		{
			num++;
			if (PlayerPrefs.GetString("isFromMainGame") == "true")
			{
				string playerName = item2.Child("User_Name").Value.ToString();
				int score_ = int.Parse(item2.Child("User_Score").Value.ToString());
				Object.Instantiate(PlayerdataLeaderboard, Leaderboardcontent).GetComponent<LeaderboardElement>().SetData(num, playerName, score_);
			}
			else
			{
				string playerName2 = item2.Child("User_Name").Value.ToString();
				int score_2 = int.Parse(item2.Child("User_Score").Value.ToString());
				Object.Instantiate(PlayerdataLeaderboard, Leaderboardcontent).GetComponent<LeaderboardElement>().SetData(num, playerName2, score_2);
			}
		}
	}

	public void ManageAccountButton(string _userType, TextMeshProUGUI _name, TextMeshProUGUI _age, TextMeshProUGUI _gender, TextMeshProUGUI _username, TextMeshProUGUI _password)
	{
		ManageAccountPanel.SetActive(value: true);
		ManageAccount_InputFields[0].text = _name.text;
		ManageAccount_InputFields[1].text = _age.text;
		ManageAccount_InputFields[2].text = _gender.text;
		ManageAccount_InputFields[3].text = _username.text;
		ManageAccount_InputFields[4].text = _password.text;
		AccountToManage_Name = _name.text;
		AccountToManage_Age = _age.text;
		AccountToManage_Gender = _gender.text;
		AccountToManage_Username = _username.text;
		AccountToManage_Password = _password.text;
		AccountToManage_Usertype = _userType;
		auth.SignOut();
		StartCoroutine(Login_ManageAccount(AccountToManage_Username + "@gmail.com", AccountToManage_Password));
	}

	public void DeleteAccount()
	{
		FirebaseDatabase.DefaultInstance.GetReference(AccountToManage_Usertype).Child(User.UserId).RemoveValueAsync();
		auth.CurrentUser?.DeleteAsync().ContinueWith(delegate(Task task)
		{
			if (task.IsCanceled)
			{
				Debug.LogError("DeleteAsync was canceled.");
			}
			else if (task.IsFaulted)
			{
				Debug.LogError("DeleteAsync encountered an error: " + task.Exception);
			}
		});
		isOnLoadingPanel = true;
		GoBackToMainMenu_ByAdmin();
	}

	public void EditAccount()
	{
		StartCoroutine(DelayRegister_ManageAccount(AccountToManage_Usertype));
	}

	public void Update_Name_ForEdit(string accountNewName)
	{
		AccountToManage_Name = accountNewName;
	}

	public void Update_Age_ForEdit(string accountNewAge)
	{
		AccountToManage_Age = accountNewAge;
	}

	public void Update_Gender_ForEdit(string accountNewGender)
	{
		AccountToManage_Gender = accountNewGender;
	}

	private IEnumerator DelayRegister_ManageAccount(string _type)
	{
		StartCoroutine(UpdateName(AccountToManage_Name, _type));
		StartCoroutine(UpdateUserAge(int.Parse(AccountToManage_Age), _type));
		StartCoroutine(UpdateGender(AccountToManage_Gender, _type));
		StartCoroutine(UpdateuserName(AccountToManage_Username, _type));
		StartCoroutine(UpdateUserPassword(AccountToManage_Password, _type));
		isOnLoadingPanel = true;
		yield return new WaitForSeconds(1f);
		GoBackToMainMenu_ByAdmin();
	}

	public void GoBackToMainMenu_ByAdmin()
	{
		isOnLoadingPanel = false;
		if (ManageAccount_InputFields != null)
		{
			for (int i = 0; i < ManageAccount_InputFields.Length; i++)
			{
				ManageAccount_InputFields[i].text = "";
			}
			ManageAccountPanel.SetActive(value: false);
		}
		RefreshData();
	}
}
