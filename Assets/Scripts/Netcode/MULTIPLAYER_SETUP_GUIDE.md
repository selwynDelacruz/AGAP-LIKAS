# AGAP-LIKAS Multiplayer Lobby Setup Guide

## Overview
This guide explains how to set up and use the multiplayer lobby system using Unity Netcode for GameObjects.

## Components Created

### 1. NetworkLobbyManager.cs
- **Location**: `Assets\Scripts\Netcode\NetworkLobbyManager.cs`
- **Purpose**: Manages multiplayer lobby state and synchronizes game settings across all connected players
- **Key Features**:
  - Syncs task count, duration, and disaster type across network
  - Tracks connected player count
  - Only host can modify settings and start the game
  - Automatically loads the selected disaster scene for all players

### 2. EnhancedNetworkUI.cs
- **Location**: `Assets\Scripts\Netcode\EnhancedNetworkUI.cs`
- **Purpose**: Provides UI for hosting/joining games and displays lobby information
- **Key Features**:
  - Host/Client/Disconnect buttons
  - Displays current lobby settings
  - Shows connected player count
  - Start game button (host only)
  - Connection status display

### 3. NetworkConnectionManager.cs
- **Location**: `Assets\Scripts\Netcode\NetworkConnectionManager.cs`
- **Purpose**: Manages network connection state and provides helper methods
- **Key Features**:
  - Singleton pattern for easy access
  - Connection state management
  - Scene transition handling
  - Debug information display (editor only)

### 4. Updated LobbyManager.cs
- **Purpose**: Integrates existing lobby UI with multiplayer system
- **Changes**:
  - Added multiplayer mode toggle
  - Syncs settings with NetworkLobbyManager when host
  - Supports both single-player and multiplayer modes

## Scene Setup Instructions

### Step 1: Lobby Scene Setup

1. **Create or Open Lobby Scene**
   - Create a new scene called "Lobby" or use your existing lobby scene

2. **Add NetworkManager**
   - Create an empty GameObject called "NetworkManager"
   - Add the `NetworkManager` component (Unity Netcode)
   - Add the `UnityTransport` component (for networking)
   - Configure transport settings:
     - Address: 127.0.0.1 (for local testing) or server IP
     - Port: 7777

3. **Add NetworkLobbyManager**
   - Create an empty GameObject called "NetworkLobbyManager"
   - Add the `NetworkLobbyManager` script
   - Add as a NetworkObject (Add Component ? NetworkObject)
   - Set "Max Players" to your desired limit (default: 4)

4. **Add NetworkConnectionManager**
   - Create an empty GameObject called "NetworkConnectionManager"
   - Add the `NetworkConnectionManager` script
   - Configure scene names:
     - Lobby Scene Name: "Lobby"
     - Main Menu Scene Name: "MainMenu"

5. **Set Up UI**
   
   Create a Canvas if you don't have one, then add:

   **Connection Panel**:
   - Host Button
   - Client Button
   - Disconnect Button
   - IP Address Input Field (optional, for joining specific servers)
   
   **Lobby Info Panel** (initially hidden):
   - Task Count Text
   - Duration Text
   - Disaster Type Text
   - Player Count Text
   - Connection Status Text
   
   **Host Controls Panel** (initially hidden, shown only to host):
   - Start Game Button

6. **Configure EnhancedNetworkUI**
   - Create an empty GameObject called "NetworkUI"
   - Add the `EnhancedNetworkUI` script
   - Assign all UI elements in the Inspector:
     - Connection Buttons (Host, Client, Disconnect)
     - Lobby Info Panel and all text fields
     - Host Controls Panel
     - IP Address Input (optional)

7. **Configure Existing LobbyManager**
   - Find your existing LobbyManager GameObject
   - In the Inspector, check "Is Multiplayer" checkbox
   - The LobbyManager will now work with the network system

### Step 2: NetworkManager Configuration

1. **Network Prefabs List**
   - In NetworkManager, add your player prefab to the "Network Prefabs" list
   - Add any other networked objects you'll spawn

2. **Scene Management**
   - In NetworkManager ? Scene Management:
     - Enable "Enable Scene Management"
     - This allows the host to load scenes for all clients

