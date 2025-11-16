# ?? Visual Quick Start - MCP Setup for AGAP-LIKAS

## ??? ONE-CLICK START

```
?? Open File Explorer
   ?
?? Navigate to: C:\Users\Non Admin\Documents\GitHub\AGAP-LIKAS\Assets\Scripts\Netcode
   ?
??? Double-click: Setup-Multiplayer.bat
   ?
? Follow the wizard!
```

---

## ?? What You'll See

### Screen 1: Welcome
```
========================================
 AGAP-LIKAS Multiplayer Setup Wizard
========================================

?? Project Path: C:\Users\Non Admin\Documents\GitHub\AGAP-LIKAS
```

### Screen 2: Verification
```
Step 1: Verifying Unity Project Structure...
  ? Assets folder found
  ? Scripts\Netcode directory exists

Step 2: Checking Required Files...
  ? NetworkLobbyManager.cs          [PRESENT]
  ? EnhancedNetworkUI.cs             [PRESENT]
  ? NetworkConnectionManager.cs      [PRESENT]
  ? NetworkPlayerManager.cs          [PRESENT]
  ? LobbyManager.cs                  [PRESENT]
  ? SceneLoader.cs                   [PRESENT]
```

### Screen 3: Package Check
```
Step 3: Checking for Unity Netcode Package...
  
  Option A (Package Found):
  ? Netcode package found: 1.x.x

  Option B (Package Missing):
  ? Netcode package NOT found
  
  ?? To install:
     1. Open Unity Editor
     2. Window ? Package Manager
     3. Unity Registry ? "Netcode for GameObjects"
     4. Click Install
```

### Screen 4: Setup Instructions
```
?? LOBBY SCENE SETUP CHECKLIST:
[Detailed step-by-step instructions appear here]

?? UI SETUP CHECKLIST:
[UI creation instructions appear here]

?? TESTING CHECKLIST:
[Testing procedures appear here]
```

### Screen 5: Summary
```
========================================
 ?? SETUP SUMMARY
========================================

? Files Status: All Present

?? Next Steps:
1. Follow the checklist above
2. Open: QUICK_START_CHECKLIST.md
3. Test locally first
4. Then test on network

Would you like to open the Quick Start Checklist now? (Y/N)
```

---

## ?? Your Action Plan

### Now (5 minutes)
```
1. Run Setup-Multiplayer.bat
2. Read the output carefully
3. Note any warnings/errors
```

### Next (15-20 minutes - In Unity)
```
1. Open Unity Editor
2. Install Netcode package (if needed)
3. Create Lobby scene (or open existing)
4. Add 4 network GameObjects:
   - NetworkManager
   - NetworkLobbyManager
   - NetworkConnectionManager
   - EnhancedNetworkUI
```

### Then (10-15 minutes - UI Setup)
```
1. Create Canvas (if not exists)
2. Add ConnectionPanel + buttons
3. Add LobbyInfoPanel + text fields
4. Add HostControlsPanel + start button
5. Assign all in EnhancedNetworkUI Inspector
```

### Finally (10 minutes - Testing)
```
1. Build the project
2. Run build (Host)
3. Press Play in Editor (Client)
4. Test connection
5. Verify settings sync
```

---

## ?? File Locations Quick Reference

```
Your Project Root
?? Assets
   ?? Scripts
      ?? Netcode
         ?? Setup-Multiplayer.bat        ? START HERE! ??
         ?? Setup-Multiplayer.ps1
         ?? mcp-server.js
         ?? mcp-config.json
         ?? MCP_STEP_BY_STEP.md          ? Detailed walkthrough
         ?? QUICK_START_CHECKLIST.md     ? 30-min guide
         ?? MCP_INTEGRATION_GUIDE.md     ? MCP documentation
         ?? QUICK_REFERENCE.md           ? Cheat sheet
         ?? [All other documentation]
```

---

## ?? UI Structure Visual

```
Canvas
?? ConnectionPanel [VISIBLE at start]
?  ?? Host Button
?  ?? Client Button
?  ?? Disconnect Button [HIDDEN at start]
?  ?? IP Address InputField [OPTIONAL]
?
?? LobbyInfoPanel [HIDDEN at start]
?  ?? Task Count Text (TMP)
?  ?? Duration Text (TMP)
?  ?? Disaster Type Text (TMP)
?  ?? Player Count Text (TMP)
?  ?? Connection Status Text (TMP)
?
?? HostControlsPanel [HIDDEN at start]
   ?? Start Game Button
```

---

## ?? Multiplayer Flow Visual

```
INSTRUCTOR (Host)                  TRAINEE (Client)
     ?                                  ?
     ? 1. Opens Lobby                   ? 1. Opens Lobby
     ? 2. Clicks "Host Game"            ? 2. Enters IP (optional)
     ? 3. Lobby appears                 ? 3. Clicks "Join Game"
     ?                                  ?
     ? ??????? Network Connection ???????
     ?                                  ?
     ? 4. Configures settings           ? 4. Views settings (read-only)
     ?    - Tasks: 3                    ?    - Tasks: 3 ?
     ?    - Duration: 5 min             ?    - Duration: 5 min ?
     ?    - Disaster: TestKen           ?    - Disaster: TestKen ?
     ?                                  ?
     ? 5. Sees "Players: 2/4"           ? 5. Sees "Players: 2/4"
     ?                                  ?
     ? 6. Clicks "Start Game"           ? 6. Waits...
     ?                                  ?
     ? ???????? Load Game Scene ??????????
     ?                                  ?
     ? 7. Game starts                   ? 7. Game starts
     ? 8. Both play together! ??        ? 8. Both play together! ??
```

---

## ?? Success Indicators

### ? Setup Complete When You See:

1. **In Build (Host):**
   ```
   [NetworkLobbyManager] Initialized lobby settings
   [NetworkConnectionManager] Server started
   [EnhancedNetworkUI] Hosting lobby...
   ```

2. **In Editor (Client):**
   ```
   [NetworkConnectionManager] Local client connected
   [EnhancedNetworkUI] Connected to lobby!
   [NetworkLobbyManager] Received lobby settings
   ```

3. **In Both:**
   ```
   Lobby Information:
   Tasks: 3
   Duration: 5 minutes
   Scenario: TestKen
   Players: 2/4
   ```

---

## ?? Quick Troubleshooting

| Problem | Quick Fix |
|---------|-----------|
| Wizard won't run | Right-click `Setup-Multiplayer.bat` ? Run as Administrator |
| Can't find file | Ensure you're in: `...\AGAP-LIKAS\Assets\Scripts\Netcode\` |
| Netcode not found | Install via Package Manager (wizard shows how) |
| UI not appearing | Check all assignments in EnhancedNetworkUI Inspector |
| Can't connect | Check firewall, port 7777 |

---

## ?? Need More Help?

**Documentation Priority:**

1. **MCP_STEP_BY_STEP.md** ? Most detailed walkthrough
2. **QUICK_START_CHECKLIST.md** ? 30-minute guided setup
3. **QUICK_REFERENCE.md** ? One-page cheat sheet
4. **MULTIPLAYER_SETUP_GUIDE.md** ? Complete manual

**All located in:** `Assets\Scripts\Netcode\`

---

## ?? Ready?

**Run this now:**
```
Setup-Multiplayer.bat
```

**Time investment:** ~45 minutes total
**Result:** Fully working multiplayer lobby! ??

---

**Good luck! You've got this! ??**
