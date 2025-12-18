# Lobby System Architecture & Flow

## ??? System Architecture

```
???????????????????????????????????????????????????????????
?                   LOBBY SYSTEM FLOW                      ?
???????????????????????????????????????????????????????????

1. MAIN MENU (Authentication)
   ?? User logs in as Instructor or Trainee
   ?? Role stored in PlayerPrefs: "Type_Of_User"
   ?? Click "Play Game" ? Load LobbyMenu

2. LOBBY MENU (Create or Join)
   ?? INSTRUCTOR (Host):
   ?  ?? Click "Create Lobby"
   ?  ?? Generate Lobby Code (e.g., "GAME42")
   ?  ?? Start NetworkManager as Host
   ?  ?? Start LobbyBroadcaster (UDP port 7778)
   ?  ?? Load LobbyRoom scene
   ?
   ?? TRAINEE (Client):
      ?? Enter Lobby Code
      ?? Click "Join Lobby"
      ?? Start LobbyScanner (listen on port 7778)
      ?? Find Host IP via LAN broadcast
      ?? Start NetworkManager as Client
      ?? Auto-follow to LobbyRoom scene

3. LOBBY ROOM (Configuration & Waiting)
   ?? HOST View:
   ?  ?? Configure Task Count (5-8)
   ?  ?? Select Disaster Type (Flood/Earthquake/TestKen)
   ?  ?? Select Duration (5/8/10 minutes)
   ?  ?? Settings synced to all clients via NetworkVariables
   ?  ?? Click "START GAME" ? Load Game Scene
   ?
   ?? CLIENT View:
      ?? Read-only display of Host's settings
      ?? Show lobby status (code, players, etc.)
      ?? Wait for Host to start game

4. GAME SCENE (Disaster Simulation)
   ?? Load selected disaster scene
   ?? Spawn victims based on TaskCount
   ?? Start timer based on Duration
   ?? Players rescue victims
   ?? End game when time runs out or all victims saved

5. RESULT SCENE (Summary)
   ?? Display final score
   ?? Save score to Firebase
   ?? Return to Main Menu or Play Again
```

---

## ?? Network Architecture

```
????????????????????????????????????????????????????????
?            NETWORK COMMUNICATION FLOW                 ?
????????????????????????????????????????????????????????

HOST (Instructor PC)                    CLIENT (Trainee PC)
?????????????????                      ??????????????????

[LobbyMenu Scene]                      [LobbyMenu Scene]
     ?                                      ?
     ?? Generate Code: "GAME42"            ?? Enter Code: "GAME42"
     ?                                      ?
     ?? Start NetworkManager               ?? Start LobbyScanner
     ?  as Host (0.0.0.0:7777)             ?  (Listen on port 7778)
     ?                                      ?
     ?? Start LobbyBroadcaster             ?
     ?  (Broadcast on port 7778)  ???????????? Receives broadcast:
     ?  Message: "GAME42|192.168.1.100|7777"  ?  "GAME42|192.168.1.100|7777"
     ?                                      ?
     ?                                      ?? Parse IP: 192.168.1.100
     ?                                      ?
     ?                                      ?? Start NetworkManager
     ?                                      ?  as Client (192.168.1.100:7777)
     ?                                      ?
     ??????????????????????????????????????????? Connection Request
     ?
     ?? Accept Connection ??????????????????????
     ?                                      ?
     ?? Load LobbyRoom Scene              ?
     ?  via NetworkSceneManager            ?
     ?                                      ?
     ?  ??????????????????????????????????  ?  Auto-follow scene load
     ?                                      ?
[LobbyRoom Scene]                      [LobbyRoom Scene]
     ?                                      ?
     ?? NetworkVariables:                  ?
     ?  - taskCount.Value = 5              ???? Receive NetworkVariable
     ?  - disasterIndex.Value = 2          ?     updates (synced)
     ?  - durationIndex.Value = 0          ?
     ?                                      ?
     ?? Host changes settings:             ?
     ?  taskCount.Value = 7  ????????????  ?? UI updates to show 7
     ?  disasterIndex.Value = 0  ???????   ?? UI updates to show Flood
     ?                                      ?
     ?? Click "START GAME"                 ?
     ?  NetworkSceneManager.LoadScene      ?
     ?  ("Flood")  ?????????????????????   ?? Auto-follow to Flood scene
     ?                                      ?
[Flood Scene]                           [Flood Scene]
     ?                                      ?
     ?? Networked gameplay               ?? Networked gameplay
     ?? Synced events & state            ?? Synced events & state
```

