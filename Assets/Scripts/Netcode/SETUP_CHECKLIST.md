# Setup Checklist - Network Lobby System

Use this checklist to ensure proper setup of the lobby system.

## ☐ Phase 1: Unity Packages

- [ ] Unity Netcode for GameObjects installed (v1.0.0+)
  - Window > Package Manager > Unity Registry
  - Search "Netcode for GameObjects"
  - Click Install

- [ ] TextMeshPro installed
  - Should be already in project
  - Import TMP Essentials if prompted

## ☐ Phase 2: Scene Setup

### NetworkManager Configuration
- [ ] Create GameObject "NetworkManager"
- [ ] Add component: NetworkManager
- [ ] Add component: Unity Transport
- [ ] In NetworkManager inspector:
  - [ ] Transport field = Unity Transport component
  - [ ] Network Prefabs list created (will add player later)

### NetworkLobbyManager Configuration
- [ ] Create GameObject "NetworkLobbyManager"
- [ ] Add component: NetworkLobbyManager
- [ ] Configure in inspector:
  - [ ] Lobby Code Length = 6 (or desired length)

### NetworkSceneManager Configuration (Optional)
- [ ] Create GameObject "NetworkSceneManager"
- [ ] Add component: NetworkSceneManager
- [ ] Configure in inspector:
  - [ ] Don't Destroy On Load = true
  - [ ] Auto Setup On Awake = true

## ☐ Phase 3: Player Prefab

- [ ] Create/Open player prefab
- [ ] Add component: NetworkObject
- [ ] Add component: NetworkPlayer
- [ ] Configure NetworkObject:
  - [ ] Don't Destroy With Owner = unchecked (usually)
  - [ ] Synchronize Transform = checked (if needed)
- [ ] Add prefab to NetworkManager:
  - [ ] NetworkManager > Network Prefabs > Add (+)
  - [ ] Drag player prefab to new slot

## ☐ Phase 4: UI Setup - Instructor Panel

- [ ] Create Panel "InstructorPanel"
- [ ] Add to panel:
  - [ ] Button "GenerateLobbyCodeButton"
    - [ ] Text: "Generate Lobby Code"
  - [ ] TextMeshPro "LobbyCodeDisplay"
    - [ ] Text: "Lobby Code: ---"
    - [ ] Font Size: Large enough to read
  - [ ] Button "StartHostButton"
    - [ ] Text: "Start Hosting"
  - [ ] Button "CopyCodeButton" (optional)
    - [ ] Text: "Copy Code"

## ☐ Phase 5: UI Setup - Trainee Panel

- [ ] Create Panel "TraineePanel"
- [ ] Add to panel:
  - [ ] TextMeshPro "Instructions"
    - [ ] Text: "Enter lobby code to join:"
  - [ ] TMP_InputField "LobbyCodeInput"
    - [ ] Placeholder: "Enter Code"
    - [ ] Character Limit: 6
    - [ ] Content Type: Alphanumeric
  - [ ] Button "JoinLobbyButton"
    - [ ] Text: "Join Lobby"

## ☐ Phase 6: UI Setup - Shared Elements

- [ ] Create Panel "StatusPanel"
- [ ] Add to panel:
  - [ ] TextMeshPro "ConnectionStatusText"
    - [ ] Text: "Status: Ready"
  - [ ] TextMeshPro "PlayerCountText"
    - [ ] Text: "Players: 0"
  - [ ] Button "DisconnectButton" (optional)
    - [ ] Text: "Disconnect"

## ☐ Phase 7: Connect UI to Scripts

### Option A: Using LobbyManager
- [ ] Find your LobbyManager GameObject
- [ ] In inspector, assign references:

**Instructor Section:**
- [ ] Generate Lobby Code Button → GenerateLobbyCodeButton
- [ ] Lobby Code Display Text → LobbyCodeDisplay
- [ ] Start Host Button → StartHostButton
- [ ] Instructor Panel → InstructorPanel GameObject

**Trainee Section:**
- [ ] Lobby Code Input Field → LobbyCodeInput
- [ ] Join Lobby Button → JoinLobbyButton
- [ ] Trainee Panel → TraineePanel GameObject

**Status Section:**
- [ ] Connection Status Text → ConnectionStatusText
- [ ] Player Count Text → PlayerCountText

### Option B: Using LobbyUIController
- [ ] Create GameObject "LobbyUI"
- [ ] Add component: LobbyUIController
- [ ] Assign all same references as above
- [ ] Configure:
  - [ ] Auto Detect Role = true

