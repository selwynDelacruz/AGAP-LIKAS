# Network Lobby System Architecture

## System Flow Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                         INSTRUCTOR FLOW                              │
└─────────────────────────────────────────────────────────────────────┘

    [Instructor Opens Lobby Scene]
              ↓
    [LobbyManager/LobbyUIController]
              ↓
    [Clicks "Generate Code"]
              ↓
    [NetworkLobbyManager.GenerateLobbyCode()]
              ↓
    [Displays Code: "A3X7B2"]
              ↓
    [Shares Code with Trainees]
              ↓
    [Clicks "Start Host"]
              ↓
    [NetworkLobbyManager.StartHostWithLobbyCode()]
              ↓
    [NetworkManager.Singleton.StartHost()]
              ↓
    [Waiting for Trainees...]
              ↓
    [OnClientConnected Event] ──→ [Update Player Count]
              ↓
    [All Players Ready]
              ↓
    [NetworkSceneManager.LoadNetworkScene("GameScene")]
              ↓
    [Game Starts - All Clients Load Synchronized]


┌─────────────────────────────────────────────────────────────────────┐
│                          TRAINEE FLOW                                │
└─────────────────────────────────────────────────────────────────────┘

    [Trainee Opens Lobby Scene]
              ↓
    [LobbyManager/LobbyUIController]
              ↓
    [Receives Code from Instructor]
              ↓
    [Enters Code: "A3X7B2"]
              ↓
    [Clicks "Join Lobby"]
              ↓
    [NetworkLobbyManager.JoinLobbyWithCode("A3X7B2")]
              ↓
    [Validates Code Format]
              ↓
    [NetworkManager.Singleton.StartClient()]
              ↓
    [Connecting to Host...]
              ↓
    [OnLobbyJoinAttempt Event] ──→ [Success/Failure Feedback]
              ↓
    [Connected to Lobby]
              ↓
    [Waiting for Host to Start Game...]
              ↓
    [Receives Scene Load from Host]
              ↓
    [Game Starts - Synchronized with Host]
```

## Component Architecture

```
┌────────────────────────────────────────────────────────────────────┐
│                        UNITY SCENE                                  │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │  NetworkManager (Unity Netcode)                              │  │
│  │  - Manages connections                                       │  │
│  │  - Spawns network objects                                    │  │
│  │  - Scene management                                          │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                            ↓  ↑                                     │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │  NetworkLobbyManager (Custom)                                │  │
│  │  - Generates lobby codes                                     │  │
│  │  - Manages lobby state                                       │  │
│  │  - Handles host/client logic                                 │  │
│  │  - Fires events for UI updates                               │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                    ↓  ↑              ↓  ↑                           │
│         ┌──────────────────┐   ┌──────────────────┐                │
│         │  LobbyManager    │   │ LobbyUIController│                │
│         │  (Main Lobby)    │   │  (UI Only)       │                │
│         │  - Task config   │   │  - Clean UI      │                │
│         │  - Disaster sel  │   │  - Events        │                │
│         │  - Duration      │   │  - Role detect   │                │
│         │  - Network UI    │   │  - Status        │                │
│         └──────────────────┘   └──────────────────┘                │
│                    ↓                      ↓                         │
│              ┌────────────────────────────────┐                     │
│              │       UI Elements              │                     │
│              │  - Buttons                     │                     │
│              │  - Text fields                 │                     │
│              │  - Input fields                │                     │
│              │  - Panels                      │                     │
│              └────────────────────────────────┘                     │
│                                                                      │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │  NetworkSceneManager (Optional)                              │  │
│  │  - Scene persistence                                         │  │
│  │  - Network scene loading                                     │  │
│  │  - Session info                                              │  │
│  └──────────────────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────────────┐
│                      PLAYER PREFAB                                  │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │  NetworkObject (Unity Netcode)                               │  │
│  │  - Network ID                                                │  │
│  │  - Ownership                                                 │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                            ↓                                        │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │  NetworkPlayer (Custom)                                      │  │
│  │  - Player role (instructor/trainee)                          │  │
│  │  - Team assignment                                           │  │
│  │  - Player name                                               │  │
│  │  - Network messaging                                         │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                            ↓                                        │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │  Your Custom Player Scripts                                  │  │
│  │  - Movement                                                  │  │
│  │  - Interactions                                              │  │
│  │  - Game logic                                                │  │
│  └──────────────────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────────────┘
```

## Event Flow

```
NetworkLobbyManager Events:
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│  OnLobbyCodeGenerated(string code)                         │
│      ↓                                                      │
│      └─→ LobbyManager updates UI with code                 │
│      └─→ LobbyUIController displays code                   │
│      └─→ Enable "Start Host" button                        │
│                                                             │
│  OnLobbyJoinAttempt(bool success, string message)          │
│      ↓                                                      │
│      └─→ Update connection status text                     │
│      └─→ Show success/error message                        │
│      └─→ Transition to connected UI                        │
│                                                             │
│  OnPlayersCountChanged(int count)                          │
│      ↓                                                      │
│      └─→ Update player count display                       │
│      └─→ Update RoomManager.playersCount                   │
│      └─→ Log player join/leave events                      │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## Data Flow

