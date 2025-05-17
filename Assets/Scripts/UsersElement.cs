using UnityEngine;
using TMPro;

public class UsersElement : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI ageText;
    public TextMeshProUGUI genderText;
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI passwordText;
    public TextMeshProUGUI userTypeText;

    public void ListData(string userType, string name, int age, string gender, string username, string password)
    {
        if (nameText != null) nameText.text = name;
        if (ageText != null) ageText.text = age.ToString();
        if (genderText != null) genderText.text = gender;
        if (usernameText != null) usernameText.text = username;
        if (passwordText != null) passwordText.text = password;
        if (userTypeText != null) userTypeText.text = userType;
    }
}
