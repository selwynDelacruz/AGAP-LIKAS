# Lobby System Setup Guide

## ?? Overview
This guide will help you set up the complete lobby system with LAN discovery for your multiplayer disaster simulation game.

**SIMPLIFIED APPROACH:** NetworkManager exists **directly in the LobbyMenu scene** (not as a prefab). This is the simplest and most visual approach.

---

## ?? Prerequisites

Before starting, ensure you have:
- Unity 2021.3 or higher
- Unity Netcode for GameObjects package installed
- Unity Transport (UTP) package installed
- TextMeshPro package installed

---

## ?? Scripts Created

The following scripts have been created in your project:

### Core Lobby Scripts (`Assets/Scripts/Lobby/`)
1. `LobbyCodeGenerator.cs` - Generates random lobby codes
2. `LobbyBroadcaster.cs` - Broadcasts lobby on LAN (Host side)
3. `LobbyScanner.cs` - Scans for lobbies on LAN (Client side)
4. `UnityMainThreadDispatcher.cs` - Handles thread-safe Unity API calls
5. `LobbyMenuManager.cs` - Manages Lobby Menu scene and handles NetworkManager
6. `LobbyRoomManager.cs` - Manages Lobby Room scene

### Network Scripts (`Assets/Scripts/Network/`)
1. `NetworkManagerInitializer.cs` - ~~Not used in this approach~~
2. `NetworkManagerDebugger.cs` - Debug UI for network status (optional)

---

## ?? Step-by-Step Setup

### STEP 1: Setup Main Menu Scene (NO Network Setup Needed!)

**Scene: MainMenu** (or your authentication scene)

? **Good news:** You don't need to add any network-related components to MainMenu!

Just ensure your existing `GoToLobby()` method loads the LobbyMenu scene:

```csharp
public void GoToLobby()
{
    Debug.Log("Loading Lobby Menu scene...");
    SceneManager.LoadScene("LobbyMenu");
}
```

**That's it for MainMenu!** 

---

### STEP 2: Create Lobby Menu Scene with NetworkManager

1. **Create New Scene:**
   - File ? New Scene
   - Save as: `LobbyMenu`

2. **Create UI Canvas:**
   - Right-click Hierarchy ? UI ? Canvas
   - Canvas Scaler: Scale With Screen Size
   - Reference Resolution: 1920x1080

3. **Add NetworkManager to the scene:**
   - Right-click Hierarchy ? Create Empty
   - Name it: `NetworkManager`
   - Click "Add Component" ? Search `NetworkManager` ? Add it
   - Click "Add Component" ? Search `UnityTransport` ? Add it

4. **Configure NetworkManager Component:**
   ```
   NetworkManager Component:
   ? Don't Destroy On Load: CHECKED (VERY IMPORTANT!)
   Network Transport: UnityTransport (should auto-assign)
   Network Prefabs: (leave empty)
   Player Prefab: (leave empty or assign your player prefab)
   ```

5. **Configure UnityTransport Component:**
   ```
   UnityTransport Component:
   Protocol Type: UnityTransport
   Connection Data:
   ?? Address: 127.0.0.1
   ?? Port: 7777
   ?? Server Listen Address: 0.0.0.0
   ```

6. **Create UnityMainThreadDispatcher:**
   - Create Empty GameObject: `UnityMainThreadDispatcher`
   - Add Component: `UnityMainThreadDispatcher` (from Lobby namespace)

7. **Create LobbyMenuManager:**
   - Create Empty GameObject: `LobbyMenuManager`
   - Add Component: `LobbyMenuManager` (from Lobby namespace)
   - In Inspector:
     - Set `Lobby Room Scene Name`: "LobbyRoom"
     - Set `Port`: 7777
     - ? Check `Show Debug Logs`

8. **Create Instructor UI (Create Lobby Panel):**
   ```
   Canvas
   ?? CreateLobbyPanel (Panel)
      ?? Title (TextMeshPro): "Create Lobby"
      ?? CreateButton (Button): "Create Lobby"
      ?? LobbyCodeDisplay (TextMeshPro): "Lobby Code: XXXX"
      ?? BackButton (Button): "Back"
   ```

