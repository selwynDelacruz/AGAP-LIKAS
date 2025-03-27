using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Menu : MonoBehaviour
{
    [SerializeField] private TMP_Text modeText;
    [SerializeField] private Image disasterImage;
    [SerializeField] private Sprite floodSprite, earthquakeSprite;
    [SerializeField] private TMP_Dropdown disasterDropdown;

    public static string SelectedDisaster { get; private set; } // Stores the selected value

    void Start()
    {
        //// Initialize selection
        //UpdateDisaster(disasterDropdown.value);
        //disasterDropdown.onValueChanged.AddListener(UpdateDisaster);
    }

    public void UpdateDisaster(int index)
    {
        switch (index)
        {
            case 0:
                modeText.text = "Flood";
                disasterImage.sprite = floodSprite;
                SelectedDisaster = "FloodScene"; // Set scene name
                break;
            case 1:
                modeText.text = "Earthquake";
                disasterImage.sprite = earthquakeSprite;
                SelectedDisaster = "EarthquakeScene"; // Set scene name
                break;
            default:
                modeText.text = "Unknown";
                disasterImage.sprite = null;
                SelectedDisaster = "";
                break;
        }
    }
}
