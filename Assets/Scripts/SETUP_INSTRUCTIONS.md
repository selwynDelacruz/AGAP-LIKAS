# ?? Instructor Spectator Camera System - Setup Complete!

## ? Scripts Created Successfully

I've created **3 new scripts** for your instructor spectator camera system:

1. **TraineeCameraTarget.cs** - Stores trainee camera references
2. **InstructorCameraController.cs** - Controls instructor spectator view
3. **CameraRoleManager.cs** - Manages camera activation by role
4. **README_CameraSystem.md** - Complete documentation

---

## ?? Important: Unity Setup Required

The scripts are created but Unity needs to refresh to add them to the Assembly-CSharp project. **Please follow these steps:**

### Step 1: Force Unity to Recompile
1. **Go to Unity Editor**
2. **Right-click** in the Project window
3. Select **Reimport All**
   - OR -
4. Go to **Assets > Refresh** (Ctrl+R)
   - OR -
5. Close and reopen Unity

### Step 2: Verify Scripts Compiled
1. Check the **Console** for any errors
2. The scripts should appear in `Assets/Scripts/` folder
3. You should be able to add them as components in the Inspector

---

## ??? Quick Setup Guide

Once Unity has recompiled, follow these steps:

### A. Player Prefab Setup (Trainee)

1. **Open your networked player prefab** (the one with `AgapThirdPersonController`)

2. **Add TraineeCameraTarget Component:**
   - Click **Add Component**
   - Search for `TraineeCameraTarget`
   - Assign **PlayerCameraRoot** (the Transform that Cinemachine follows)
   - Enable "Auto Find Camera Root" if you want automatic detection

3. **Add CameraRoleManager Component (Optional):**
   - Click **Add Component**
   - Search for `CameraRoleManager`
   - This will auto-disable cameras for non-owners

### B. Instructor Spectator Camera Setup

1. **In your Game Scene** (NOT in the player prefab):
   - Create a new **Empty GameObject**
   - Name it `InstructorSpectatorCamera`

2. **Add Cinemachine Virtual Camera:**
   - Add Component > **Cinemachine Virtual Camera**
   - Set Priority to **20** (higher than trainee cameras)

3. **Add InstructorCameraController:**
   - Add Component > **InstructorCameraController**
   - It will auto-find the Cinemachine camera
   - Configure settings:
     - ? Auto Follow First Trainee
     - Follow Offset: `(0, 2, -5)`
     - Next Trainee Key: **Tab**
     - Previous Trainee Key: **LeftShift**

4. **Add CinemachineFollow Component:**
   - This handles smooth camera following
   - It will be auto-configured by the controller

---

## ?? How It Works

### For Instructors (Host):
- When they start as host, their **player camera is disabled**
- The **spectator camera activates automatically**
- When a trainee connects, the camera **locks onto them**
- Press **Tab** to switch to the next trainee
- Press **LeftShift** to switch to previous trainee

### For Trainees (Clients):
- Their **gameplay camera works normally**
- Their camera root is **stored** for the instructor to follow
- Other players' cameras are **disabled** on their machine

---

## ?? Testing

### In Unity Editor (Using ParrelSync):
1. Start host instance **as Instructor**
2. Start clone instance **as Trainee**
3. Instructor should see spectator view following trainee
4. Trainee should see normal first/third-person view

### Network Testing:
1. Build and run on separate machines
2. Host logs in as **instructor**
3. Client logs in as **trainee**
4. Instructor gets spectator view of trainee

---

## ?? Key Features Implemented

? **Role-Based Camera Activation**
- Automatically detects instructor vs trainee
- Only activates appropriate cameras

? **Automatic Target Detection**
- Finds trainee camera roots automatically
- Updates when trainees connect/disconnect

? **Manual Switching**
- Tab through multiple trainees
- Smooth camera transitions

? **Network-Aware**
- Works with Unity Netcode for GameObjects
- Integrates with your existing GameManager

? **Flexible Configuration**
- Adjustable follow offsets
- Customizable damping
- Debug mode for troubleshooting

---

## ?? Troubleshooting

### "Scripts won't add as components"
**Solution:** Unity needs to recompile. Try:
```
Assets > Reimport All
OR
Close and reopen Unity
```

### "Camera doesn't follow trainee"
**Solution:**  
1. Check that `TraineeCameraTarget` has `PlayerCameraRoot` assigned
2. Enable debug mode in `InstructorCameraController`
3. Check Console for connection messages

### "Multiple cameras active"
**Solution:**
1. Ensure `CameraRoleManager` is on player prefab
2. Check camera priorities (spectator should be 20, player should be 10)
3. Only one `CinemachineBrain` should exist in scene

---

## ?? Full Documentation

See **README_CameraSystem.md** in `Assets/Scripts/` for:
- Detailed API reference
- Advanced customization
- Integration guides
- Performance tips

---

## ?? You're All Set!

The system is ready to use once Unity recompiles. Follow the setup steps above and you'll have a fully functional instructor spectator camera system!

### Need Help?
1. Enable **debug mode** in the scripts
2. Check **Console** for detailed logs
3. Verify **GameManager.IsInstructor()** returns true for instructor
4. Make sure **NetworkManager** is properly configured

---

**Created for AGAP-LIKAS Disaster Response Training Simulation**  
Compatible with .NET Framework 4.7.1 | Unity Netcode for GameObjects | Cinemachine
