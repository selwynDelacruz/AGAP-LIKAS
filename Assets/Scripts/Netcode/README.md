# Network Lobby System with Unity Netcode

This system implements a lobby code-based multiplayer system using Unity Netcode for GameObjects, where instructors can generate lobby codes and trainees can join using those codes.

## Overview

The network lobby system consists of the following components:

1. **NetworkLobbyManager** - Core lobby management (code generation, hosting, joining)
2. **LobbyManager** - Integration with existing lobby UI
3. **LobbyUIController** - Dedicated UI controller for lobby interactions
4. **NetworkUIButtons** - Simple UI buttons for network operations
5. **NetworkPlayer** - Player network synchronization

## Features

✅ **Role-Based Access**
- Instructors can generate lobby codes and host sessions
- Trainees can join using lobby codes
- Role detection from RoomManager or PlayerPrefs

✅ **Lobby Code System**
- 6-character alphanumeric codes (configurable)
- Automatic code generation
- Code validation

✅ **Network Synchronization**
- Player data synchronization
- Player count tracking
- Connection status updates

✅ **UI Integration**
- Automatic UI panel switching based on role
- Real-time status updates
- Player count display

## Setup Instructions

### 1. Prerequisites

Ensure you have Unity Netcode for GameObjects installed:
- Window > Package Manager > Unity Registry
- Search for "Netcode for GameObjects"
- Install version 1.0.0 or higher

### 2. Scene Setup

#### A. Create NetworkManager GameObject
1. Create an empty GameObject in your scene
2. Rename it to "NetworkManager"
3. Add Component > Netcode > NetworkManager
4. Add Component > Netcode > Unity Transport
5. Configure NetworkManager:
   - Set "Transport" to Unity Transport component
   - Add your player prefab to "Network Prefabs" list

#### B. Create NetworkLobbyManager GameObject
1. Create an empty GameObject in your scene
2. Rename it to "NetworkLobbyManager"
3. Add the `NetworkLobbyManager` script
4. Configure settings:
   - Lobby Code Length: 6 (default)

#### C. Setup UI Elements

**For Instructor UI:**
```
InstructorPanel/
├── GenerateLobbyCodeButton (Button)
├── LobbyCodeDisplay (TextMeshPro)
├── StartHostButton (Button)
└── CopyCodeButton (Button) [Optional]
```

**For Trainee UI:**
```
TraineePanel/
├── LobbyCodeInput (TMP_InputField)
└── JoinButton (Button)
```

**Shared UI:**
```
SharedPanel/
├── StatusText (TextMeshPro)
├── PlayerCountText (TextMeshPro)
└── DisconnectButton (Button)
```

### 3. Script Configuration

#### A. LobbyManager Configuration
1. Add/Update `LobbyManager` component on your lobby scene GameObject
2. Assign UI references:
   - **Instructor References:**
     - generateLobbyCodeButton
     - lobbyCodeDisplayText
     - startHostButton
     - instructorPanel
   - **Trainee References:**
     - lobbyCodeInputField
     - joinLobbyButton
     - traineePanel
   - **Shared References:**
     - connectionStatusText
     - playerCountText

#### B. LobbyUIController Configuration (Alternative)
If using the dedicated UI controller:
1. Create a GameObject named "LobbyUI"
2. Add `LobbyUIController` component
3. Assign all UI references as described above

### 4. Player Prefab Setup

1. Create or select your player prefab
2. Add `NetworkObject` component (required by Netcode)
3. Add `NetworkPlayer` script
4. Configure:
   - Set player name if desired
5. Add the prefab to NetworkManager's "Network Prefabs" list

### 5. Testing

#### Local Testing (Single Machine)
1. **Build Settings:**
   - Add your lobby scene to Build Settings
   - Build a standalone executable

2. **Test Process:**
   - Run the Unity Editor (Instructor)
   - Run the built executable (Trainee)
   
3. **Instructor Steps:**
   - Click "Generate Lobby Code"
   - Note the generated code
   - Click "Start Host"

4. **Trainee Steps:**
   - Enter the lobby code
   - Click "Join Lobby"

#### Network Testing (Multiple Machines)
For production deployment, you'll need to integrate with Unity Relay Service or configure direct IP connections.

## Production Deployment

### Option 1: Unity Relay Service (Recommended)

Unity Relay allows players to connect without port forwarding:

1. **Setup Unity Services:**
   ```
   Window > General > Services
   Link your project to Unity Cloud
   Enable Relay service
   ```

2. **Install Relay Package:**
   ```
   Window > Package Manager
   Install "Relay" package
   ```

