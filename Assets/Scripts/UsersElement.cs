using TMPro;
using UnityEngine;

public class UsersElement : MonoBehaviour
{
	public TextMeshProUGUI user_Name;

	public TextMeshProUGUI user_Age;

	public TextMeshProUGUI user_Gender;

	public TextMeshProUGUI user_Username;

	public TextMeshProUGUI user_Password;

	private string userType_;

	public void ListData(string usertype, string _name, int _age, string _gender, string _username, string _password)
	{
		user_Name.text = _name;
		user_Age.text = _age.ToString();
		user_Gender.text = _gender;
		user_Username.text = _username;
		user_Password.text = _password;
		userType_ = usertype;
	}

	public void ManageAccount()
	{
		AuthManager.Instance.ManageAccountButton(userType_, user_Name, user_Age, user_Gender, user_Username, user_Password);
	}
}
