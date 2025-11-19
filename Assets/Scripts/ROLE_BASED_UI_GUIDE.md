# Role-Based UI Access Control Guide

## Overview
The GameManager now includes comprehensive role-based UI access control that ensures:
- **Instructors (Hosts)** see only the Instructor UI
- **Trainees (Clients)** see only the Trainee UI

This system integrates with both the existing Firebase authentication and Unity Netcode for GameObjects networking system.

---

## How It Works

### 1. Role Detection
The system automatically detects the user's role using two sources:
- **PlayerPrefs**: `"Type_Of_User"` (set during Firebase authentication)
- **NetworkManager**: Checks if the player is Host or Client

### 2. UI Assignment Logic
```
Instructor UI is shown when:
- User logged in as "instructor" AND
- User is the network Host

Trainee UI is shown when:
- User logged in as "trainee" OR
- User is a network Client (not host)
```

---

## Setup Instructions

### Step 1: Assign UI Panels in Inspector

1. Open your game scene in Unity
2. Select the **GameManager** GameObject
3. In the Inspector, locate the **"Role-Based UI Panels"** section
4. Assign the following:
   - **Instructor UI Panel**: Drag the Canvas/Panel that contains instructor-specific UI
   - **Trainee UI Panel**: Drag the Canvas/Panel that contains trainee-specific UI

### Step 2: Organize Your UI Hierarchy

**Recommended Structure:**
```
Canvas
??? InstructorUI (Panel)
?   ??? InstructorControls
?   ??? MonitoringPanel
?   ??? InstructorTimer
??? TraineeUI (Panel)
    ??? TraineeHUD
    ??? ObjectivePanel
    ??? TraineeTimer
```

### Step 3: Test in Editor

Enable **Debug Role UI** in the GameManager inspector to see detailed logs about role detection and UI activation.

---

## Configuration Options

### In GameManager Inspector:

| Field | Description | Default |
|-------|-------------|---------|
| `Instructor UI Panel` | GameObject containing instructor-specific UI | null |
| `Trainee UI Panel` | GameObject containing trainee-specific UI | null |
| `Debug Role UI` | Enable detailed logging for troubleshooting | true |

---

## Code Usage Examples

### Check Current User Role
```csharp
if (GameManager.Instance.IsInstructor())
{
    // Instructor-only code
    Debug.Log("This is the instructor!");
}

if (GameManager.Instance.IsTrainee())
{
    // Trainee-only code
    Debug.Log("This is a trainee!");
}
```

### Get Role String
```csharp
string role = GameManager.Instance.GetCurrentUserRole();
Debug.Log($"Current role: {role}");
```

### Manually Toggle UI (Advanced)
```csharp
// Show instructor UI only
GameManager.Instance.ToggleRoleUI(showInstructor: true, showTrainee: false);

// Show trainee UI only
GameManager.Instance.ToggleRoleUI(showInstructor: false, showTrainee: true);
```

---

## Workflow Integration

### Complete User Flow:

1. **User Login** (MainMenu Scene)
   ```
   User selects role ? Firebase authentication ? PlayerPrefs sets "Type_Of_User"
   ```

2. **Network Connection**
   ```
   Instructor clicks "Host" ? Becomes network host
   Trainee clicks "Join" ? Becomes network client
   ```

3. **Game Scene Load**
   ```
   GameManager.Start() is called
   ?
   InitializeRoleDetection() checks:
   - PlayerPrefs.GetString("Type_Of_User")
   - NetworkManager.Singleton.IsHost
   ?
   InitializeRoleBasedUI() shows/hides panels
   ```

---

## Testing Scenarios

### Scenario 1: Single Player Testing
If NetworkManager is not active:
- System falls back to PlayerPrefs role only
- Instructor role ? treated as host
- Trainee role ? treated as client

### Scenario 2: Multiplayer Testing
**Instructor (Host):**
1. Login as instructor
2. Click "Host" in NetworkUI
3. Load game scene
4. ? Only Instructor UI visible

**Trainee (Client):**
1. Login as trainee
2. Click "Join" in NetworkUI
3. Load game scene
4. ? Only Trainee UI visible

---

## Troubleshooting

### Problem: Both UIs are visible
**Possible Causes:**
- UI Panels not assigned in GameManager
- NetworkManager not initialized before GameManager
- User role not set in PlayerPrefs

**Solution:**
1. Check GameManager Inspector assignments
2. Enable "Debug Role UI" in inspector
3. Check Console logs for role detection messages
4. Verify login flow sets PlayerPrefs correctly

### Problem: No UI is visible
**Possible Causes:**
- UI Panels disabled in scene by default
- Role detection failed

**Solution:**
1. Ensure UI Panel GameObjects exist in scene
2. Check Console for error messages
3. Verify PlayerPrefs contains "Type_Of_User" key
4. Make sure Canvas has correct render mode

### Problem: Wrong UI showing
**Possible Causes:**
- Role mismatch (trainee as host or instructor as client)
- PlayerPrefs not cleared from previous session

**Solution:**
1. Clear PlayerPrefs before testing: `PlayerPrefs.DeleteAll()`
2. Ensure instructor logs in AND hosts
3. Ensure trainee logs in AND joins as client

---

## Debug Logs

When `debugRoleUI = true`, you'll see logs like:
```
[GameManager] Role Detection - UserType: instructor, IsHost: True, IsClient: False
[GameManager] Instructor UI Panel: ENABLED
[GameManager] Trainee UI Panel: DISABLED
```

---

## Best Practices

### 1. Separate UI Concerns
Keep instructor and trainee UI elements in completely separate panel hierarchies to avoid conflicts.

### 2. Use Role Checks for Functionality
Not just for UI visibility - use role checks to enable/disable features:
```csharp
void Update()
{
    if (GameManager.Instance.IsInstructor())
    {
        // Instructor can spawn victims
        if (Input.GetKeyDown(KeyCode.V))
        {
            SpawnVictim();
        }
    }
}
```

### 3. Network Synchronization
Remember: UI is local per client. Don't try to sync UI state over network.

### 4. Timer Display Example
The GameManager already handles timers for both roles:
- `instructorDurationText` - shown in Instructor UI
- `trainee1DurationText` & `trainee2DurationText` - shown in Trainee UI

---

## Advanced: Extending the System

### Add New Role-Specific Features

```csharp
public class MyCustomScript : MonoBehaviour
{
    void Start()
    {
        // Only run instructor-specific initialization
        if (GameManager.Instance.IsInstructor())
        {
            InitializeInstructorFeatures();
        }
        
        // Only run trainee-specific initialization
        if (GameManager.Instance.IsTrainee())
        {
            InitializeTraineeFeatures();
        }
    }
    
    private void InitializeInstructorFeatures()
    {
        // Enable monitoring cameras
        // Show admin controls
        // etc.
    }
    
    private void InitializeTraineeFeatures()
    {
        // Enable player controls
        // Show objectives
        // etc.
    }
}
```

---

## Related Documentation

- `NETWORK_MULTIPLAYER_GUIDE.md` - Networking setup
- `LOBBY_SETUP_GUIDE.md` - Lobby configuration
- `AuthManager.cs` - User authentication system
- `NetworkUI.cs` - Network connection UI

---

## Summary

The role-based UI system provides:
? Automatic role detection from auth and networking
? Separate UI panels for instructor and trainee
? Public API for role checking in custom scripts
? Debug logging for troubleshooting
? Validation warnings for role mismatches

This ensures a clean separation between instructor monitoring/control features and trainee gameplay features.
