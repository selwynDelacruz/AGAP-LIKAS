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

    // Store original unmasked values for authentication
    private string originalUsername;
    private string originalPassword;

    public void ListData(string userType, string name, int age, string gender, string username, string password)
    {
        // Store original values for authentication
        originalUsername = username;
        originalPassword = password;
        userTypeText = userType;

        // Display values in UI (masked for security)
        if (nameText != null) nameText.text = name;
        if (ageText != null) ageText.text = age.ToString();
        if (genderText != null) genderText.text = gender;
        if (usernameText != null) usernameText.text = MaskUsername(username);
        if (passwordText != null) passwordText.text = MaskPassword(password);

        Debug.Log($"[UsersElement] Created item for: {username}");
    }

    public void ManageAccount()
    {
        // Pass original unmasked credentials to AuthManager
        AuthManager.Instance.ManageAccountButton(
            userTypeText, 
            nameText.text,           // name (not masked)
            ageText.text,            // age (not masked)
            genderText.text,         // gender (not masked)
            originalUsername,        // ORIGINAL username
            originalPassword         // ORIGINAL password
        );
    }
    
    // Helper method to mask passwords for display
    private string MaskPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return "";
            
        if (password.Length <= 2)
            return new string('*', password.Length);
            
        return password[0] + new string('*', password.Length - 2) + password[password.Length - 1];
    }
    
    // Helper method to mask usernames for display
    private string MaskUsername(string username)
    {
        if (string.IsNullOrEmpty(username))
            return "";
            
        if (username.Length <= 2)
            return new string('*', username.Length);
            
        return username[0] + new string('*', username.Length - 2) + username[username.Length - 1];
    }
}
