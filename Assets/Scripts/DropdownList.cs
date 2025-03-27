using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DropdownList : MonoBehaviour
{
    [SerializeField] private TMP_Text modeText;
    [SerializeField] private Image disasterImage;
    [SerializeField] private Sprite floodSprite, earthquakeSprite;
    [SerializeField] private TMP_Dropdown disasterDropdown;

    public static string SelectedDisaster { get; private set; } // Stores the selected value

    void Start()
    {
        // Initialize selection
        UpdateDisaster(disasterDropdown.value);
        disasterDropdown.onValueChanged.AddListener(UpdateDisaster);
    }

    public void UpdateDisaster(int index)
    {
        switch (index)
        {
            case 0:
                modeText.text = "Flood";
                disasterImage.sprite = floodSprite;
                SelectedDisaster = "Flood"; // Set scene name
                break;
            case 1:
                modeText.text = "Earthquake";
                disasterImage.sprite = earthquakeSprite;
                SelectedDisaster = "Earthquake"; // Set scene name
                break;
            default:
                modeText.text = "Unknown";
                disasterImage.sprite = null;
                SelectedDisaster = "";
                break;
        }
    }

}