```
Instructor (Host):
┌─────────────┐     ┌──────────────────┐     ┌─────────────┐
│   Input:    │ ──→ │   Processing:    │ ──→ │   Output:   │
│             │     │                  │     │             │
│ - Generate  │     │ Create random    │     │ Lobby Code  │
│   CodeBtn   │     │ 6-char code      │     │ "A3X7B2"    │
│             │     │                  │     │             │
│ - Start     │     │ NetworkManager   │     │ Hosting on  │
│   Host Btn  │     │ .StartHost()     │     │ Port 7777   │
│             │     │                  │     │             │
│ - Game      │     │ Store settings   │     │ PlayerPrefs │
│   Settings  │     │ in PlayerPrefs   │     │ saved       │
└─────────────┘     └──────────────────┘     └─────────────┘

Trainee (Client):
┌─────────────┐     ┌──────────────────┐     ┌─────────────┐
│   Input:    │ ──→ │   Processing:    │ ──→ │   Output:   │
│             │     │                  │     │             │
│ - Code      │     │ Validate code    │     │ Code OK/    │
│   "A3X7B2"  │     │ format (6 chars) │     │ Invalid     │
│             │     │                  │     │             │
│ - Join      │     │ NetworkManager   │     │ Connect to  │
│   Lobby Btn │     │ .StartClient()   │     │ Host        │
│             │     │                  │     │             │
│ - Wait      │     │ Receive settings │     │ Sync state  │
│   for Host  │     │ from host        │     │ with host   │
└─────────────┘     └──────────────────┘     └─────────────┘
```

## Network Communication

```
┌─────────────────┐                           ┌─────────────────┐
│   INSTRUCTOR    │                           │     TRAINEE     │
│     (Host)      │                           │    (Client)     │
└────────┬────────┘                           └────────┬────────┘
         │                                             │
         │  1. Start Host (Port 7777)                 │
         ├────────────────────────────────────────────┤
         │                                             │
         │                                             │  2. Start Client
         │                                             │     Connect to
         │                                             │     127.0.0.1:7777
         │  ←──────────────────────────────────────── │
         │         Connection Request                  │
         │                                             │
         │  ────────────────────────────────────────→ │
         │         Connection Accepted                 │
         │         Client ID: 1                        │
         │                                             │
         │  ←────────────────────────────────────────→│
         │         Spawn NetworkObjects                │
         │         (Players, etc.)                     │
         │                                             │
         │  ←────────────────────────────────────────→│
         │         Sync NetworkVariables               │
         │         (Player data, team, etc.)           │
         │                                             │
         │  3. Load Game Scene                         │
         │  ────────────────────────────────────────→ │
         │         NetworkSceneManager                 │
         │         .LoadScene("GameScene")             │
         │                                             │
         │  ←────────────────────────────────────────→│
         │         Both load same scene                │
         │         synchronously                       │
         │                                             │
         │  ←────────────────────────────────────────→│
         │         Continuous state sync               │
         │         during gameplay                     │
         │                                             │
```

## File Organization

```
Assets/Scripts/
├── LobbyManager.cs                 [Main lobby controller]
├── RoomManager.cs                  [Room/session manager]
├── Netcode/
│   ├── NetworkLobbyManager.cs      [★ Lobby code system]
│   ├── NetworkPlayer.cs            [★ Player sync]
│   ├── NetworkSceneManager.cs      [★ Scene management]
│   ├── LobbyUIController.cs        [★ UI handler]
│   ├── NetworkUIButtons.cs         [Updated buttons]
│   ├── LobbySystemExample.cs       [★ Usage examples]
│   ├── README.md                   [★ Full documentation]
│   ├── QUICK_SETUP.md              [★ Quick guide]
│   ├── IMPLEMENTATION_SUMMARY.md   [★ Summary]
│   └── ARCHITECTURE.md             [★ This file]

★ = Newly created/updated for this implementation
```

## Key Design Decisions

1. **Singleton Pattern**: NetworkLobbyManager uses Instance for easy access
2. **Event-Driven**: UI updates through events, not polling
3. **Role-Based**: Automatic detection from RoomManager/PlayerPrefs
4. **Modular**: Each component has single responsibility
5. **Extensible**: Easy to add features (relay, matchmaking, etc.)
6. **Fallback**: Works with or without custom managers

## Thread Safety

- All network operations run on main thread
- NetworkVariables are thread-safe by design
- Events fire on main thread
- UI updates are safe from callbacks

## Performance Characteristics

- **Code Generation**: O(1) - instant
- **Connection**: O(1) - single handshake
- **Player Sync**: O(n) - scales with player count
- **Memory**: Minimal - ~1KB per lobby
- **Bandwidth**: Low - only deltas sent

---

This architecture supports 2-10 players efficiently.
For larger scale, consider dedicated servers.
