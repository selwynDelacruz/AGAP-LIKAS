// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// AuthManager
using System.Collections;
using System.Collections.Generic;
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

	public TMP_InputField SuperAdmin_Age;

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
		if (UsersListContent == null)
    {
        Debug.LogError("UsersListContent is not assigned - please assign ScrollView Content transform");
    }
    if (playerData == null) 
    {
        Debug.LogError("playerData prefab is not assigned - please assign UserListItem prefab");
    }
		
		
		Time.timeScale = 1f;
    isOnLoadingPanel = false; // Changed from true since we're not auto-logging in
    isOpenRegisterUser = false;
    isManagerCreatingAccounts = false;
    
    // REMOVE these auto-login checks
    // if (PlayerPrefs.GetString("LoginAlready") == "true")
    // {
    //     InvokeRepeating("Delay", 10f, 10f);
    //     if (!string.IsNullOrEmpty(Current_Name))
    //     {
    //         CancelInvoke("Delay");
    //     }
    // }
    // else
    // {
    //     CancelInvoke("Delay");
    // }
    // if (PlayerPrefs.GetString("LoginAlready") != "true" && string.IsNullOrEmpty(Current_Name))
    // {
    //     isOnLoadingPanel = false;
    // }

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
    
    FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
        dependencyStatus = task.Result;
        if (dependencyStatus == DependencyStatus.Available)
        {
            InitializeFirebase();
            Debug.Log("Firebase initialized successfully");
        }
        else
        {
            Debug.LogError($"Could not resolve Firebase dependencies: {dependencyStatus}");
            LoadingPanel.SetActive(true);
        }
    });

    // REMOVE this line
    // Invoke("LoginAutomatic", 0.5f);
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
		Debug.Log($"Username set to: {UsernameNew_ToSet}");
	}

	public void GetPlayerName(string _name)
	{
		User_Name = _name;
		Debug.Log($"Name set to: {User_Name}");
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
		Debug.Log($"Gender set to: {_genderNum}");
	}

	public void GetPlayerAge(string _age)
	{
		// Add validation for age input
    if (string.IsNullOrEmpty(_age))
    {
        Debug.LogWarning("Age cannot be empty");
        User_Age = 0;
        return;
    }

    if (!int.TryParse(_age, out int parsedAge))
    {
        Debug.LogWarning("Invalid age format");
        User_Age = 0;
        return;
    }

    if (parsedAge < minAge)
    {
        Debug.LogWarning($"Age must be at least {minAge}");
        User_Age = 0;
        return;
    }

    User_Age = parsedAge;
    Debug.Log($"Age set to: {User_Age}");
	}

	public void LoginButton()
	{
		switch (PlayerPrefs.GetString("Type_Of_User"))
		{
		case "instructor":
			if (Login_Instructor_Panel.activeSelf)
			{
				Debug.Log(Login_Instructor_Panel.activeSelf);
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
    
    // Set username and password first
    UsernameNew_ToSet = SuperAdmin_emailRegisterField.text;
    User_Password = SuperAdmin_passwordRegisterField.text;
    
    // Then start registration
    StartCoroutine(Register_SuperAdmin(
        SuperAdmin_emailRegisterField.text + "@gmail.com",
        SuperAdmin_passwordRegisterField.text,
        SuperAdmin_emailRegisterField.text,
        User_Gender,
        User_Name,
        User_Age));
	}

	public void CreateAccount_ManageAccount_RegisterButton()
	{
		Debug.Log("Register button clicked");
    	Debug.Log($"Current type to manage: {typeOfUserToManage}");

    // Validate required fields
    if (string.IsNullOrEmpty(User_Name) || string.IsNullOrEmpty(User_Gender))
    {
        CreateAccount_warningRegisterText.text = "Please fill in all required fields!";
        CreateAccount_warningRegisterText.color = Color.red;
        Debug.LogError("Required user data is missing!");
        return;
    }

    isClickedAddNewUser = true;

    switch (typeOfUserToManage)
    {
        case "instructor":
            Debug.Log("Starting instructor registration...");
            StartCoroutine(Register_Instructor(
                CreateAccount_Username.text + "@gmail.com", 
                CreateAccount_Password.text, 
                CreateAccount_Username.text, 
                User_Gender, 
                User_Name, 
                User_Age
            ));
            // Success message will be shown in RegisterSuccessShowPanel
            break;

        case "trainee":
            Debug.Log("Starting trainee registration...");
            StartCoroutine(Register_Trainee(
                CreateAccount_Username.text + "@gmail.com", 
                CreateAccount_Password.text, 
                CreateAccount_Username.text, 
                User_Gender, 
                User_Name, 
                User_Age
            ));
            // Success message will be shown in RegisterSuccessShowPanel
            break;

        default:
            CreateAccount_warningRegisterText.text = "Invalid user type!";
            CreateAccount_warningRegisterText.color = Color.red;
            Debug.LogError($"Invalid user type: {typeOfUserToManage}");
            break;
    }
	}

	private IEnumerator Login(string _email, string _password)
	{
		// First authenticate with Firebase
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
					SetWarning_LoginInfoText(messsage, "yellow");
				}
				break;
			}
			yield break;
		}
		User = LoginTask.Result.User;

		// Get user data to verify user type
		string attemptedUserType = PlayerPrefs.GetString("Type_Of_User");
		Task<DataSnapshot> userTypeTask = DBreference.Child(attemptedUserType)
			.Child(User.UserId)
			.Child("User_Type")
			.GetValueAsync();

		yield return new WaitUntil(() => userTypeTask.IsCompleted);

		if (userTypeTask.Exception != null)
		{
			Debug.LogError("Failed to verify user type");
			SetWarning_LoginInfoText("Login Failed: Invalid credentials", "red");
			auth.SignOut();
			yield break;
		}

		DataSnapshot snapshot = userTypeTask.Result;
		if (!snapshot.Exists || snapshot.Value == null || 
			snapshot.Value.ToString() != attemptedUserType)
		{
			Debug.LogWarning($"User tried to log in as {attemptedUserType} but is actually a different type");
			SetWarning_LoginInfoText("Invalid login type for this account", "red");
			auth.SignOut();
			yield break;
		}

		// Continue with existing login success logic
		Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
		StartCoroutine(LoadUserData(attemptedUserType));
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
					CreateAccount_warningRegisterText.text = $"Successfully created {typeOfUserToManage} account!";
    				Debug.Log($"Successfully created {typeOfUserToManage} account!");
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
		// Check required fields first
	if (string.IsNullOrEmpty(_name) || 
		string.IsNullOrEmpty(_gender) || 
		string.IsNullOrEmpty(_username) || 
		string.IsNullOrEmpty(_password))
	{
		CreateAccount_SetWarning_RegisterInfoText("All fields are required!", "red");
		yield break;
	}

	// Specific check for password length
	if (_password.Length <= 5)
	{
		CreateAccount_SetWarning_RegisterInfoText("Password must be at least 6 characters long!", "yellow");
		yield break;
	}

	// Check age
	if (_age <= 0)
	{
		CreateAccount_SetWarning_RegisterInfoText("Please enter a valid age!", "red");
		yield break;
	}

    // Create auth account
    var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
    yield return new WaitUntil(() => RegisterTask.IsCompleted);

    if (RegisterTask.Exception != null)
    {
        // ... existing error handling ...
        yield break;
    }

    User = RegisterTask.Result.User;
		if (User != null)
		{
			// Set display name
			UserProfile profile = new UserProfile { DisplayName = _username };
			var ProfileTask = User.UpdateUserProfileAsync(profile);
			yield return new WaitUntil(() => ProfileTask.IsCompleted);

			if (ProfileTask.Exception != null)
			{
				// ... existing error handling ...
				yield break;
			}

			// Save all instructor data including username and password
			var DBTask = DBreference.Child("instructor").Child(User.UserId).SetValueAsync(new Dictionary<string, object>
		{
			{ "User_Name", _name },
			{ "User_Gender", _gender },
			{ "User_Username", _username },
			{ "User_Password", _password },
			{ "User_Age", _age },
			{ "User_Type", "instructor"},
			{ "User_Score", 0 }
		});

			yield return new WaitUntil(() => DBTask.IsCompleted);

			if (DBTask.Exception != null)
			{
				Debug.LogWarning($"Failed to register task with {DBTask.Exception}");
				CreateAccount_SetWarning_RegisterInfoText("Failed to save user data!", "red");
				yield break;
			}

			isOnLoadingPanel = true;
			yield return new WaitForSeconds(5f);
			isOnLoadingPanel = false;
			CreateAccount_RegisterNewAccount();
			CreateAccount_warningRegisterText.text = $"Successfully created {typeOfUserToManage} account!";
			CreateAccount_warningRegisterText.color = Color.green;
    }
}

	private IEnumerator Register_Trainee(string _email, string _password, string _username, string _gender, string _name, int _age)
	{
		// Validate input data
		if (string.IsNullOrEmpty(_name) || 
		string.IsNullOrEmpty(_gender) || 
		string.IsNullOrEmpty(_username) || 
		string.IsNullOrEmpty(_password))
	{
		CreateAccount_SetWarning_RegisterInfoText("All fields are required!", "red");
		yield break;
	}

	// Specific check for password length
	if (_password.Length <= 5)
	{
		CreateAccount_SetWarning_RegisterInfoText("Password must be at least 6 characters long!", "yellow");
		yield break;
	}

	// Check age
	if (_age <= 0)
	{
		CreateAccount_SetWarning_RegisterInfoText("Please enter a valid age!", "red");
		yield break;
	}

    // Create auth account
    var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
    yield return new WaitUntil(() => RegisterTask.IsCompleted);

    if (RegisterTask.Exception != null)
    {
        // Handle registration errors
        Debug.LogWarning($"Failed to register task with {RegisterTask.Exception}");
        AuthError errorCode = (AuthError)(RegisterTask.Exception.GetBaseException() as FirebaseException).ErrorCode;
        string message = "Register Failed! Please check your internet connection";
        
        switch (errorCode)
        {
            case AuthError.MissingEmail:
                message = "Missing Username!";
                break;
            case AuthError.MissingPassword:
                message = "Missing Password!";
                break;
            case AuthError.WeakPassword:
                message = "Weak Password";
                break;
            case AuthError.EmailAlreadyInUse:
                message = "Username already in use!";
                break;
        }
        CreateAccount_SetWarning_RegisterInfoText(message, "red");
        ManageAccount_RegisterBTN_UI.interactable = true;
        yield break;
    }

    User = RegisterTask.Result.User;
		if (User != null)
		{
			// Set display name
			UserProfile profile = new UserProfile { DisplayName = _username };
			var ProfileTask = User.UpdateUserProfileAsync(profile);
			yield return new WaitUntil(() => ProfileTask.IsCompleted);

			if (ProfileTask.Exception != null)
			{
				Debug.LogWarning($"Failed to register task with {ProfileTask.Exception}");
				CreateAccount_SetWarning_RegisterInfoText("Username set Failed!", "red");
				ManageAccount_RegisterBTN_UI.interactable = true;
				yield break;
			}

			// Save all trainee data including username and password
			var DBTask = DBreference.Child("trainee").Child(User.UserId).SetValueAsync(new Dictionary<string, object>
		{
			{ "User_Name", _name },
			{ "User_Gender", _gender },
			{ "User_Username", _username },
			{ "User_Password", _password },
			{ "User_Age", _age },
			{ "User_Type", "trainee" },
			{ "User_Score", 0 }
		});

			yield return new WaitUntil(() => DBTask.IsCompleted);

			if (DBTask.Exception != null)
			{
				Debug.LogWarning($"Failed to register task with {DBTask.Exception}");
				CreateAccount_SetWarning_RegisterInfoText("Failed to save user data!", "red");
				yield break;
			}

			isOnLoadingPanel = true;
			yield return new WaitForSeconds(5f);
			isOnLoadingPanel = false;
			CreateAccount_RegisterNewAccount();
			CreateAccount_warningRegisterText.text = $"Successfully created {typeOfUserToManage} account!";
			CreateAccount_warningRegisterText.color = Color.green;
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
		CreateAccount_warningRegisterText.text = _messsage;
		switch (color)
		{
		case "red":
			CreateAccount_warningRegisterText.color = Color.red;
			break;
		case "yellow":
			CreateAccount_warningRegisterText.color = Color.yellow;
			break;
		case "white":
			CreateAccount_warningRegisterText.color = Color.white;
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
		try
    {
        // Check if UI elements are assigned
        if (CreateAccount_warningRegisterText == null)
        {
            Debug.LogError("CreateAccount_warningRegisterText is not assigned!");
            return;
        }
        if (CreateAccount_Username == null)
        {
            Debug.LogError("CreateAccount_Username is not assigned!");
            return;
        }
        if (CreateAccount_Password == null)
        {
            Debug.LogError("CreateAccount_Password is not assigned!");
            return;
        }
        if (CreateAccount_Name == null)
        {
            Debug.LogError("CreateAccount_Name is not assigned!");
            return;
        }
        if (CreateAccount_Age == null)
        {
            Debug.LogError("CreateAccount_Age is not assigned!");
            return;
        }
        if (CreateAccount_Gender == null)
        {
            Debug.LogError("CreateAccount_Gender is not assigned!");
            return;
        }
        if (ManageAccount_RegisterBTN_UI == null)
        {
            Debug.LogError("ManageAccount_RegisterBTN_UI is not assigned!");
            return;
        }

        // Clear all fields
        CreateAccount_warningRegisterText.text = "";
        CreateAccount_Username.text = "";
        CreateAccount_Password.text = "";
        CreateAccount_Name.text = "";
        CreateAccount_Age.text = "";
        CreateAccount_Gender.SetValueWithoutNotify(0);
        ManageAccount_RegisterBTN_UI.interactable = true;

        // Reset user data
        User_Name = "";
        User_Gender = "";
        UsernameNew_ToSet = "";
        User_Age = 0;
        User_Password = "";
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"Error in CreateAccount_RegisterNewAccount: {ex.Message}");
    }
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
		// SuperAdmin_Age.SetValueWithoutNotify(0);
		SuperAdmin_Age.text = "0";
		User_Age = 0;
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
				Debug.Log("UserType: " + UserType);
				break;
			case "trainee":
				Login_Trainee_Panel.SetActive(value: true);
				ChooseTypeOfUserPanel.SetActive(value: false);
				PlayerPrefs.SetString("Type_Of_User", UserType);
				Debug.Log("trainee clicked");
				Debug.Log("UserType: " + UserType);
				PlayerPrefs.Save();
				break;
			case "super_admin":
				Login_SuperAdmin_Panel.SetActive(value: true);
				ChooseTypeOfUserPanel.SetActive(value: false);
				PlayerPrefs.SetString("Type_Of_User", UserType);
				PlayerPrefs.Save();
				Debug.Log("superadmin clicked");
				Debug.Log("UserType: " + UserType);
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
				// Clear UI fields
				ClearAllLoginField(Instructor_emailLoginField, Instructor_passwordLoginField, Instructor_confirmLoginText);
				Login_Instructor_Panel.SetActive(false);
				MenuPanel_Instructor.SetActive(false);
				ChooseTypeOfUserPanel.SetActive(true);
				
				// Handle logout
				if (auth != null && auth.CurrentUser != null)
				{
					auth.SignOut();
					Debug.Log("Instructor signed out");
				}
				
				// Clear preferences and cached data
				PlayerPrefs.DeleteAll();
				PlayerPrefs.Save();
				
				// Reset UI state
				Instructor_confirmLoginText.text = "";
				Login_InstructorButton.interactable = true;
				
				// Clear cached user data
				Current_Name = "";
				Current_Username = "";
				Current_Gender = "";
				Current_Age = 0;
				Current_Score = 0;
				break;

			case "trainee":
				// Clear UI fields
				ClearAllLoginField(Trainee_emailLoginField, Trainee_passwordLoginField, Trainee_confirmLoginText);
				Login_Trainee_Panel.SetActive(false);
				MenuPanel_Trainee.SetActive(false);
				ChooseTypeOfUserPanel.SetActive(true);
				
				// Handle logout
				if (auth != null && auth.CurrentUser != null)
				{
					auth.SignOut();
					Debug.Log("Trainee signed out");
				}
				
				// Clear preferences and cached data
				PlayerPrefs.DeleteAll();
				PlayerPrefs.Save();
				
				// Reset UI state
				Trainee_confirmLoginText.text = "";
				Login_TraineeButton.interactable = true;
				
				// Clear cached user data
				Current_Name = "";
				Current_Username = "";
				Current_Gender = "";
				Current_Age = 0;
				Current_Score = 0;
				break;

			case "super_admin":
				// Clear UI fields
				ClearAllLoginField(SuperAdmin_emailLoginField, SuperAdmin_passwordLoginField, SuperAdmin_confirmLoginText);
				MenuPanel_SuperAdmin.SetActive(false);
				ChooseTypeOfUserPanel.SetActive(true);
				
				// Handle logout
				if (auth != null && auth.CurrentUser != null)
				{
					auth.SignOut();
					Debug.Log("SuperAdmin signed out");
				}
				
				// Clear preferences and cached data
				PlayerPrefs.DeleteAll();
				PlayerPrefs.Save();
				
				// Reset UI state
				SuperAdmin_confirmLoginText.text = "";
				Login_SuperAdminButton.interactable = true;
				
				// Clear cached user data
				Current_Name = "";
				Current_Username = "";
				Current_Gender = "";
				Current_Age = 0;
				Current_Score = 0;
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
		SceneManager.LoadScene("Main Menu");
	}

	private IEnumerator RegisterSuccessShowPanel()
	{
		 // Show success message in CreateAccount_warningRegisterText
   		CreateAccount_warningRegisterText.text = $"Successfully created {typeOfUserToManage} account!";
    	
		Debug.Log("Successfully created account!");

    	// Show the success panel
		SuccessfullyCreatedAccount.SetActive(true);
    
    	yield return new WaitForSeconds(1f);
    
    	// Hide the success panel and clear the text
    	SuccessfullyCreatedAccount.SetActive(false);
    	CreateAccount_warningRegisterText.text = "";
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
			AdminNameText.text = Current_Name;
			
			// Store username in PlayerPrefs so it persists across scenes
			PlayerPrefs.SetString("Current_Username", Current_Username);
			PlayerPrefs.Save();
			Debug.Log($"[AuthManager] Saved username to PlayerPrefs: {Current_Username}");
			
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
    Debug.Log("Starting LoadInstructorList");

    // Validate references
    if (DBreference == null || UsersListContent == null || playerData == null)
    {
        Debug.LogError($"Missing references - DBreference: {DBreference != null}, UsersListContent: {UsersListContent != null}, playerData: {playerData != null}");
        yield break;
    }

    // Get instructor data with detailed logging
    Debug.Log("Fetching instructor data from Firebase...");
    Task<DataSnapshot> DBTask = DBreference.Child("instructor").OrderByChild("User_Username").GetValueAsync();
    yield return new WaitUntil(() => DBTask.IsCompleted);

    if (DBTask.Exception != null)
    {
        Debug.LogError($"Firebase query failed: {DBTask.Exception}");
        yield break;
    }

    DataSnapshot result = DBTask.Result;
    Debug.Log($"Query complete - Has data: {result.Exists}, Child count: {result.ChildrenCount}");

    // Clear existing items
    foreach (Transform item in UsersListContent)
    {
        if (item != null)
            Destroy(item.gameObject);
    }

    if (result == null || !result.HasChildren)
    {
        Debug.Log("No instructor data found in database");
        yield break;
    }

    try
    {
        Debug.Log("Starting to process instructor data...");
        foreach (DataSnapshot item in result.Children.Reverse())
        {
            if (item == null) continue;

            // Log each instructor's data
            Debug.Log($"Processing instructor: {item.Key}");
            var name = item.Child("User_Name").Value?.ToString();
            var ageStr = item.Child("User_Age").Value?.ToString();
            var gender = item.Child("User_Gender").Value?.ToString();
            var username = item.Child("User_Username").Value?.ToString();
            var password = item.Child("User_Password").Value?.ToString();
            var usertype = item.Child("User_Type").Value?.ToString();

            Debug.Log($"Instructor data - Name: {name}, Age: {ageStr}, Gender: {gender}, Username: {username}");

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(ageStr) || 
                string.IsNullOrEmpty(password))
            {
                Debug.LogWarning("Skipping instructor due to missing required data");
                continue;
            }

            // Create list item
            int age = int.Parse(ageStr);
            var newItem = Instantiate(playerData, UsersListContent);
            
            if (newItem != null)
            {
                var element = newItem.GetComponent<UsersElement>();
					if (element != null)
					{
						element.ListData(usertype, name, age, gender, username, password);
						Debug.Log($"Successfully created list item for {name}");
						Debug.Log($"Successfully created list item for {username}");
                }
					else
					{
						Debug.LogError("UsersElement component missing on prefab");
					}
            }
        }
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"Error processing instructor data: {ex.Message}\n{ex.StackTrace}");
    }
}

	public void GetSearchBarUsers(string data)
{
    // Add null checks and debug logging
    if (SelectedUserTypeToShow == null)
    {
        Debug.LogError("SelectedUserTypeToShow dropdown is not assigned in Inspector!");
        return;
    }

    if (SearchBarInputField == null)
    {
        Debug.LogError("SearchBarInputField is not assigned in Inspector!");
        return;
    }

    try 
    {
        if (SelectedUserTypeToShow.value == 0)
        {
            StartCoroutine(LoadInstructorList_SearchBar(data));
            Debug.Log($"Searching instructors for: {data}");
        }
        else
        {
            StartCoroutine(LoadTraineeList_SearchBar(data));
            Debug.Log($"Searching trainees for: {data}");
        }
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"Error in GetSearchBarUsers: {ex.Message}");
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
    Debug.Log("Starting LoadTraineeList");

    // Validate references
    if (DBreference == null || UsersListContent == null || playerData == null)
    {
        Debug.LogError($"Missing references - DBreference: {DBreference != null}, UsersListContent: {UsersListContent != null}, playerData: {playerData != null}");
        yield break;
    }

    // Get trainee data with detailed logging
    Debug.Log("Fetching trainee data from Firebase...");
    Task<DataSnapshot> DBTask = DBreference.Child("trainee").OrderByChild("User_Name").GetValueAsync();
    yield return new WaitUntil(() => DBTask.IsCompleted);

    if (DBTask.Exception != null)
    {
        Debug.LogError($"Firebase query failed: {DBTask.Exception}");
        yield break;
    }

    DataSnapshot result = DBTask.Result;
    Debug.Log($"Query complete - Has data: {result.Exists}, Child count: {result.ChildrenCount}");

    // Clear existing items
    foreach (Transform item in UsersListContent)
    {
        if (item != null)
            Destroy(item.gameObject);
    }

    if (result == null || !result.HasChildren)
    {
        Debug.Log("No trainee data found in database");
        yield break;
    }

    try
    {
        Debug.Log("Starting to process trainee data...");
        foreach (DataSnapshot trainee in result.Children.Reverse())
        {
            if (trainee == null) continue;

            // Safely get values with null checks
            var nameSnapshot = trainee.Child("User_Name").Value;
            var ageSnapshot = trainee.Child("User_Age").Value;
            var genderSnapshot = trainee.Child("User_Gender").Value;
            var usernameSnapshot = trainee.Child("User_Username").Value;
            var passwordSnapshot = trainee.Child("User_Password").Value;
            var typeSnapshot = trainee.Child("User_Type").Value;

            // Check if any required data is missing
            if (nameSnapshot == null || ageSnapshot == null || 
                genderSnapshot == null || usernameSnapshot == null || 
                passwordSnapshot == null || typeSnapshot == null)
            {
                Debug.LogWarning($"Skipping trainee {trainee.Key} - missing required data");
                continue;
            }

            string name = nameSnapshot.ToString();
            string ageStr = ageSnapshot.ToString();
            string gender = genderSnapshot.ToString();
            string username = usernameSnapshot.ToString();
            string password = passwordSnapshot.ToString();
            string usertype = typeSnapshot.ToString();

            Debug.Log($"Processing trainee: {name}, Username: {username}");

            if (!string.IsNullOrEmpty(name) && int.TryParse(ageStr, out int age))
            {
                var newItem = Instantiate(playerData, UsersListContent);
                if (newItem != null)
                {
                    var element = newItem.GetComponent<UsersElement>();
                    if (element != null)
                    {
                        element.ListData(usertype, name, age, gender, username, password);
                        Debug.Log($"Successfully created list item for {name}");
                    }
                    else
                    {
                        Debug.LogError("UsersElement component missing on prefab");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Invalid data for trainee {trainee.Key}");
            }
        }
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"Error processing trainee data: {ex.Message}\n{ex.StackTrace}");
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
		Debug.Log("manageaccountpanel: active");
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

		}
		ManageAccountPanel.SetActive(false);
    	MenuPanel_SuperAdmin.SetActive(true);
		//RefreshData();
	}
}