9. **Create Trainee UI (Join Lobby Panel):**
   ```
   Canvas
   ?? JoinLobbyPanel (Panel)
      ?? Title (TextMeshPro): "Join Lobby"
      ?? LobbyCodeInput (TMP_InputField): Placeholder "Enter Code"
      ?? JoinButton (Button): "Join Lobby"
      ?? StatusText (TextMeshPro): "" (for messages)
      ?? BackButton (Button): "Back"
   ```

10. **Assign References in LobbyMenuManager:**
    - Drag all UI elements to corresponding fields in Inspector:
      - Create Lobby Panel ? `createLobbyPanel`
      - Join Lobby Panel ? `joinLobbyPanel`
      - Create Button ? `createLobbyButton`
      - Lobby Code Display Text ? `lobbyCodeDisplayText`
      - Lobby Code Input Field ? `lobbyCodeInputField`
      - Join Button ? `joinLobbyButton`
      - Status Text ? `statusText`
      - Back Button ? `backButton`
    - Set `Lobby Room Scene Name`: "LobbyRoom"
    - Set `Port`: 7777
    - ? Check `Show Debug Logs`

11. **Optional: Add Network Debugger:**
    - Create Empty GameObject: `NetworkDebugger`
    - Add Component: `NetworkManagerDebugger`
    - Configure:
      - ? Check `Show On Screen Debug`
      - Toggle Key: `F1`

12. **Add to Build Settings:**
    - File ? Build Settings
    - Click "Add Open Scenes" (with LobbyMenu scene open)

---

### STEP 3: Create Lobby Room Scene

1. **Create New Scene:**
   - File ? New Scene
   - Save as: `LobbyRoom`

2. **Create UI Canvas:**
   - Right-click Hierarchy ? UI ? Canvas
   - Canvas Scaler: Scale With Screen Size
   - Reference Resolution: 1920x1080

3. **Create LobbyRoomManager:**
   - Create Empty GameObject: `LobbyRoomManager`
   - Add Component: `LobbyRoomManager` (from Lobby namespace)
   - **IMPORTANT:** Add Component: `NetworkObject`
     - This is required for NetworkBehaviour!

4. **Create Host Panel (Instructor UI):**
   ```
   Canvas
   ?? HostPanel (Panel)
      ?? Title (TextMeshPro): "Game Configuration"
      ?? TaskCount Section:
      ?  ?? MinusButton (Button): "-"
      ?  ?? TaskCountText (TextMeshPro): "5"
      ?  ?? PlusButton (Button): "+"
      ?? DisasterDropdown (TMP_Dropdown):
      ?  Options: "Flood", "Earthquake", "TestKen"
      ?? DurationDropdown (TMP_Dropdown):
      ?  Options: "5 minutes", "8 minutes", "10 minutes"
      ?? StartGameButton (Button): "START GAME"
   ```

5. **Create Client Panel (Trainee UI):**
   ```
   Canvas
   ?? ClientPanel (Panel)
      ?? Title (TextMeshPro): "Waiting Room"
      ?? WaitingMessage (TextMeshPro): "Waiting for host..."
   ```

6. **Create Network Status Display (Shared):**
   ```
   Canvas
   ?? StatusPanel (Panel)
      ?? LobbyCodeText (TextMeshPro): "Lobby Code: XXXX"
      ?? ConnectedPlayersText (TextMeshPro): "Connected: 1"
      ?? InstructorNameText (TextMeshPro): "Instructor: Name"
      ?? TraineeNamesText (TextMeshPro): "Trainees: 0"
      ?? CurrentSettingsText (TextMeshPro): "Settings..."
   ```

7. **Assign References in LobbyRoomManager:**
   - Drag all UI elements to corresponding fields
   - Set `Min Tasks`: 5
   - Set `Max Tasks`: 8
   - ? Check `Show Debug Logs`

8. **Add to Build Settings:**
   - File ? Build Settings
   - Click "Add Open Scenes" (with LobbyRoom scene open)
   - **IMPORTANT:** Make sure it's AFTER LobbyMenu in the list