---

## ?? Component Responsibilities

### LobbyCodeGenerator
```
Purpose: Generate and validate lobby codes
Type: Static utility class
Thread: Main thread
```
- Generates 6-character codes (e.g., "GAME42")
- Excludes confusing characters (0, O, 1, I, L)
- Validates code format

### LobbyBroadcaster
```
Purpose: Broadcast lobby information on LAN
Type: MonoBehaviour
Thread: Background thread (UDP broadcast loop)
Runs on: Host only
```
- Sends UDP broadcasts every 1 second
- Message format: "LOBBYCODE|IP|PORT"
- Broadcast address: 255.255.255.255:7778
- Stops when host shuts down

### LobbyScanner
```
Purpose: Scan LAN for lobbies
Type: MonoBehaviour
Thread: Background thread (UDP listen loop)
Runs on: Client only
```
- Listens on UDP port 7778
- Timeout: 5 seconds
- Filters broadcasts by lobby code
- Callbacks executed on main thread via dispatcher

### UnityMainThreadDispatcher
```
Purpose: Execute actions on Unity's main thread
Type: MonoBehaviour (Singleton, DontDestroyOnLoad)
Thread: Main thread only
```
- Required because network callbacks run on background threads
- Queues actions to execute in Update()
- Thread-safe enqueueing with lock

### NetworkManagerInitializer
```
Purpose: Initialize and manage NetworkManager
Type: MonoBehaviour (Singleton, DontDestroyOnLoad)
Thread: Main thread
```
- Spawns NetworkManager prefab at runtime
- Ensures only one instance exists
- Provides helper methods for transport configuration
- Persists across all scenes

### LobbyMenuManager
```
Purpose: Manage Lobby Menu UI and logic
Type: MonoBehaviour
Thread: Main thread
Runs in: LobbyMenu scene only
```
- Shows Create or Join panel based on user role
- Handles "Create Lobby" flow (instructor)
- Handles "Join Lobby" flow (trainee)
- Integrates LobbyBroadcaster/Scanner
- Loads LobbyRoom scene after connection

### LobbyRoomManager
```
Purpose: Manage Lobby Room UI and game settings
Type: NetworkBehaviour (requires NetworkObject)
Thread: Main thread
Runs in: LobbyRoom scene only
```
- NetworkVariables for synced settings:
  - taskCount (5-8)
  - disasterIndex (0=Flood, 1=Earthquake, 2=TestKen)
  - durationIndex (0=5min, 1=8min, 2=10min)
- Host can modify settings
- Clients see read-only view
- Saves settings to PlayerPrefs before game starts
- Loads game scene via NetworkSceneManager

---

## ?? Scene Lifecycle

### Scene 1: MainMenu
```
Lifecycle:
1. Load ? AuthManager authenticates user
2. Store role in PlayerPrefs
3. User clicks "Play Game"
4. Load LobbyMenu scene

Persistent Objects:
- NetworkManagerInitializer (DontDestroyOnLoad)
- UnityMainThreadDispatcher (DontDestroyOnLoad)
```

### Scene 2: LobbyMenu
```
Lifecycle:
1. Load ? LobbyMenuManager reads role from PlayerPrefs
2. Show appropriate UI (Create or Join)
3. User creates/joins lobby
4. NetworkManager starts (Host or Client)
5. Load LobbyRoom scene

Persistent Objects:
- NetworkManager (spawned, DontDestroyOnLoad)
- LobbyBroadcaster (if host, attached to LobbyMenuManager)
- NetworkManagerInitializer (still persistent)
- UnityMainThreadDispatcher (still persistent)
```

