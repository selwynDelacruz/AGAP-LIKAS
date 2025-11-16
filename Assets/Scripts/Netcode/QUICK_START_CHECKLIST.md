# Quick Start Checklist - Multiplayer Lobby Setup

## Prerequisites
- [ ] Unity Netcode for GameObjects package installed (Window ? Package Manager ? Unity Registry ? "Netcode for GameObjects")
- [ ] Project builds successfully
- [ ] You have a Lobby scene or MainMenu scene

## Scene Setup (15-20 minutes)

### Step 1: NetworkManager Setup (5 min)
- [ ] Open your Lobby scene
- [ ] Create empty GameObject ? Name it "NetworkManager"
- [ ] Add Component ? Netcode ? NetworkManager
- [ ] Add Component ? Netcode ? UnityTransport
- [ ] In NetworkManager component:
  - [ ] Enable "Scene Management" section ? Check "Enable Scene Management"
  - [ ] Set "Transport" to the UnityTransport component

### Step 2: NetworkLobbyManager Setup (3 min)
- [ ] Create empty GameObject ? Name it "NetworkLobbyManager"
- [ ] Add Component ? NetworkObject
- [ ] Add Component ? NetworkLobbyManager (your new script)
- [ ] Configure:
  - [ ] Max Players: 4 (or your desired number)

### Step 3: NetworkConnectionManager Setup (2 min)
- [ ] Create empty GameObject ? Name it "NetworkConnectionManager"
- [ ] Add Component ? NetworkConnectionManager
- [ ] Configure:
  - [ ] Lobby Scene Name: "Lobby" (or your scene name)
  - [ ] Main Menu Scene Name: "MainMenu" (or your scene name)

### Step 4: UI Setup (10 min)

#### Create Connection Panel
- [ ] Create UI ? Panel ? Name it "ConnectionPanel"
- [ ] Add Button ? Name it "HostButton" ? Text: "Host Game"
- [ ] Add Button ? Name it "ClientButton" ? Text: "Join Game"
- [ ] Add Button ? Name it "DisconnectButton" ? Text: "Disconnect"
- [ ] Add InputField ? Name it "IPAddressInput" ? Placeholder: "Enter IP (optional)"

#### Create Lobby Info Panel
- [ ] Create UI ? Panel ? Name it "LobbyInfoPanel"
- [ ] Set Active to FALSE initially
- [ ] Add TextMeshPro ? Name it "TaskCountText"
- [ ] Add TextMeshPro ? Name it "DurationText"
- [ ] Add TextMeshPro ? Name it "DisasterTypeText"
- [ ] Add TextMeshPro ? Name it "PlayerCountText"
- [ ] Add TextMeshPro ? Name it "ConnectionStatusText"

#### Create Host Controls Panel
- [ ] Create UI ? Panel ? Name it "HostControlsPanel"
- [ ] Set Active to FALSE initially
- [ ] Add Button ? Name it "StartGameButton" ? Text: "Start Game"

### Step 5: EnhancedNetworkUI Setup (5 min)
- [ ] Create empty GameObject ? Name it "EnhancedNetworkUI"
- [ ] Add Component ? EnhancedNetworkUI
- [ ] Assign all UI elements in Inspector:
  - **Connection Buttons:**
    - [ ] Host Button
    - [ ] Client Button
    - [ ] Disconnect Button
  - **Lobby Info Panel:**
    - [ ] Lobby Info Panel GameObject
    - [ ] Task Count Text
    - [ ] Duration Text
    - [ ] Disaster Type Text
    - [ ] Player Count Text
    - [ ] Connection Status Text
  - **Host Controls:**
    - [ ] Host Controls Panel GameObject
    - [ ] Start Game Button
  - **Connection Settings:**
    - [ ] IP Address Input

### Step 6: LobbyManager Integration (2 min)
- [ ] Find your existing LobbyManager GameObject
- [ ] In Inspector, check "Is Multiplayer" checkbox

## Testing (10 minutes)

### Local Test (Same Computer)
- [ ] Build the project (File ? Build Settings ? Build)
- [ ] Run the build
- [ ] In the build, click "Host Game"
- [ ] In Unity Editor, press Play
- [ ] In the editor, click "Join Game"
- [ ] Verify:
  - [ ] Both show connected status
  - [ ] Both display lobby settings
  - [ ] Host can change settings
  - [ ] Client sees settings update
  - [ ] Host can start game
  - [ ] Both load into game scene

### Network Test (Different Computers)
- [ ] On Host computer:
  - [ ] Find IP address (Windows: ipconfig, Mac: ifconfig)
  - [ ] Open port 7777 in firewall
  - [ ] Run build, click "Host Game"
- [ ] On Client computer:
  - [ ] Run build
  - [ ] Enter host's IP in input field
  - [ ] Click "Join Game"
- [ ] Verify connection and lobby functionality

## Common Issues & Quick Fixes

### "NetworkManager.Singleton is null"
- ? Ensure NetworkManager GameObject exists in scene
- ? Check NetworkManager component is attached

### "Cannot start host/client"
- ? Check UnityTransport component is attached
- ? Verify port 7777 is not in use by another application

### UI doesn't show
- ? Verify all UI elements are assigned in EnhancedNetworkUI Inspector
- ? Check Canvas has Canvas Scaler and GraphicRaycaster

### Settings don't sync
- ? NetworkLobbyManager must have NetworkObject component
- ? Check "Is Multiplayer" is enabled in LobbyManager
- ? Only host can change settings (by design)

### Can't connect from another computer
- ? Verify firewall allows port 7777
- ? Check both computers are on same network
- ? Confirm correct IP address is entered
- ? Try ping test: `ping <host-ip>`

## Verification Checklist

Before instructor/trainee session:
- [ ] Host can start and configure lobby
- [ ] Client can connect to host
- [ ] Settings appear correctly for both
- [ ] Host can change settings, client sees updates
- [ ] Player count displays correctly
- [ ] Start Game button works (host only)
- [ ] Game scene loads for all players
- [ ] PlayerPrefs correctly populated in game scene
- [ ] VictimSpawner reads correct TaskCount

## Next Steps After Basic Setup Works

1. **Add NetworkPlayerManager to game scenes**
   - Follow instructions in NetworkPlayerManager.cs comments
   - Set up player spawning

2. **Create networked player prefab**
   - Add NetworkObject, NetworkTransform, NetworkAnimator
   - Configure for multiplayer

3. **Update VictimSpawner for network**
   - Add host-only check for spawning
   - Make victims networked objects

4. **Create NetworkGameManager**
   - Sync game state, timer, scores
   - Handle game completion

## Help & Documentation

- Full Setup Guide: `Assets\Scripts\Netcode\MULTIPLAYER_SETUP_GUIDE.md`
- Architecture Diagram: `Assets\Scripts\Netcode\ARCHITECTURE_DIAGRAM.cs`
- Implementation Summary: `Assets\Scripts\Netcode\IMPLEMENTATION_SUMMARY.md`

## Estimated Time
- **Scene Setup**: 15-20 minutes
- **Testing**: 10 minutes
- **Total**: ~30 minutes for basic lobby functionality

---

**Ready to Start?** Follow the checklist from top to bottom. Each step is designed to take just a few minutes!