---

### STEP 4: Configure Game Scenes

For each of your game scenes (Flood, Earthquake, TestKen):

1. **Optional: Add NetworkManagerDebugger:**
   - Create Empty GameObject: `NetworkDebugger`
   - Add Component: `NetworkManagerDebugger`
   - Configure:
     - ? Check `Show On Screen Debug`
     - Toggle Key: `F1`

2. **Add to Build Settings:**
   - File ? Build Settings
   - Add all game scenes

---

## ?? Testing the Lobby System

### Test 1: Single Player (No Network)

1. **Play MainMenu scene**
2. Login as Instructor
3. Click "Play Game" ? should load LobbyMenu
4. **Expected in Console:**
   ```
   [LobbyMenuManager] NetworkManager set to DontDestroyOnLoad
   [LobbyMenuManager] User role: instructor
   [LobbyMenuManager] Instructor UI enabled
   ```
5. **Check Hierarchy:**
   - Look for "DontDestroyOnLoad" folder
   - NetworkManager should be inside it
6. Click "Create Lobby"
7. **Expected:**
   - Console: `[LobbyMenuManager] Started as Host on 0.0.0.0:7777`
   - Console: `[LobbyMenuManager] Generated lobby code: (e.g., "GAME42")`
   - Console: `[LobbyBroadcaster] Started broadcasting`
   - Scene loads to LobbyRoom
8. In LobbyRoom, Host panel should be visible with configuration options
9. Configure settings and click "START GAME"
10. Game scene should load (Flood/Earthquake/TestKen)

### Test 2: Multiplayer (LAN)

**Host Machine (Instructor):**
1. Run game in Unity Editor
2. Login as Instructor
3. Go to LobbyMenu ? Create Lobby
4. **Note the Lobby Code** displayed (e.g., "GAME42")
5. Wait in LobbyRoom

**Client Machine (Trainee):**
1. **Build** the game (File ? Build Settings ? Build)
2. Run the built executable on same network
3. Login as Trainee
4. Go to LobbyMenu ? Join Lobby
5. Enter the **Lobby Code** from host
6. Click "Join Lobby"
7. **Expected:**
   - Console: "Searching for lobby GAME42..."
   - Console: "Lobby found at [IP]:[Port]"
   - Console: "Connected!"
   - Scene loads to LobbyRoom
8. Client sees Host's settings (read-only)
9. Wait for Host to click "START GAME"
10. Both machines load game scene together

### Test 3: Press F1 for Debug Info

While playing, press `F1` to see:
- NetworkManager Status
- Is Host / Is Server / Is Client
- Connected Clients count
- Lobby Code
- Connection details

---

## ?? Troubleshooting

### Issue: "NetworkManager Singleton is null"
**Solution:** 
- Open LobbyMenu scene
- Verify NetworkManager GameObject exists in hierarchy
- Check that it has both NetworkManager and UnityTransport components
- Verify "Don't Destroy On Load" is CHECKED on NetworkManager component

### Issue: NetworkManager doesn't persist to other scenes
**Solution:**
- Select NetworkManager in LobbyMenu scene
- In Inspector, find NetworkManager component
- Ensure "Don't Destroy On Load" checkbox is CHECKED
- LobbyMenuManager calls DontDestroyOnLoad() in Awake()

### Issue: "Lobby not found" when joining
**Possible Causes:**
1. **Different Networks:** Ensure both machines are on same WiFi/LAN
2. **Firewall:** Windows Firewall may block UDP broadcasts
   - Solution: Allow Unity through Windows Firewall
   - Go to: Windows Defender Firewall ? Allow an app ? Unity Editor
3. **Wrong Code:** Code is case-sensitive (but auto-converted to uppercase)
4. **Host not broadcasting:** Check console logs on host machine

### Issue: Client doesn't follow when host loads scene
**Solution:**
- Verify NetworkManager has `DontDestroyOnLoad` checked
- Check that LobbyRoomManager has NetworkObject component
- Ensure all scenes are in Build Settings
- NetworkManager must persist from LobbyMenu