### Step 3: Testing

#### Local Testing (Same Computer)
1. Build the project
2. Run the build once as Host
3. Run the Unity Editor and click Client
4. Both should connect and see lobby info

#### Network Testing (Different Computers)
1. Host needs to:
   - Know their local IP address
   - Configure firewall to allow port 7777
   - Start as Host
2. Client needs to:
   - Enter host's IP address in the input field
   - Click Client button

## Usage Flow

### For Host (Instructor):
1. Open Lobby scene
2. Click "Host" button
3. Configure game settings (task count, duration, disaster type)
4. Wait for trainee(s) to connect
5. When ready, click "Start Game"
6. Game scene loads for all connected players

### For Client (Trainee):
1. Open Lobby scene
2. Enter host's IP address (if not on same network)
3. Click "Client" button
4. Wait for host to start game
5. View lobby settings (read-only)
6. Game starts when host clicks "Start Game"

## Integration with Existing Game Flow

### PlayerPrefs Integration
The lobby system automatically updates PlayerPrefs with selected settings:
- `TaskCount`: Number of tasks
- `DisasterType`: Selected disaster scenario
- `GameDuration`: Game duration in seconds

These are used by:
- `VictimSpawner.cs` - Reads TaskCount to spawn victims
- Other game systems that need configuration data

### Scene Loading
When the host starts the game:
1. Settings are saved to PlayerPrefs
2. NetworkManager loads the disaster scene for all clients
3. Each client's game scene reads from PlayerPrefs
4. Game starts synchronized for all players

## Troubleshooting

### "NetworkManager.Singleton is null"
- Ensure you have a NetworkManager GameObject in the scene
- Check that it has the NetworkManager component

### "Only the host can start the game"
- This is normal - only the player who clicked "Host" can start
- Clients see lobby info but cannot start the game

### Players can't connect
- Check firewall settings
- Verify port 7777 is open
- Ensure correct IP address is entered
- Try local testing first (127.0.0.1)

### Settings not syncing
- Ensure NetworkLobbyManager is added as a NetworkObject
- Check that "Is Multiplayer" is enabled in LobbyManager
- Verify host is changing settings, not clients

## Next Steps

To fully integrate multiplayer into your game:

1. **Player Spawning**
   - Create a player prefab with NetworkObject component
   - Add it to NetworkManager's Network Prefabs list
   - Spawn players when scene loads

2. **Victim Spawning**
   - Update VictimSpawner to work with network
   - Only host should spawn victims
   - Use NetworkObject for victims if they need to be synced

3. **Game State Management**
   - Create a NetworkGameManager for in-game state
   - Sync timer, score, completion status
   - Handle game end conditions

4. **Player Interactions**
   - Use RPCs (Remote Procedure Calls) for actions
   - Sync player positions and animations
   - Handle victim rescue interactions

## Code References

### Starting a Host
```csharp
NetworkManager.Singleton.StartHost();
```

### Starting a Client
```csharp
NetworkManager.Singleton.StartClient();
```

### Checking Connection State
```csharp
if (NetworkManager.Singleton.IsConnectedClient)
{
    // Connected
}
```

### Getting Lobby Settings (Any Client)
```csharp
int taskCount = NetworkLobbyManager.Instance.GetTaskCount();
string disaster = NetworkLobbyManager.Instance.GetDisasterType();
int duration = NetworkLobbyManager.Instance.GetDuration();
```

### Updating Lobby Settings (Host Only)
```csharp
if (NetworkLobbyManager.Instance.IsServer)
{
    NetworkLobbyManager.Instance.UpdateLobbySettings(taskCount, duration, disasterType);
}
```

## Additional Resources

- [Unity Netcode Documentation](https://docs-multiplayer.unity3d.com/netcode/current/about/)
- [Netcode for GameObjects Samples](https://github.com/Unity-Technologies/com.unity.multiplayer.samples.coop)
- [Unity Transport Package](https://docs.unity3d.com/Packages/com.unity.transport@latest)

## Support

For questions or issues with the multiplayer system, please check:
1. Unity Console for error messages
2. Network Debug Info (shown in editor top-left)
3. This README's troubleshooting section