3. **Update NetworkLobbyManager:**
   Uncomment and implement the Relay integration sections in:
   - `ConfigureTransportForHost()`
   - `ConfigureTransportForClient()`

### Option 2: Direct IP Connection

For local network or VPN setups:

1. Update `ConfigureTransportForHost()`:
   ```csharp
   transport.SetConnectionData("0.0.0.0", 7777);
   ```

2. Update `ConfigureTransportForClient()`:
   ```csharp
   string ipAddress = GetIPFromLobbyCode(lobbyCode);
   transport.SetConnectionData(ipAddress, 7777);
   ```

3. Implement a mapping service between lobby codes and IP addresses

## Usage Examples

### Example 1: Generate and Host Lobby (Instructor)

```csharp
// Generate lobby code
string code = NetworkLobbyManager.Instance.GenerateLobbyCode();
Debug.Log($"Lobby Code: {code}");

// Start hosting
bool success = NetworkLobbyManager.Instance.StartHostWithLobbyCode();
```

### Example 2: Join Lobby (Trainee)

```csharp
// Join with code
string enteredCode = "ABC123";
bool success = NetworkLobbyManager.Instance.JoinLobbyWithCode(enteredCode);
```

### Example 3: Subscribe to Events

```csharp
void Start()
{
    NetworkLobbyManager.Instance.OnLobbyCodeGenerated += OnCodeGenerated;
    NetworkLobbyManager.Instance.OnPlayersCountChanged += OnPlayerCountChanged;
}

void OnCodeGenerated(string code)
{
    Debug.Log($"New lobby code: {code}");
}

void OnPlayerCountChanged(int count)
{
    Debug.Log($"Players in lobby: {count}");
}
```

## API Reference

### NetworkLobbyManager

**Methods:**
- `GenerateLobbyCode()` - Generates a unique lobby code (Instructor only)
- `StartHostWithLobbyCode()` - Starts hosting with the generated code
- `JoinLobbyWithCode(string code)` - Joins a lobby as trainee
- `LeaveLobby()` - Disconnects from the current lobby
- `GetCurrentLobbyCode()` - Returns the current lobby code
- `GetConnectedPlayersCount()` - Returns the number of connected players

**Events:**
- `OnLobbyCodeGenerated(string code)` - Fired when a lobby code is generated
- `OnLobbyJoinAttempt(bool success, string message)` - Fired when joining is attempted
- `OnPlayersCountChanged(int count)` - Fired when player count changes

### NetworkPlayer

**Properties:**
- `IsInstructor` - Whether this player is an instructor
- `Team` - The player's team ID

**Methods:**
- `SetPlayerName(string name)` - Sets the player's display name
- `GetPlayerName()` - Gets the player's display name
- `SendMessage(string message)` - Broadcasts a message to all players

## Troubleshooting

### Issue: Cannot connect between devices
**Solution:** 
- Ensure both devices are on the same network for local testing
- Check firewall settings
- For production, use Unity Relay Service

### Issue: Lobby code not generating
**Solution:**
- Verify the user is marked as "instructor" in PlayerPrefs or RoomManager
- Check NetworkLobbyManager Instance is not null
- Review console for error messages

### Issue: Client cannot join lobby
**Solution:**
- Verify lobby code is correct (6 characters, alphanumeric)
- Ensure host has started before client attempts to join
- Check NetworkManager configuration

### Issue: Players not synchronized
**Solution:**
- Verify NetworkPlayer script is on player prefab
- Ensure NetworkObject component is present
- Check player prefab is in NetworkManager's Network Prefabs list

## Best Practices

1. **Always check for null references** before accessing NetworkLobbyManager.Instance
2. **Subscribe to events** to handle async network operations
3. **Validate lobby codes** on the client side before attempting to join
4. **Handle connection failures** gracefully with user feedback
5. **Test with multiple clients** before production deployment
6. **Use Unity Relay** for production to avoid NAT/firewall issues
7. **Implement timeout logic** for connection attempts
8. **Store lobby codes** securely if implementing persistence

## Future Enhancements

Potential additions to the system:

- [ ] Lobby code expiration
- [ ] Password-protected lobbies
- [ ] Lobby browser/list
- [ ] Team selection UI
- [ ] Ready-up system
- [ ] Host migration
- [ ] Reconnection support
- [ ] Anti-cheat measures
- [ ] Matchmaking integration

## Credits

Created for AGAP-LIKAS using Unity Netcode for GameObjects.

## License

This code is part of the AGAP-LIKAS project.
