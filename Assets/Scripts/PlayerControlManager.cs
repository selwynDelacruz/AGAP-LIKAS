using UnityEngine;
using StarterAssets;

/// <summary>
/// Manages player controls based on userType - disables controls for instructors
/// </summary>
public class PlayerControlManager : MonoBehaviour
{
    private string userType;
    private bool isInstructor = false;

    void Start()
    {
        // Get user type from PlayerPrefs
        userType = PlayerPrefs.GetString("Type_Of_User", "");
        isInstructor = (userType == "instructor");

        Debug.Log($"[PlayerControlManager] User Type: {userType}, Is Instructor: {isInstructor}");

        if (isInstructor)
        {
            DisablePlayerControls();
        }
    }

    private void DisablePlayerControls()
    {
        Debug.Log("[PlayerControlManager] Disabling player controls for instructor");

        // Find and disable ThirdPersonController (Earthquake mode - using Agap controller)
        StarterAssets.AgapThirdPersonController thirdPersonController = Object.FindAnyObjectByType<StarterAssets.AgapThirdPersonController>();
        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = false;
            Debug.Log("[PlayerControlManager] Agap ThirdPersonController disabled");
        }

        // Find and disable ALL PlayerInteract components (both players have this)
        PlayerInteract[] playerInteracts = Object.FindObjectsByType<PlayerInteract>(FindObjectsSortMode.None);
        foreach (PlayerInteract playerInteract in playerInteracts)
        {
            playerInteract.enabled = false;
            Debug.Log($"[PlayerControlManager] PlayerInteract disabled on {playerInteract.gameObject.name}");
        }

        // Find and disable ALL PlayerInputs (Script) components (both players have this)
        PlayerInputControl.PlayerInputs[] playerInputsScripts = Object.FindObjectsByType<PlayerInputControl.PlayerInputs>(FindObjectsSortMode.None);
        foreach (PlayerInputControl.PlayerInputs playerInputScript in playerInputsScripts)
        {
            playerInputScript.enabled = false;
            Debug.Log($"[PlayerControlManager] PlayerInputs (Script) disabled on {playerInputScript.gameObject.name}");
        }

        // Find and disable BoatController (Flood mode)
        BoatController boatController = Object.FindAnyObjectByType<BoatController>();
        if (boatController != null)
        {
            boatController.enabled = false;
            Debug.Log("[PlayerControlManager] BoatController disabled");
        }

        // Disable ALL PlayerInput components (Unity Input System)
        #if ENABLE_INPUT_SYSTEM
        UnityEngine.InputSystem.PlayerInput[] playerInputs = Object.FindObjectsByType<UnityEngine.InputSystem.PlayerInput>(FindObjectsSortMode.None);
        foreach (UnityEngine.InputSystem.PlayerInput playerInput in playerInputs)
        {
            playerInput.enabled = false;
            Debug.Log($"[PlayerControlManager] PlayerInput disabled on {playerInput.gameObject.name}");
        }
        #endif

        // Disable CharacterController on walking player (but not Rigidbody on boat)
        CharacterController[] characterControllers = Object.FindObjectsByType<CharacterController>(FindObjectsSortMode.None);
        foreach (CharacterController characterController in characterControllers)
        {
            characterController.enabled = false;
            Debug.Log($"[PlayerControlManager] CharacterController disabled on {characterController.gameObject.name}");
        }

        Debug.Log("[PlayerControlManager] All player controls disabled for spectator mode");
    }

    /// <summary>
    /// Check if current user is instructor
    /// </summary>
    public bool IsInstructor()
    {
        return isInstructor;
    }

    /// <summary>
    /// Get current user type
    /// </summary>
    public string GetUserType()
    {
        return userType;
    }
}
