using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class RescueInteraction : MonoBehaviour
{

    public float interactionRange = 5f;
    public LayerMask victimLayer;
    private GameObject currentVictim;

    public GameObject rescueDialogUI;
    public Button rescueButton;
    public Button leaveButton;
    public TextMeshProUGUI dialogText;

    public Transform passengerSeat; // assign in Inspector
    private GameObject rescuedVictim;
    void Start()
    {
        rescueDialogUI.SetActive(false);

        rescueButton.onClick.AddListener(() =>
        {
            RescueVictim();
            rescueDialogUI.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            currentVictim = null;

        });

        leaveButton.onClick.AddListener(() =>
        {
            rescueDialogUI.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            currentVictim = null;
            
        });
    }

    // Update is called once per frame
    void Update()
    {
        CheckForVictim();

        if (currentVictim != null && !rescueDialogUI.activeSelf)
        {
            float distance = Vector3.Distance(transform.position, currentVictim.transform.position);
            if (distance <= interactionRange)
            {
                ShowRescueDialog(currentVictim);
            }
        }
    }

    void CheckForVictim()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange, victimLayer);
        currentVictim = hits.Length > 0 ? hits[0].gameObject : null;
        
    }
    void ShowRescueDialog(GameObject victim)
    {
        rescueDialogUI.SetActive(true);
        dialogText.text = "You found a victim on the roof! \n What would you like to do?";

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void RescueVictim()
    {
        if(currentVictim != null)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            // Parent the victim to the boat (or this object)
            currentVictim.transform.SetParent(passengerSeat);
            currentVictim.transform.localPosition = Vector3.zero;
            currentVictim.transform.localRotation = Quaternion.identity;

            // Optional: Disable movement or physics
            Rigidbody rb = currentVictim.GetComponent<Rigidbody>();
            if (rb) rb.isKinematic = true;

            // Optional: Disable victim interaction script
            currentVictim.tag = "Untagged";
            currentVictim = null;

            rescueDialogUI.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            currentVictim = null; // Reset
        }
    }
}
