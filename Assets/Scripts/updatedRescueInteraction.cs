using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RescueBoatInteraction : MonoBehaviour
{
    public GameObject rescueDialogUI;
    public Button rescueButton;
    public Button leaveButton;
    public TextMeshProUGUI dialogText;
    public bool isPaused;

    private GameObject currentVictim;
    public Transform passengerSeat; // where rescued victims will be placed on the boat

    void Start()
    {
        rescueDialogUI.SetActive(false);

        rescueButton.onClick.AddListener(() =>
        {
            RescueVictim();
            Time.timeScale = 1.0f;
            isPaused = false;
        });

        leaveButton.onClick.AddListener(() =>
        {
            CloseDialog();
            Time.timeScale = 1.0f;
            isPaused = false;
        });
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Victim"))
        {
            currentVictim = other.gameObject;
            Time.timeScale = 0f; // Pause Game
            isPaused = true;
            ShowRescueDialog();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CloseDialog();
    }

    void ShowRescueDialog()
    {
        dialogText.text = "You found a victim on the roof!\nWhat would you like to do?";
        rescueDialogUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void CloseDialog()
    {
        rescueDialogUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        currentVictim = null;
    }

    void RescueVictim()
    {
        if (currentVictim != null)
        {
            // Parent to boat
            currentVictim.transform.SetParent(passengerSeat);
            currentVictim.transform.localPosition = Vector3.zero;
            currentVictim.transform.localRotation = Quaternion.identity;

            // Disable physics
            Rigidbody rb = currentVictim.GetComponent<Rigidbody>();
            if (rb) rb.isKinematic = true;

            // Make sure it won’t trigger again
            currentVictim.tag = "Untagged";
            Collider col = currentVictim.GetComponent<Collider>();
            if (col) col.enabled = false;

            CloseDialog();
        }
    }
}
