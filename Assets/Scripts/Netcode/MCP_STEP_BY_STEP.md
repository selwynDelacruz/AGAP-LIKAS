# ?? MCP Automation - Step-by-Step Setup Guide

## Prerequisites Checklist

Before running the automation, ensure:

- [ ] Unity Editor is closed (to avoid file conflicts)
- [ ] You're on the `stable-netcode` branch ? (You are!)
- [ ] All multiplayer script files are present ? (They are!)
- [ ] Windows PowerShell is available

## ?? Running the MCP Automation

### Method 1: Double-Click (Easiest)

1. **Open File Explorer**
2. **Navigate to:**
   ```
   C:\Users\Non Admin\Documents\GitHub\AGAP-LIKAS\Assets\Scripts\Netcode
   ```
3. **Double-click:** `Setup-Multiplayer.bat`
4. **Wait for the wizard** to analyze your project

### Method 2: PowerShell Direct

1. **Right-click** in File Explorer in the Netcode folder
2. **Select:** "Open in Terminal" or "Open PowerShell window here"
3. **Run:**
   ```powershell
   .\Setup-Multiplayer.ps1
   ```

### Method 3: Node.js (If you have Node.js installed)

1. **Open Terminal** in the Netcode folder
2. **Run:**
   ```bash
   node mcp-server.js
   ```

## ?? What the Wizard Will Show You

### Phase 1: Verification
```
========================================
 AGAP-LIKAS Multiplayer Setup Wizard
========================================

Step 1: Verifying Unity Project Structure...
  ? Assets folder found
  ? Scripts\Netcode directory exists

Step 2: Checking Required Files...
  ? NetworkLobbyManager.cs
  ? EnhancedNetworkUI.cs
  ? NetworkConnectionManager.cs
  ? NetworkPlayerManager.cs
  ? LobbyManager.cs
  ? SceneLoader.cs (Updated!)
  ? All documentation files

Step 3: Checking for Unity Netcode Package...
  [Will check if installed]
```

### Phase 2: Installation Instructions (If Netcode Not Found)

If Netcode isn't installed, the wizard will show:

```
?? To install Netcode for GameObjects:
   1. Open Unity Editor
   2. Window ? Package Manager
   3. Unity Registry ? Search 'Netcode for GameObjects'
   4. Click Install
```

**Action Required:** Install the package if prompted, then re-run the wizard.

### Phase 3: Scene Setup Guidance

The wizard will display:

```
?? LOBBY SCENE SETUP CHECKLIST:
================================

1??  NetworkManager GameObject:
   - Create empty GameObject named 'NetworkManager'
   - Add Component: NetworkManager
   - Add Component: UnityTransport
   - Enable 'Scene Management' in NetworkManager
   - Assign UnityTransport to Transport field

2??  NetworkLobbyManager GameObject:
   - Create empty GameObject named 'NetworkLobbyManager'
   - Add Component: NetworkObject
   - Add Component: NetworkLobbyManager
   - Set Max Players to 4

[... more instructions ...]
```

### Phase 4: UI Setup Instructions

```
?? UI SETUP CHECKLIST:
======================

Canvas ? Create the following UI hierarchy:

?? ConnectionPanel (initially visible)
   ?? Host Button
   ?? Client Button
   ?? IP Address InputField

?? LobbyInfoPanel (initially hidden)
   ?? Task Count Text (TMP)
   ?? Duration Text (TMP)
   [... more UI elements ...]
```

### Phase 5: Testing Procedures

```
?? TESTING CHECKLIST:
=====================

LOCAL TEST (Same Computer):
1. File ? Build Settings ? Build
2. Run the build (acts as Host)
[... detailed test steps ...]
```

## ?? After Running the Wizard

### Immediate Next Steps:

1. **Review the output** - The wizard provides detailed instructions
2. **Check for warnings** - Yellow warnings need attention
3. **Note any errors** - Red errors must be fixed

### Manual Work Required:

The wizard **guides** you, but these steps must be done **manually in Unity**:

#### A. Install Netcode Package (If not installed)
```
Unity Editor ? Window ? Package Manager
? Unity Registry ? "Netcode for GameObjects" ? Install
```

#### B. Create Lobby Scene (Or open existing one)
```
File ? New Scene ? Save as "Lobby"
```

#### C. Add Network GameObjects (Follow wizard checklist)
```
Create in Lobby scene:
1. NetworkManager
2. NetworkLobbyManager  
3. NetworkConnectionManager
4. EnhancedNetworkUI
```

#### D. Create UI Hierarchy (Follow wizard instructions)
```
Canvas ? Add all UI elements
Assign in EnhancedNetworkUI Inspector
```

#### E. Configure Components
```
Assign all references in Inspector
Check 'Is Multiplayer' in LobbyManager
```

## ?? Detailed Setup Walkthrough

### Step 1: Open Unity and Create Lobby Scene

1. **Open Unity Editor**
2. **File ? New Scene**
3. **Save as:** `Lobby.unity` in `Assets/Scenes/`

