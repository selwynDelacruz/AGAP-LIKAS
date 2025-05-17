using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UsersElement : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI ageText;
    public TextMeshProUGUI genderText;
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI passwordText;
    public string userTypeText;
     public Button manageButton;

    public void ListData(string userType, string name, int age, string gender, string username, string password)
    {
        if (nameText != null) nameText.text = name;
        if (ageText != null) ageText.text = age.ToString();
        if (genderText != null) genderText.text = gender;
        if (usernameText != null) usernameText.text = username;
        if (passwordText != null) passwordText.text = password;
        userTypeText = userType;

        // Debugging logs to check if the text components are assigned correctly
        if(usernameText != null)
        {
            Debug.Log("usernameText: " + usernameText.text);
        }
        else
        {
            Debug.Log("usernameText is null");
        }
    }

    public void ManageAccount()
	{
		AuthManager.Instance.ManageAccountButton(userTypeText, usernameText, ageText, genderText, usernameText, passwordText);
	}

    
}
