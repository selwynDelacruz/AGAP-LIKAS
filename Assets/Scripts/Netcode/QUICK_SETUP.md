# Quick Setup Guide - Lobby Code System

## Quick Start (5 Minutes)

### Step 1: Add NetworkManager to Scene
1. Create empty GameObject → Name: "NetworkManager"
2. Add Component: `NetworkManager`
3. Add Component: `Unity Transport`
4. In NetworkManager component:
   - Set Transport = Unity Transport component

### Step 2: Add NetworkLobbyManager to Scene
1. Create empty GameObject → Name: "NetworkLobbyManager"
2. Add Component: `NetworkLobbyManager` script

### Step 3: Setup Player Prefab
1. Open/Create your player prefab
2. Add Component: `NetworkObject`
3. Add Component: `NetworkPlayer` script
4. Add this prefab to NetworkManager → Network Prefabs list

### Step 4: Update Your Lobby UI

#### Option A: Update Existing LobbyManager
Your existing `LobbyManager.cs` is already updated! Just add these UI elements in the Inspector:

**Instructor Fields:**
- generateLobbyCodeButton (Button)
- lobbyCodeDisplayText (TextMeshPro)
- startHostButton (Button)
- instructorPanel (GameObject)

**Trainee Fields:**
- lobbyCodeInputField (TMP_InputField)
- joinLobbyButton (Button)
- traineePanel (GameObject)

**Status Fields:**
- connectionStatusText (TextMeshPro)
- playerCountText (TextMeshPro)

#### Option B: Use LobbyUIController
1. Create empty GameObject → Name: "LobbyUI"
2. Add Component: `LobbyUIController` script
3. Assign the same UI references as above

### Step 5: Test It!

**As Instructor:**
1. Run in Editor
2. Click "Generate Lobby Code" button
3. Note the code (e.g., "A3X7B2")
4. Click "Start Host" button

**As Trainee:**
1. Build and run executable (or second editor instance)
2. Enter the lobby code
3. Click "Join Lobby" button

## Minimal UI Setup Example

If you want to create a minimal test scene:

```
Canvas/
├── InstructorPanel/
│   ├── Title (Text): "INSTRUCTOR"
│   ├── GenerateCodeBtn (Button): "Generate Code"
│   ├── CodeDisplay (Text): "---"
│   └── StartHostBtn (Button): "Start Host"
│
├── TraineePanel/
│   ├── Title (Text): "TRAINEE"
│   ├── CodeInput (InputField): placeholder "Enter Code"
│   └── JoinBtn (Button): "Join Lobby"
│
└── StatusPanel/
    ├── Status (Text): "Ready"
    └── Players (Text): "Players: 0"
```

## Code Example - How to Use in Other Scripts

```csharp
// Check how many players are connected
int playerCount = NetworkLobbyManager.Instance.GetConnectedPlayersCount();

// Get current lobby code
string code = NetworkLobbyManager.Instance.GetCurrentLobbyCode();

// Leave lobby
NetworkLobbyManager.Instance.LeaveLobby();

// Subscribe to events
NetworkLobbyManager.Instance.OnPlayersCountChanged += (count) => {
    Debug.Log($"Player count: {count}");
};
```

## Important Notes

⚠️ **For Local Testing:**
- The default setup uses localhost (127.0.0.1)
- Host and client must be on same machine OR same local network
- Port 7777 is used by default

⚠️ **For Production:**
- Integrate Unity Relay Service (see README.md)
- Or implement your own server/matchmaking solution

⚠️ **Role Detection:**
- System checks `PlayerPrefs.GetString("Type_Of_User")`
- Value should be "instructor" or "trainee"
- Or it checks `RoomManager.Instance.isInstructor`

## Troubleshooting

**"NetworkLobbyManager.Instance is null"**
→ Add NetworkLobbyManager script to a GameObject in your scene

**"Can't generate lobby code"**
→ Make sure Type_Of_User = "instructor" in PlayerPrefs

**"Join fails"**
→ Check that host started BEFORE client tries to join
→ Verify lobby code is exactly 6 characters
→ Check firewall settings

**"Player not spawning"**
→ Add player prefab to NetworkManager → Network Prefabs list
→ Ensure NetworkObject component is on player prefab

## Next Steps

1. ✅ Setup the basic system (you're here!)
2. 📝 Customize UI to match your game's style
3. 🎮 Test with multiple clients
4. 🌐 Integrate Unity Relay for internet play
5. 🎨 Add lobby features (ready system, team selection, etc.)

For complete documentation, see: `README.md`