### Step 2: Add NetworkManager

1. **Right-click** in Hierarchy ? Create Empty
2. **Rename** to `NetworkManager`
3. **Add Component** ? Search "NetworkManager" ? Add
4. **Add Component** ? Search "UnityTransport" ? Add
5. **In NetworkManager Inspector:**
   - Enable "Enable Scene Management" ?
   - Set Transport ? UnityTransport (drag from same GameObject)

### Step 3: Add NetworkLobbyManager

1. **Create Empty** GameObject ? `NetworkLobbyManager`
2. **Add Component** ? NetworkObject
3. **Add Component** ? NetworkLobbyManager (your script)
4. **In Inspector:**
   - Max Players: 4

### Step 4: Add NetworkConnectionManager

1. **Create Empty** GameObject ? `NetworkConnectionManager`
2. **Add Component** ? NetworkConnectionManager (your script)
3. **In Inspector:**
   - Lobby Scene Name: "Lobby"
   - Main Menu Scene Name: "MainMenu"

### Step 5: Create UI Structure

1. **Create UI ? Canvas** (if not exists)
2. **Create UI ? Panel** ? Rename to `ConnectionPanel`
   - **Add Button** ? "Host Game"
   - **Add Button** ? "Join Game"
   - **Add Button** ? "Disconnect" (hide initially)
   - **Add InputField** ? "IP Address" (optional)

3. **Create UI ? Panel** ? Rename to `LobbyInfoPanel` (hide initially)
   - **Add Text (TMP)** ? "Task Count Text"
   - **Add Text (TMP)** ? "Duration Text"
   - **Add Text (TMP)** ? "Disaster Type Text"
   - **Add Text (TMP)** ? "Player Count Text"
   - **Add Text (TMP)** ? "Connection Status Text"

4. **Create UI ? Panel** ? Rename to `HostControlsPanel` (hide initially)
   - **Add Button** ? "Start Game"

### Step 6: Add EnhancedNetworkUI

1. **Create Empty** GameObject ? `EnhancedNetworkUI`
2. **Add Component** ? EnhancedNetworkUI (your script)
3. **Assign ALL UI elements** in Inspector:
   - Connection Buttons (Host, Client, Disconnect)
   - Lobby Info Panel + all text fields
   - Host Controls Panel + Start Game button
   - IP Address InputField

### Step 7: Update Existing LobbyManager

1. **Find** your existing `LobbyManager` GameObject
2. **In Inspector:**
   - ? Check "Is Multiplayer"

### Step 8: Build and Test

1. **File ? Build Settings**
2. **Add Open Scenes** (if not added)
3. **Build** (Create a build folder)
4. **Run Build** ? Click "Host Game"
5. **Press Play in Editor** ? Click "Join Game"
6. **Verify** both instances connect

## ?? Troubleshooting

### Issue: "NetworkManager.Singleton is null"
**Solution:** Ensure NetworkManager GameObject exists in scene with NetworkManager component

### Issue: "Cannot start host/client"
**Solution:** Check UnityTransport component is added and assigned

### Issue: UI doesn't appear
**Solution:** Verify all UI elements are assigned in EnhancedNetworkUI Inspector

### Issue: Settings don't sync
**Solution:** 
- Ensure NetworkLobbyManager has NetworkObject component
- Verify "Is Multiplayer" is checked in LobbyManager

### Issue: Can't connect from another computer
**Solution:**
- Open port 7777 in Windows Firewall
- Use correct IP address (ipconfig to find)
- Both computers on same network

## ?? Reference Documentation

While following the wizard, refer to:

- **QUICK_START_CHECKLIST.md** - Complete 30-min guide
- **UI_LAYOUT_REFERENCE.cs** - UI structure details
- **MULTIPLAYER_SETUP_GUIDE.md** - Comprehensive manual
- **QUICK_REFERENCE.md** - One-page cheat sheet

## ? Verification Checklist

Before considering setup complete, verify:

- [ ] Netcode package installed
- [ ] All network GameObjects created
- [ ] NetworkLobbyManager has NetworkObject
- [ ] All UI elements created and assigned
- [ ] LobbyManager has "Is Multiplayer" checked
- [ ] Local test successful (build + editor)
- [ ] Both instances connect
- [ ] Lobby settings sync from host to client
- [ ] Host can start game
- [ ] Game scene loads for both players

## ?? Expected Timeline

| Phase | Time | Description |
|-------|------|-------------|
| Run Wizard | 2 min | Automated checks |
| Install Netcode | 3 min | If needed |
| Scene Setup | 15 min | Create GameObjects |
| UI Setup | 15 min | Create and assign UI |
| Build & Test | 10 min | First test |
| **Total** | **~45 min** | Complete setup |

## ?? You're Ready!

After completing these steps, you'll have:

? Fully functional multiplayer lobby
? Host/Client connection system
? Settings synchronization
? Network scene loading
? Tested and working multiplayer

**Now run the wizard:**
```powershell
.\Setup-Multiplayer.bat
```

And follow the on-screen instructions! ??
