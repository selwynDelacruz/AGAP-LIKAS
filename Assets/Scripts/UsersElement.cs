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
        if (usernameText != null) usernameText.text = MaskUsername(username);
        if (passwordText != null) passwordText.text = MaskPassword(password);
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
		AuthManager.Instance.ManageAccountButton(userTypeText, nameText, ageText, genderText, usernameText, passwordText);
	}
    
    // Helper method to mask passwords
    private string MaskPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return "";
            
        // Show only first and last characters, mask the rest with asterisks
        if (password.Length <= 2)
            return new string('*', password.Length);
            
        return password[0] + new string('*', password.Length - 2) + password[password.Length - 1];
    }
    
    private string MaskUsername(string username)
    {
        if (string.IsNullOrEmpty(username))
            return "";
            
        // Show only first and last characters, mask the rest with asterisks
        if (username.Length <= 2)
            return new string('*', username.Length);
            
        return username[0] + new string('*', username.Length - 5) + username[username.Length - 1];
    }

}
