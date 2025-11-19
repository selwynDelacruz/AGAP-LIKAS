# Instructor Spectator Camera System

This system provides role-based camera management for the AGAP-LIKAS multiplayer disaster training simulation, allowing instructors to spectate trainees with a dedicated camera while trainees use their gameplay cameras.

## ?? Overview

### What This System Does:
- **Instructor (Host)**: Gets a spectator camera that can follow and observe trainees
- **Trainee (Client)**: Uses their normal gameplay camera for first/third-person control
- **Automatic Setup**: Cameras automatically configure based on user role and network ownership

---

## ?? Files Created

1. **TraineeCameraTarget.cs** - Stores each trainee's camera root for the instructor to follow
2. **InstructorCameraController.cs** - Controls the instructor's spectator camera
3. **CameraRoleManager.cs** - Manages camera activation based on role and ownership

---

## ??? Setup Instructions

### Step 1: Player Prefab Setup (For Trainees)

1. **Add TraineeCameraTarget Component:**
   - Open your player prefab (the one spawned for networked players)
   - Add the `TraineeCameraTarget` component
   - Assign the **PlayerCameraRoot** field (usually the transform that the camera follows)
   - Enable "Auto Find Camera Root" if you want automatic detection

2. **Add CameraRoleManager Component (Optional but Recommended):**
   - Add the `CameraRoleManager` component to the same player prefab
   - Assign or auto-find the player's `CinemachineCamera`
   - This ensures non-owner cameras are disabled

### Step 2: Instructor Spectator Camera Setup

1. **Create Spectator Camera GameObject:**
   - In your game scene (NOT in the player prefab), create a new GameObject
   - Name it "InstructorSpectatorCamera"

2. **Add Cinemachine Virtual Camera:**
   - Add a `CinemachineCamera` component to the GameObject
   - Set the priority to something high (e.g., 20) so it takes precedence when active

3. **Add InstructorCameraController:**
   - Add the `InstructorCameraController` component to the same GameObject
   - Assign the `CinemachineCamera` reference (or enable auto-find)
   - Configure settings as desired:
     - **Auto Follow First Trainee**: Enabled by default
     - **Follow Offset**: Default (0, 2, -5) - adjust for your needs
     - **Allow Manual Switching**: Enabled to switch between trainees
     - **Next Trainee Key**: Tab (default)
     - **Previous Trainee Key**: LeftShift (default)

4. **Add CinemachineFollow Component:**
   - Add the `CinemachineFollow` component to configure smooth following
   - This will be auto-configured by the controller, but you can manually tune it

### Step 3: GameManager Integration (Already Set Up)

Your `GameManager.cs` already has the necessary role detection methods:
- `IsInstructor()` - Returns true if user is instructor and host
- `IsTrainee()` - Returns true if user is trainee or client

The camera scripts use these methods automatically.

---

## ?? How It Works

### For Instructors (Host):
1. When the instructor starts as host, their player camera is disabled
2. The spectator camera automatically enables
3. When the first trainee connects, the spectator camera locks onto them
4. Press **Tab** to cycle to the next trainee
5. Press **LeftShift** to cycle to the previous trainee

### For Trainees (Clients):
1. When a trainee connects, their gameplay camera activates normally
2. Their `TraineeCameraTarget` stores their camera root
3. The instructor's spectator camera can now follow them
4. Other players' cameras are disabled on their machine (only their own camera works)

---

## ?? Configuration Options

### TraineeCameraTarget Settings:
```csharp
[SerializeField] private Transform playerCameraRoot;  // Assign manually or use auto-find
[SerializeField] private bool autoFindCameraRoot = true;  // Auto-detect camera root
[SerializeField] private bool debugMode = false;  // Enable debug logging
```

### InstructorCameraController Settings:
```csharp
// Follow Settings
[SerializeField] private bool autoFollowFirstTrainee = true;
[SerializeField] private Vector3 followOffset = new Vector3(0, 2, -5);
[SerializeField] private Vector3 followDamping = new Vector3(1f, 1f, 1f);

// Manual Controls
[SerializeField] private bool allowManualSwitching = true;
[SerializeField] private KeyCode nextTraineeKey = KeyCode.Tab;
[SerializeField] private KeyCode previousTraineeKey = KeyCode.LeftShift;
```

### CameraRoleManager Settings:
```csharp
[SerializeField] private bool disableCameraForNonOwners = true;  // Disable remote cameras
[SerializeField] private bool debugMode = false;  // Enable debug logging
```

---

## ?? Debugging