### Issue: "NetworkBehaviour requires NetworkObject"
**Solution:**
- Add `NetworkObject` component to LobbyRoomManager GameObject in LobbyRoom scene

### Issue: Settings not syncing to clients
**Solution:**
- Verify NetworkVariables are declared correctly in LobbyRoomManager
- Check that only Host/Server can change settings
- Ensure LobbyRoomManager has NetworkObject component

---

## ?? Important Notes

1. **Scene Order in Build Settings:**
   ```
   0. MainMenu         (No network components)
   1. LobbyMenu        (Has NetworkManager in scene)
   2. LobbyRoom        (NetworkObject required on LobbyRoomManager)
   3. Flood
   4. Earthquake
   5. TestKen
   6. Result
   ```

2. **NetworkManager Location:**
   - ? **NOT** in MainMenu scene
   - ? **EXISTS** in LobbyMenu scene hierarchy
   - ? **DontDestroyOnLoad** checked
   - ? Persists automatically to all subsequent scenes

3. **PlayerPrefs Used:**
   - `Type_Of_User`: "instructor" or "trainee"
   - `LobbyCode`: Current lobby code
   - `TaskCount`: Number of victims (5-8)
   - `DisasterType`: "Flood", "Earthquake", or "TestKen"
   - `GameDuration`: Duration in seconds (300/480/600)
   - `CameFromLobby`: 1 if from lobby, 0 otherwise

4. **Network Ports:**
   - Port 7777: Game connection (Netcode)
   - Port 7778: LAN discovery (UDP broadcast)
   - Make sure these ports are not blocked by firewall

5. **Thread Safety:**
   - LobbyBroadcaster and LobbyScanner use background threads
   - UnityMainThreadDispatcher ensures Unity API calls happen on main thread
   - Never call Unity API directly from broadcast/scan threads

---

## ?? Why This Approach is Better

### ? Advantages:

1. **Simpler Setup:**
   - No prefabs to create
   - No runtime spawning logic
   - Just place NetworkManager in scene

2. **Visual and Obvious:**
   - NetworkManager is visible in hierarchy
   - Easy to find and configure
   - Beginner-friendly

3. **Easier Configuration:**
   - Edit NetworkManager settings directly in scene
   - See changes immediately
   - No prefab editing needed

4. **Better Debugging:**
   - NetworkManager visible while playing
   - Can inspect settings at runtime
   - Clear hierarchy organization

5. **No References Needed:**
   - LobbyMenuManager doesn't need prefab reference
   - NetworkManager.Singleton automatically works
   - Less prone to "missing reference" errors

### ?? Flow Summary:

```
MainMenu (No NetworkManager)
?? User logs in
?? Stores role in PlayerPrefs
?? Clicks "Play Game"
    ?? Loads LobbyMenu
        ?? LobbyMenu Scene Contains:
            ?? NetworkManager ? Already in scene
            ?? LobbyMenuManager.Awake():
                ?? Calls DontDestroyOnLoad(NetworkManager)
                ?? NetworkManager persists through:
                    ?? LobbyRoom
                    ?? Game Scene (Flood/Earthquake/TestKen)
                    ?? Result (until cleanup)
```

---

## ?? Next Steps

After completing the setup:

1. **Test locally** - Create and join in same Unity Editor
2. **Build and test** on multiple machines on same network
3. **Customize UI** to match your game's theme
4. **Add player names** to lobby room display
5. **Add ready-up system** (optional enhancement)
6. **Add lobby browser** to show all active lobbies (optional enhancement)

---

## ?? References

- [Unity Netcode Documentation](https://docs-multiplayer.unity3d.com/netcode/current/about/)
- [Unity Transport (UTP) Documentation](https://docs.unity3d.com/Packages/com.unity.transport@2.0/manual/index.html)
- `SIMPLIFIED_SETUP.md` - Quick reference for this approach
- Your existing GameManager.cs for game flow

---

**Setup Complete!** ??

If you encounter any issues, check the Troubleshooting section or review the console logs with Debug Mode enabled.
