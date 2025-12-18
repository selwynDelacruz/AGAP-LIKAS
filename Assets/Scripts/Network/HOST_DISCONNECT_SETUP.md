# Host Disconnect Handler Setup Guide

## Overview
When the instructor/host leaves or disconnects from the game, all connected clients (trainees) will automatically return to the lobby menu.

## How It Works

### Event Flow:
```
1. Instructor/Host disconnects or closes game
   ?
2. Server shuts down
   ?
3. HostDisconnectHandler detects server stopped
   ?
4. Clients wait 1 second (configurable delay)
   ?
5. Clients shutdown their NetworkManager
   ?
6. Clients load LobbyMenu scene
```

## Implementation

### Option 1: Automatic Setup (Recommended)
The `GameManager` will automatically add the `HostDisconnectHandler` to the NetworkManager if it's missing.

**No manual setup required!** Just ensure:
- ? `GameManager` exists in your gameplay scenes (TestKen, Flood, Earthquake)
- ? `autoAddDisconnectHandler` is checked in GameManager inspector (default: true)

### Option 2: Manual Setup
If you prefer manual control:

1. **Attach to NetworkManager:**
   - Select your `NetworkManager` GameObject (in DontDestroyOnLoad or scene)
   - Add Component ? `HostDisconnectHandler`

2. **Configure Settings:**
   - **Lobby Scene Name**: Name of your lobby scene (default: "LobbyMenu")
   - **Disconnect Delay**: Seconds to wait before returning (default: 1.0)
   - **Show Debug Logs**: Enable for testing (default: true)

## Configuration

### GameManager Settings

| Setting | Description | Default |
|---------|-------------|---------|
| Auto Add Disconnect Handler | Automatically adds HostDisconnectHandler if missing | ? Checked |

### HostDisconnectHandler Settings

| Setting | Description | Default |
|---------|-------------|---------|
| Lobby Scene Name | Scene to load on disconnect | "LobbyMenu" |
| Disconnect Delay | Wait time before returning (seconds) | 1.0 |
| Show Debug Logs | Enable detailed logging | ? Checked |

## Testing

### Test as Instructor (Host):
1. Start game as instructor/host
2. Have at least one trainee/client connect
3. **Close the game or press Stop in Unity Editor**
4. Observe client behavior

### Test as Trainee (Client):
1. Join game as trainee/client
2. Wait for host to disconnect
3. **Expected Behavior:**
   - See console log: `[HostDisconnectHandler] Server stopped!`
   - After 1 second delay
   - Return to LobbyMenu scene automatically

### Expected Console Logs (Client Side):

```
[HostDisconnectHandler] Subscribed to network disconnect events
[HostDisconnectHandler] Server stopped! IsHost: False
[HostDisconnectHandler] Handling disconnect: Server/Host has stopped. Returning to LobbyMenu in 1s
[HostDisconnectHandler] Returning to lobby: Server/Host has stopped
[HostDisconnectHandler] NetworkManager shutdown complete
[HostDisconnectHandler] Loading scene: LobbyMenu
```

## Advanced Usage

### Custom Disconnect Messages
You can show a UI message to players before returning to lobby:

```csharp
// In HostDisconnectHandler.cs, modify HandleHostDisconnect:
private void HandleHostDisconnect(string reason)
{
    // Show custom UI message
    if (UIManager.Instance != null)
    {
        UIManager.Instance.ShowMessage($"Host disconnected: {reason}\nReturning to lobby...");
    }
    
    // ... rest of the code
}
```

### Manual Return to Lobby Button
You can add a "Leave Game" button that calls the disconnect handler:

```csharp
// In your UI button handler:
public void OnLeaveGameButtonClicked()
{
    var disconnectHandler = FindObjectOfType<HostDisconnectHandler>();
    if (disconnectHandler != null)
    {
        disconnectHandler.ManuallyReturnToLobby();
    }
}
```

## Troubleshooting

### Issue: Clients not returning to lobby

**Possible Causes:**
1. ? `HostDisconnectHandler` not attached to NetworkManager
2. ? Scene name incorrect (check spelling: "LobbyMenu")
3. ? NetworkManager.Singleton is null

**Solutions:**
1. ? Check if `autoAddDisconnectHandler` is enabled in GameManager
2. ? Verify lobby scene name in inspector matches actual scene name
3. ? Enable `showDebugLogs` and check Unity Console for errors

### Issue: Clients disconnect immediately

**Possible Cause:**
- ? Host/server not properly started before clients connect

**Solution:**
- ? Ensure host calls `NetworkManager.Singleton.StartHost()` before clients connect

### Issue: Multiple disconnect handlers running

**Possible Cause:**
- ? HostDisconnectHandler attached multiple times

**Solution:**
- ? Check NetworkManager GameObject - should only have ONE HostDisconnectHandler
- ? Disable `autoAddDisconnectHandler` if you're manually adding it

## Integration with Existing Systems

### Works With:
- ? LobbyMenuManager
- ? PlayerSpawnManager
- ? NetworkUI
- ? GameManager role detection

### Scene Persistence:
- The `HostDisconnectHandler` is attached to `NetworkManager`
- NetworkManager persists with `DontDestroyOnLoad`
- Handler works across all gameplay scenes (TestKen, Flood, Earthquake)

## Events Subscribed:

| Event | Description | Fires When |
|-------|-------------|------------|
| `OnServerStopped` | Server shutdown | Host closes/disconnects |
| `OnClientDisconnectCallback` | Client disconnect | This client loses connection |

Both events trigger the return-to-lobby logic for clients.

## Summary

? **Automatic**: GameManager handles everything
? **Reliable**: Uses Unity Netcode's built-in disconnect events
? **Configurable**: Adjust delay and scene name in inspector
? **Debug-Friendly**: Enable logs to track behavior
? **Safe**: Properly shuts down NetworkManager before scene transition

## Related Files

- `Assets/Scripts/Network/HostDisconnectHandler.cs` - Main handler script
- `Assets/Scripts/GameManager.cs` - Auto-initialization logic
- `Assets/Scripts/Netcode/NetworkUI.cs` - Network startup UI
- `Assets/Scripts/Lobby/LobbyMenuManager.cs` - Lobby scene manager