## ☐ Phase 8: Role Configuration

Choose one method to set user role:

### Method A: Using RoomManager
- [ ] Ensure RoomManager exists
- [ ] RoomManager.isInstructor is set correctly

### Method B: Using PlayerPrefs
- [ ] Set PlayerPrefs before lobby scene:
```csharp
PlayerPrefs.SetString("Type_Of_User", "instructor"); // or "trainee"
```

## ☐ Phase 9: Build Settings

- [ ] File > Build Settings
- [ ] Add your lobby scene
- [ ] Add your game scene(s)
- [ ] Order: Lobby first, then game scenes
- [ ] Platform: Select target platform

## ☐ Phase 10: Testing - Local

### Editor Test (Instructor)
- [ ] Open lobby scene
- [ ] Press Play
- [ ] Verify instructor panel is visible
- [ ] Click "Generate Lobby Code"
- [ ] Verify code appears (e.g., "A3X7B2")
- [ ] Click "Start Host"
- [ ] Verify status shows "Hosting"
- [ ] Verify player count shows "1"

### Build Test (Trainee)
- [ ] File > Build and Run
- [ ] Executable opens
- [ ] Verify trainee panel is visible
- [ ] Enter the lobby code from editor
- [ ] Click "Join Lobby"
- [ ] Verify status shows "Connecting" then "Connected"
- [ ] In editor, verify player count shows "2"

## ☐ Phase 11: Functionality Verification

- [ ] Code generation works
- [ ] Code displays correctly
- [ ] Host can start hosting
- [ ] Trainee can join with correct code
- [ ] Invalid code shows error
- [ ] Player count updates correctly
- [ ] Both panels switch based on role
- [ ] Connection status updates
- [ ] Disconnect works (if implemented)

## ☐ Phase 12: Production Setup (Optional)

### Unity Relay Integration
- [ ] Window > Services
- [ ] Link project to Unity Cloud
- [ ] Enable Relay service
- [ ] Install Relay package
- [ ] Update NetworkLobbyManager:
  - [ ] ConfigureTransportForHost() with Relay
  - [ ] ConfigureTransportForClient() with Relay
- [ ] Test with devices on different networks

## ☐ Phase 13: Advanced Features (Optional)

- [ ] Lobby code expiration timer
- [ ] Lobby browser/list
- [ ] Password protection
- [ ] Team selection UI
- [ ] Ready-up system
- [ ] Host migration
- [ ] Reconnection support
- [ ] Chat system
- [ ] Lobby settings sync

## ☐ Phase 14: Documentation

- [ ] Read README.md
- [ ] Review QUICK_SETUP.md
- [ ] Check IMPLEMENTATION_SUMMARY.md
- [ ] Study ARCHITECTURE.md
- [ ] Look at LobbySystemExample.cs

## Common Issues Checklist

If something doesn't work, check:

- [ ] NetworkManager exists in scene
- [ ] Unity Transport is attached and assigned
- [ ] NetworkLobbyManager exists in scene
- [ ] Player prefab has NetworkObject
- [ ] Player prefab is in Network Prefabs list
- [ ] UI references are all assigned
- [ ] User role is set (instructor/trainee)
- [ ] Host started before client tries to join
- [ ] Lobby code is exactly 6 characters
- [ ] Using same Unity version for build and editor
- [ ] Firewall not blocking port 7777
- [ ] No compile errors in console

## Quick Commands

### Set User as Instructor
```csharp
PlayerPrefs.SetString("Type_Of_User", "instructor");
PlayerPrefs.Save();
```

### Set User as Trainee
```csharp
PlayerPrefs.SetString("Type_Of_User", "trainee");
PlayerPrefs.Save();
```

### Manual Network Start (Testing)
```csharp
// Host
NetworkLobbyManager.Instance.GenerateLobbyCode();
NetworkLobbyManager.Instance.StartHostWithLobbyCode();

// Client
NetworkLobbyManager.Instance.JoinLobbyWithCode("ABC123");
```

## Support

For issues or questions:
1. Check console for error messages
2. Review README.md troubleshooting section
3. Verify all checklist items completed
4. Check Unity Netcode documentation
5. Test with minimal setup first

## Completion

When all items are checked:
✅ System is ready for use
✅ Test with real users
✅ Monitor for issues
✅ Iterate and improve

---

**Last Updated:** November 16, 2025  
**Version:** 1.0  
**Status:** Production Ready (Local Network) / Development (Internet Play)
