using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RescueBoatInteraction : MonoBehaviour
{
    public GameObject rescueDialogUI; // UI for rescue dialog
    public Button rescueButton; // Button to rescue the victim
    public Button leaveButton; // Button to leave the victim
    public TextMeshProUGUI dialogText; // Text for the dialog UI
    public bool isPaused; // Tracks if the game is paused
    public int maxPassengers = 2; // Maximum number of passengers allowed

    private GameObject currentVictim; // Reference to the victim being interacted with
    public Transform passengerSeat1; // First passenger seat for the first victim
    public Transform passengerSeat2; // Second passenger seat for the second victim
    private int passengerCount = 0; // Tracks the number of rescued passengers

    void Start()
    {
        // Ensure the dialog UI is hidden at the start
        if (rescueDialogUI != null)
        {
            rescueDialogUI.SetActive(false);
        }
        else
        {
            Debug.LogError("Rescue Dialog UI is not assigned!");
        }

        // Set up button listeners
        if (rescueButton != null)
        {
            rescueButton.onClick.AddListener(() =>
            {
                RescueVictim();
                Time.timeScale = 1.0f;
                isPaused = false;
            });
        }
        else
        {
            Debug.LogError("Rescue Button is not assigned!");
        }

        if (leaveButton != null)
        {
            leaveButton.onClick.AddListener(() =>
            {
                CloseDialog();
                Time.timeScale = 1.0f;
                isPaused = false;
            });
        }
        else
        {
            Debug.LogError("Leave Button is not assigned!");
        }

        // Ensure passenger seats are assigned and are children of the boat
        if (passengerSeat1 == null)
        {
            Debug.LogError("Passenger Seat 1 is not assigned!");
        }
        else if (passengerSeat1.parent == null || !passengerSeat1.IsChildOf(transform))
        {
            Debug.LogWarning("Passenger Seat 1 is not a child of the boat! This may cause the victim to not move with the boat.");
        }

        if (passengerSeat2 == null)
        {
            Debug.LogError("Passenger Seat 2 is not assigned!");
        }
        else if (passengerSeat2.parent == null || !passengerSeat2.IsChildOf(transform))
        {
            Debug.LogWarning("Passenger Seat 2 is not a child of the boat! This may cause the victim to not move with the boat.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the collided object is a victim
        if (other.CompareTag("Victim"))
        {
            currentVictim = other.gameObject;
            Time.timeScale = 0f; // Pause the game
            isPaused = true;
            ShowRescueDialog();
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Close dialog when the boat leaves the victim's trigger area
        if (other.CompareTag("Victim") && currentVictim == other.gameObject)
        {
            CloseDialog();
        }
    }

    void ShowRescueDialog()
    {
        // Display the rescue dialog with options
        if (dialogText != null)
        {
            dialogText.text = passengerCount >= maxPassengers
                ? "The boat is full! You should put the victims to a safe place first."
                : "You found a victim on the roof!\nWhat would you like to do?";
        }
        rescueDialogUI.SetActive(true);

        // Unlock cursor for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void CloseDialog()
    {
        // Hide the dialog and reset cursor
        rescueDialogUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1.0f; // Resume game
        isPaused = false;
        currentVictim = null; // Clear the reference
    }

    void RescueVictim()
    {
        if (currentVictim != null)
        {
            if (passengerCount < maxPassengers)
            {
                // Determine which seat to use based on passenger count
                Transform targetSeat = passengerCount == 0 ? passengerSeat1 : passengerSeat2;

                // Parent the victim to the target seat so it moves with the boat
                currentVictim.transform.SetParent(targetSeat, true); // 'true' keeps world position initially
                currentVictim.transform.localPosition = Vector3.zero; // Snap to the seat's local position
                currentVictim.transform.localRotation = Quaternion.identity; // Reset rotation relative to the seat

                // Debug the parenting hierarchy
                Debug.Log($"Victim parented to: {currentVictim.transform.parent.name}, Seat parent: {targetSeat.parent.name}");

                // Disable physics to prevent the victim from falling off or moving independently
                Rigidbody rb = currentVictim.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false; // Explicitly disable gravity
                }

                // Play the sitting animation and disable root motion to ensure the victim stays with the boat
                Animator anim = currentVictim.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.enabled = true; // Re-enable animator to play the sit animation
                    anim.applyRootMotion = false; // Disable root motion to prevent animation from moving the victim

                    // Check if "victimOldman sit" state exists, otherwise try "victim sit"
                    AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
                    if (anim.HasState(0, Animator.StringToHash("victimOldman sit")))
                    {
                        anim.Play("victimOldman sit");
                    }
                    else if (anim.HasState(0, Animator.StringToHash("victim sit")))
                    {
                        anim.Play("victim sit");
                    }
                    else
                    {
                        Debug.LogWarning("Neither 'victimOldman sit' nor 'victim sit' animation state found in Animator!");
                    }
                }
                else
                {
                    Debug.LogWarning("Victim does not have an Animator component!");
                }

                // Prevent further triggers and interactions
                currentVictim.tag = "Untagged";
                Collider col = currentVictim.GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = false;
                }

                passengerCount++; // Increment passenger count
                CloseDialog();
            }
            else
            {
                // Show the full boat message again if rescue is attempted
                ShowRescueDialog();
            }
        }
        else
        {
            Debug.LogWarning("Cannot rescue victim: Victim is null.");
        }
    }
}