### Enable Debug Mode:
Set `debugMode = true` in any of the scripts to see detailed console logs:
- Role detection results
- Camera activation/deactivation
- Trainee connection events
- Target switching

### Common Issues:

**Issue 1: Instructor camera doesn't enable**
- Solution: Check that `Type_Of_User` PlayerPref is set to "instructor"
- Verify NetworkManager shows IsHost = true
- Check GameManager.IsInstructor() returns true

**Issue 2: Spectator camera doesn't follow trainee**
- Solution: Ensure TraineeCameraTarget has valid PlayerCameraRoot assigned
- Check that trainee is spawned with NetworkObject and is owned by client
- Enable debug mode to see if trainee was detected

**Issue 3: Multiple cameras active**
- Solution: Ensure CameraRoleManager is on player prefab
- Check camera priorities (spectator should be higher when active)
- Verify only one CinemachineBrain exists in the scene

---

## ?? Testing

### Single-Machine Testing (Using ParrelSync):
1. Start the host instance as Instructor
2. Start a clone instance as Trainee
3. In Instructor view, you should see the spectator camera following the trainee
4. In Trainee view, you should see normal gameplay camera

### Network Testing:
1. Build and run on separate machines
2. Host logs in as instructor
3. Client logs in as trainee
4. Instructor should see trainee's perspective via spectator camera

---

## ?? API Reference

### TraineeCameraTarget

**Static Properties:**
- `Transform LocalTraineeCameraRoot` - Gets the local trainee's camera root

**Instance Properties:**
- `Transform PlayerCameraRoot` - Gets this player's camera root

**Static Methods:**
- `Transform GetCameraRootForClient(ulong clientId)` - Gets camera root for specific client

### InstructorCameraController

**Public Methods:**
- `void OnTraineeConnected(Transform traineeCamera)` - Called when trainee connects
- `void SetTarget(Transform target)` - Manually set follow target
- `Transform GetCurrentTarget()` - Gets current follow target
- `int GetTraineeCount()` - Gets number of connected trainees

**Private Methods:**
- `void SwitchToNextTrainee()` - Cycles to next trainee
- `void SwitchToPreviousTrainee()` - Cycles to previous trainee

### CameraRoleManager

**Private Methods:**
- `void ConfigureCameraForRole()` - Sets up camera based on role
- `bool IsInstructorRole()` - Checks if user is instructor
- `void EnablePlayerCamera()` - Activates player camera
- `void DisablePlayerCamera()` - Deactivates player camera

---

## ?? Advanced Usage

### Custom Camera Offsets:
Adjust the follow offset for different viewing angles:
```csharp
// Top-down view
followOffset = new Vector3(0, 10, 0);

// Side view
followOffset = new Vector3(5, 2, 0);

// Behind and above (default)
followOffset = new Vector3(0, 2, -5);
```

### Multiple Instructors:
The system supports multiple instructors. Each will have their own spectator camera that can independently follow different trainees.

### Programmatic Control:
You can control the spectator camera via code:
```csharp
// Get the controller
var spectatorCam = FindObjectOfType<InstructorCameraController>();

// Switch to specific trainee
spectatorCam.SetTarget(specificTraineeTransform);

// Get current info
Debug.Log($"Following: {spectatorCam.GetCurrentTarget().name}");
Debug.Log($"Total trainees: {spectatorCam.GetTraineeCount()}");
```

---

## ?? Integration with Existing Systems

### Works With:
- ? GameManager role detection
- ? NetworkManager host/client detection
- ? Existing player spawning
- ? Cinemachine virtual cameras
- ? Unity Input System

### Compatible With:
- ? First-person controllers
- ? Third-person controllers
- ? Boat controller
- ? Any NetworkBehaviour player prefab

---

## ?? Performance Considerations

- **Minimal Overhead**: Scripts only run on relevant clients
- **No RPCs**: Uses local detection and static references
- **Efficient Polling**: Update loops are optimized
- **Auto-Cleanup**: Resources are freed on disconnect

---

## ?? Support

If you encounter issues:
1. Enable debug mode in all three scripts
2. Check console for error messages
3. Verify all inspector references are assigned
4. Ensure NetworkManager is properly configured
5. Test in single-player mode first

---

## ?? License

Part of the AGAP-LIKAS project.
Compatible with .NET Framework 4.7.1

---

## ? Future Enhancements

Potential improvements:
- [ ] UI panel showing all trainees with buttons to switch
- [ ] Picture-in-picture mode showing multiple trainees
- [ ] Replay/recording system for instructor review
- [ ] Zoom controls for spectator camera
- [ ] Minimap with trainee positions

---

**Created for AGAP-LIKAS Disaster Response Training Simulation**