### Scene 3: LobbyRoom
```
Lifecycle:
1. Load ? LobbyRoomManager spawns (NetworkObject)
2. NetworkVariables sync settings to all clients
3. Host configures game settings
4. Host clicks "START GAME"
5. Save settings to PlayerPrefs
6. Load game scene (Flood/Earthquake/TestKen)

Persistent Objects:
- NetworkManager (still persistent)
- NetworkManagerInitializer (still persistent)
- UnityMainThreadDispatcher (still persistent)
- LobbyBroadcaster (still running on host)
```

### Scene 4: Game Scene (Flood/Earthquake/TestKen)
```
Lifecycle:
1. Load ? GameManager reads settings from PlayerPrefs
2. Spawn victims based on TaskCount
3. Start timer based on Duration
4. Networked gameplay
5. Game ends ? Load Result scene

Persistent Objects:
- NetworkManager (still persistent)
- Other persistent objects remain
```

### Scene 5: Result
```
Lifecycle:
1. Display results
2. Save score to Firebase
3. User clicks "Play Again" or "Main Menu"
4. If Main Menu: Shutdown NetworkManager
5. Load appropriate scene

Cleanup:
- NetworkManager.Shutdown() if returning to menu
- LobbyBroadcaster stops
```

---

## ?? Key Design Decisions

### 1. Why NetworkVariables?
- **Automatic syncing** across all clients
- **No manual RPCs** needed for simple data
- **Built-in change callbacks** for UI updates
- **Server-authoritative** (only host can change)

### 2. Why LAN Discovery instead of direct IP?
- **Better UX**: Users don't need to know IP addresses
- **User-friendly codes**: Easy to communicate verbally
- **Auto-discovery**: No manual network configuration
- **LAN-only**: Suitable for local co-located training

### 3. Why DontDestroyOnLoad for NetworkManager?
- **Persists connections** across scene changes
- **Maintains client-server relationship**
- **Automatic scene synchronization**
- **No reconnection needed** between scenes

### 4. Why separate Lobby Menu and Lobby Room?
- **Clear separation of concerns**:
  - LobbyMenu: Connection establishment
  - LobbyRoom: Game configuration
- **Better UX flow**
- **Easier to manage** role-based UI

### 5. Why PlayerPrefs for settings?
- **Simple data persistence**
- **Accessible across scenes**
- **No need for complex data structures**
- **Already used** in your existing codebase

---

## ?? Important Constraints

### 1. Network Constraints
- **LAN Only**: No internet/WAN support (by design)
- **Same Network**: All players must be on same WiFi/LAN
- **Port Forwarding**: Not needed for LAN
- **Firewall**: May block UDP broadcasts (needs allow rule)

### 2. Threading Constraints
- **Unity API**: Must be called on main thread
- **Network Callbacks**: Run on background threads
- **Solution**: Use UnityMainThreadDispatcher

### 3. Netcode Constraints
- **Scene Loading**: Only host/server can load scenes
- **Clients Auto-follow**: Clients automatically load same scene
- **NetworkBehaviour**: Requires NetworkObject component
- **NetworkVariables**: Only server can write (by default)

### 4. Timing Constraints
- **Broadcast Interval**: 1 second
- **Scan Timeout**: 5 seconds
- **Connection Timeout**: Default Netcode timeout

---

## ?? Security Considerations

### Current Implementation (LAN Training)
- ? Local network only (isolated)
- ? No sensitive data transmitted
- ? Code validation prevents basic errors

### Future Considerations (if extending to WAN)
- ?? Add encryption for data transmission
- ?? Implement lobby authentication
- ?? Add rate limiting for connection attempts
- ?? Validate all client inputs on server

---

## ?? Performance Considerations

### Network Traffic
- **Broadcast**: ~50 bytes every 1 second (minimal)
- **NetworkVariables**: ~12 bytes per change (minimal)
- **Scene Sync**: Handled by Netcode (optimized)

### Memory Usage
- **NetworkManager**: ~1-2 MB
- **Lobby Components**: <1 MB
- **Total Overhead**: Negligible for modern systems

### CPU Usage
- **Background Threads**: Minimal (sleep-based loops)
- **Main Thread**: Only UI updates and callbacks
- **Overall Impact**: <1% CPU usage for lobby system

---

This document provides a comprehensive understanding of the lobby system architecture. Refer to SETUP_GUIDE.md for step-by-step implementation instructions.
