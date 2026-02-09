# Network Interaction System Setup Guide

## Overview
This guide explains how to set up networked interactions for victim rescuing, medkit usage, and rubble clearing in your multiplayer disaster simulation game.

## Changes Made

### 1. PlayerInteract.cs - Network Synchronization
**Converted to NetworkBehaviour** with ServerRpcs for all interactions.

#### Key Features:
- ? **Owner-only input** - Only the local player can interact
- ? **Server-authoritative** - All interactions validated on server
- ? **Automatic detection** - Detects if object has NetworkObject component
- ? **Backwards compatible** - Still works with non-networked objects

#### ServerRpcs Added:
| ServerRpc | Purpose | Points Awarded |
|-----------|---------|----------------|
| `RequestVictimRescueServerRpc` | Rescue NPC victim | 20 points |
| `RequestMedkitUseServerRpc` | Use medkit on victim | 10 points |
| `RequestRubbleClearServerRpc` | Clear rubble obstacle | 10 points |
| `RequestGenericInteractionServerRpc` | Fallback for other types | Varies |

#### Client RPCs:
- `NotifyMedkitUseFailedClientRpc` - Shows error when medkit use fails

### 2. Required Setup for Victim/NPC Prefabs

#### Step 1: Add NetworkObject Component
1. Select victim prefab in Project window
2. Add Component ? **NetworkObject**
3. Configure settings:
   - **Ownership**: None (Server)
   - **Synchronize Transform**: ? Checked
   - **Spawn With Observers**: ? Checked
   - **Scene Migration Synchronization**: ? Checked

#### Step 2: Register in NetworkManager
1. Open NetworkManager GameObject
2. Find **Network Prefabs List**
3. Add ALL victim prefabs to the list

#### Step 3: Update VictimSpawner (Required)
The `VictimSpawner` needs to use network spawning instead of local `Instantiate`.

**See**: `VICTIMSPAWNER_NETWORK_SETUP.md` for detailed conversion guide.

### 3. Required Setup for Medkit Prefabs

Same as victim prefabs:
1. ? Add NetworkObject component
2. ? Configure as server-owned
3. ? Register in NetworkManager's Network Prefabs List

### 4. Required Setup for Rubble Prefabs

Same as above - all interactable objects that should sync across network need NetworkObject.

## How It Works

### Network Flow for Victim Rescue:

```
Client (Trainee)                    Server (Instructor/Host)
     |                                      |
     | 1. Press & Hold E                    |
     |------------------------------------- |
     |                                      |
     | 2. RequestVictimRescueServerRpc      |
     |------------------------------------->|
     |                                      | 3. Validate NetworkObject exists
     |                                      | 4. Award points (PointManager)
     |                                      | 5. Increment saved victims
     |                                      | 6. Despawn NetworkObject
     |<-------------------------------------|
     | 7. Victim disappears on all clients  |
```

### Network Flow for Medkit Usage:

```
Client (Trainee)                    Server (Instructor/Host)
     |                                      |
     | 1. Press & Hold E on victim          |
     |------------------------------------- |
     |                                      |
     | 2. RequestMedkitUseServerRpc         |
     |------------------------------------->|
     |                                      | 3. Check medkit count
     |                                      | 4. If count > 0:
     |                                      |    - Use medkit
     |                                      |    - Award points
     |                                      |    - Despawn victim
     |                                      | 5. If count == 0:
     |<-------------------------------------|    - Send failure notification
     | 6. NotifyMedkitUseFailedClientRpc    |
     |    (triggers red blink)              |
```

## Testing Checklist

### ? Pre-Test Setup:
- [ ] All victim prefabs have NetworkObject component
- [ ] All victim prefabs registered in NetworkManager
- [ ] All medkit victim prefabs have NetworkObject
- [ ] All rubble prefabs have NetworkObject (if using networked rubble)
- [ ] VictimSpawner updated to use network spawning
- [ ] Player prefab has PlayerInteract script

### ? Test as Host (Instructor):
1. Start game as host
2. Approach victim and press E for 5 seconds
3. **Expected**: Victim disappears, points awarded
4. Check console for: `[PlayerInteract] Server: Victim rescued and despawned`

### ? Test as Client (Trainee):
1. Join as client
2. Approach victim and press E for 5 seconds
3. **Expected**: Victim disappears for ALL players
4. **Expected**: Points synced across network

### ? Test Medkit Usage:
1. Use all medkits
2. Try to interact with medkit-victim
3. **Expected**: Red blink effect, "You don't have medkit!" message
4. Enter safe zone to replenish
5. **Expected**: Medkit count restored, can use again

## Debugging

### Enable Debug Logs:
In `PlayerInteract` inspector, check **Show Debug Logs**

### Common Issues:

| Issue | Cause | Solution |
|-------|-------|----------|
| Victim doesn't disappear on client | Prefab not registered | Add to NetworkManager prefabs list |
| "NetworkObject not found!" | Wrong spawn method | Use `NetworkObject.Spawn()` instead of `Instantiate` |
| Points not syncing | PointManager not networked | See POINTMANAGER_NETWORK_SETUP.md |
| Medkit count wrong | GameManager medkit not synced | Use NetworkVariable for medkit count |

### Expected Console Logs:

**On Server (when victim rescued):**
```
[PlayerInteract] Server: Processing victim rescue request for NetworkObjectId: 5
[PlayerInteract] Server: Awarded 20 points for rescuing victim
[GameManager] Saved victims: 1/5
[PlayerInteract] Server: Victim rescued and despawned
```

**On Client (when victim rescued):**
```
[PlayerInteract] Started holding E. Found interactable: Victim_NPC
[PlayerInteract] Hold complete! Triggering network interaction.
[PlayerInteract] Networked interactable detected: Victim_NPC (NetworkObjectId: 5)
```

## Next Steps

1. **Convert VictimSpawner to network spawning** - See VICTIMSPAWNER_NETWORK_SETUP.md
2. **Add NetworkVariables to PointManager** - See POINTMANAGER_NETWORK_SETUP.md
3. **Sync GameManager medkit count** - Use NetworkVariable<int> for medkit count
4. **Test with multiple clients** - Verify all interactions sync properly

## Files Modified

- ? `Assets/Scripts/PlayerInteract.cs` - Added NetworkBehaviour, ServerRpcs, ClientRpcs
- ? `Assets/Scripts/VictimSpawner.cs` - Needs network spawning conversion
- ? `Assets/Scripts/PointManager.cs` - Needs NetworkVariable for points
- ? `Assets/Scripts/GameManager.cs` - Needs NetworkVariable for medkit count

## Architecture Notes

### Server Authority:
- ? All point awards happen on server
- ? All object despawns controlled by server  
- ? Medkit consumption validated server-side
- ? Victim count tracked server-side

### Client Authority:
- ? Input detection (pressing E)
- ? Visual feedback (progress bar)
- ? Local interaction detection (overlap sphere)

This ensures no cheating and all clients see consistent game state!

## Support

If interactions aren't working:
1. Check Unity Console for errors
2. Enable debug logs on PlayerInteract
3. Verify NetworkObjects are spawned (not locally instantiated)
4. Check NetworkManager prefab list contains all interactable prefabs

