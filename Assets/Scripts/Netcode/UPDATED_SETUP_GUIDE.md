# Updated Setup Guide - Three-Scene Lobby System

## System Overview

Your game now has a three-scene multiplayer flow:

1. **Main Menu Scene** - Login & Lobby Code Generation/Joining
2. **Lobby Scene** - Game Settings & Player List Display
3. **Game Scenes** - Earthquake / Flood / TestKen

## Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    MAIN MENU SCENE                           │
│                                                              │
│  [Login Panel] → [Auth Manager]                             │
│     ↓                                                        │
│  [Select Role: Instructor / Trainee / Super Admin]          │
│     ↓                                                        │
│  ┌────────────────────┐       ┌────────────────────┐       │
│  │   INSTRUCTOR       │       │     TRAINEE        │       │
│  │                    │       │                    │       │
│  │ [Generate Code]    │       │ [Enter Code]       │       │
│  │  → Code: A3X7B2    │       │  → Input: A3X7B2   │       │
│  │ [Start Hosting]    │       │ [Join Lobby]       │       │
│  │  → Creating lobby  │       │  → Connecting...   │       │
│  └────────────────────┘       └────────────────────┘       │
│           ↓                            ↓                    │
│           └────────────┬───────────────┘                    │
│                        ↓                                     │
└────────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│                     LOBBY SCENE                              │
│                                                              │
│  [Lobby Code Display: A3X7B2]                               │
│                                                              │
│  [Connected Players]                                         │
│  ┌──────────────────────────────────────────────┐           │
│  │ INSTRUCTOR: JohnDoe                          │           │
│  │ TRAINEE: JaneSmith                           │           │
│  │ TRAINEE: BobJones                            │           │
│  └──────────────────────────────────────────────┘           │
│                                                              │
│  [Game Settings] (Instructor Only)                          │
│  ┌──────────────────────────────────────────────┐           │
│  │ Tasks: [- 3 +]                               │           │
│  │ Disaster: [Flood ▼]                          │           │
│  │ Duration: [5 minutes ▼]                      │           │
│  │                                               │           │
│  │ [START GAME]                                 │           │
│  └──────────────────────────────────────────────┘           │
│                        ↓                                     │
└─────────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│                    GAME SCENE                                │
│           (Earthquake / Flood / TestKen)                     │
│                                                              │
│  [Synchronized gameplay for all connected players]          │
│  [All players see the same game state]                      │
│  [Game settings applied from lobby]                         │
└─────────────────────────────────────────────────────────────┘
```

## Complete Setup Instructions

### Phase 1: Main Menu Scene Setup

#### 1. Add Network Components
Create GameObject "NetworkManager":
- Add Component: `NetworkManager`
- Add Component: `Unity Transport`
- Configure Network Prefabs list (add player prefab)

Create GameObject "NetworkLobbyManager":
- Add Component: `NetworkLobbyManager`
- This persists across all scenes (DontDestroyOnLoad)

#### 2. Create Lobby Code UI (After Login)

**For Instructor:**
```
InstructorLobbyPanel/
├── Title (Text): "Create Lobby"
├── GenerateCodeButton (Button): "Generate Lobby Code"
├── LobbyCodeDisplay (Text): "Code: ---"
├── ContinueButton (Button): "Continue to Lobby"
└── StatusText (Text): ""
```

**For Trainee:**
```
TraineeLobbyPanel/
├── Title (Text): "Join Lobby"
├── CodeInput (TMP_InputField): Placeholder "Enter Code"
├── JoinButton (Button): "Join Lobby"
└── StatusText (Text): ""
```

#### 3. Add MainMenuLobbyController
- Create GameObject "MainMenuLobbyController"
- Add Component: `MainMenuLobbyController`
- Assign UI references in inspector
- Set Lobby Scene Name: "LobbyScene" (or your actual scene name)

#### 4. Integration with AuthManager

After login, show the lobby code panels:
```csharp
// In your AuthManager or login success handler
if (MainMenuLobbyController.Instance != null)
{
    MainMenuLobbyController.Instance.ShowLobbyCodePanel();
}
```

### Phase 2: Lobby Scene Setup

#### 1. Create Lobby Scene Manager
Create GameObject "LobbySceneManager":
- Add Component: `LobbySceneManager`

#### 2. Game Settings UI (Instructor Only)

```
GameSettingsPanel/
├── TaskCount/
│   ├── MinusButton (Button): "-"
│   ├── CountText (Text): "1"
│   └── PlusButton (Button): "+"
├── DisasterDropdown (TMP_Dropdown)
│   └── Options: "Flood", "Earthquake", "TestKen"
├── DurationDropdown (TMP_Dropdown)
│   └── Options: "5 minutes", "8 minutes", "10 minutes"
└── StartGameButton (Button): "START GAME"
```

#### 3. Player List Display

```
PlayerListPanel/
├── LobbyCodeDisplay (Text): "Lobby Code: A3X7B2"
├── PlayerCountText (Text): "Players: 1"
├── ScrollView/
│   └── Content (Transform) ← PlayerListContainer
└── PlayerListItemPrefab ← Assign this prefab
```

**Create PlayerListItemPrefab:**
- Create UI Panel prefab
- Add TMP_Text component
- Save as prefab
- Assign to LobbySceneManager

#### 4. Assign References

In LobbySceneManager inspector:
- Game Settings Panel
- All buttons and dropdowns
- Player List Container (ScrollView/Content)
- Player List Item Prefab
- Scene names for Earthquake, Flood, TestKen

### Phase 3: Player Prefab Setup

Your player prefab needs:
1. `NetworkObject` component
2. `NetworkPlayer` component
3. Add to NetworkManager → Network Prefabs list

The `NetworkPlayer` script will automatically:
- Get username from AuthManager
- Sync role (Instructor/Trainee)
- Update the lobby player list

### Phase 4: Game Scenes

No special setup needed! The game scenes will:
- Automatically receive all connected players
- Have access to game settings via PlayerPrefs:
  - `PlayerPrefs.GetInt("TaskCount")`
  - `PlayerPrefs.GetString("DisasterType")`
  - `PlayerPrefs.GetInt("GameDuration")`

## User Flow Examples

### Instructor Flow:
1. **Main Menu** → Login as Instructor
2. **Main Menu** → Click "Generate Lobby Code" → Get code "A3X7B2"
3. **Main Menu** → System auto-hosts → Click "Continue to Lobby"
4. **Lobby Scene** → See connected players list
5. **Lobby Scene** → Configure: Tasks=3, Disaster=Flood, Duration=5min
6. **Lobby Scene** → Click "START GAME"
7. **Game Scene** → Flood scene loads for everyone

### Trainee Flow:
1. **Main Menu** → Login as Trainee
2. **Main Menu** → Enter code "A3X7B2" → Click "Join Lobby"
3. **Main Menu** → System auto-connects → Auto-transition to Lobby
4. **Lobby Scene** → See player list, wait for instructor
5. **Lobby Scene** → Cannot change settings (instructor only)
6. **Lobby Scene** → Instructor starts game
7. **Game Scene** → Same scene loads automatically

## Data Persistence

### From AuthManager (After Login):
```csharp
PlayerPrefs.SetString("Type_Of_User", "instructor"); // or "trainee"
// AuthManager already stores:
// - Current_Name
// - Current_Username
// - Current_Gender
// - Current_Age
```

### To Network (Main Menu):
```csharp
PlayerPrefs.SetString("PlayerName", AuthManager.Instance.Current_Name);
PlayerPrefs.SetString("PlayerUsername", AuthManager.Instance.Current_Username);
```

### From Lobby to Game:
```csharp
PlayerPrefs.SetInt("TaskCount", taskCount);
PlayerPrefs.SetString("DisasterType", "Flood");
PlayerPrefs.GetInt("GameDuration", duration);
```

## Player Display in Lobby

Players are displayed as:
```
INSTRUCTOR: JohnDoe
TRAINEE: JaneSmith  
TRAINEE: BobJones
```

Colors:
- Instructor = Yellow
- Trainee = White

## Important Notes

### Network Persistence
- `NetworkLobbyManager` persists across all scenes
- Network connection maintained from Main Menu → Lobby → Game
- Don't destroy NetworkManager between scenes

### Role Restrictions
- **Instructor:**
  - Can generate lobby codes
  - Can host sessions
  - Can modify game settings
  - Can start the game
  
- **Trainee:**
  - Can join with lobby code
  - Cannot modify settings
  - Waits for instructor to start
  
- **Super Admin:**
  - Manages accounts only
  - Not involved in gameplay

### Scene Loading
- Instructor uses `NetworkManager.SceneManager.LoadScene()`
- All clients load the scene automatically
- Game settings sync via PlayerPrefs

## Testing Procedure

### Test 1: Instructor Creates Lobby
1. Run in Unity Editor
2. Login as instructor
3. Generate lobby code
4. Verify code appears
5. Continue to lobby
6. Verify player list shows "INSTRUCTOR: [your name]"
7. Configure game settings
8. Start game
9. Verify game scene loads

### Test 2: Trainee Joins Lobby
1. Build executable
2. Run executable
3. Login as trainee
4. Enter lobby code from instructor
5. Join lobby
6. Verify auto-transition to lobby scene
7. Verify player list shows both players
8. Wait for instructor to start
9. Verify game scene loads

### Test 3: Multiple Trainees
1. Repeat Test 2 with multiple executables
2. Verify all appear in player list
3. Verify all load into game together

## Troubleshooting

**Problem:** Player list doesn't show names
- Check AuthManager is setting Current_Name and Current_Username
- Check PlayerPrefs are being set before joining
- Check NetworkPlayer is on player prefab

**Problem:** Can't transition from Main Menu to Lobby
- Check scene name matches in MainMenuLobbyController
- Check lobby scene is in Build Settings
- Check NetworkLobbyManager exists and persists

**Problem:** Trainee can see game settings
- Check LobbySceneManager is properly detecting role
- Check gameSettingsPanel.SetActive(isInstructor)

**Problem:** Game doesn't start for trainees
- Check instructor is using NetworkManager.SceneManager.LoadScene()
- Check all scenes are in Build Settings
- Check NetworkManager has scene management enabled

## File Checklist

### New Files Created:
- [x] `NetworkLobbyManager.cs` (updated with DontDestroyOnLoad)
- [x] `MainMenuLobbyController.cs` (Main Menu scene)
- [x] `LobbySceneManager.cs` (Lobby scene)
- [x] `NetworkPlayer.cs` (updated with username sync)
- [x] `NetworkString.cs` (struct for string serialization)

### Scenes Required:
- [ ] Main Menu Scene (with login + lobby code UI)
- [ ] Lobby Scene (with game settings + player list)
- [ ] Earthquake Scene
- [ ] Flood Scene
- [ ] TestKen Scene

### Prefabs Required:
- [ ] Player Prefab (with NetworkObject + NetworkPlayer)
- [ ] Player List Item Prefab (UI panel with Text)

## Next Steps

1. Setup Main Menu Scene UI
2. Setup Lobby Scene UI
3. Test locally with editor + build
4. Integrate Unity Relay for internet play
5. Add polish (animations, sounds, etc.)

---

**Updated:** November 16, 2025  
**Version:** 2.0 - Three Scene Flow  
**Status:** Ready for Implementation
