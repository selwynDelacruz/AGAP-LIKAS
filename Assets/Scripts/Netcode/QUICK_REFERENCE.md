# ?? AGAP-LIKAS Multiplayer - Quick Reference Card

## ?? Installation (One-Time Setup)

### Step 1: Install Unity Netcode Package
```
Unity Editor ? Window ? Package Manager ? Unity Registry
Search: "Netcode for GameObjects" ? Install
```

### Step 2: Run Automated Setup
```
Double-click: Assets\Scripts\Netcode\Setup-Multiplayer.bat
OR
Right-click folder ? Open in Terminal ? node mcp-server.js
```

---

## ?? Scene Setup (Follow Wizard Output)

### Required GameObjects:
1. **NetworkManager** (NetworkManager + UnityTransport)
2. **NetworkLobbyManager** (NetworkObject + NetworkLobbyManager script)
3. **NetworkConnectionManager** (NetworkConnectionManager script)
4. **EnhancedNetworkUI** (EnhancedNetworkUI script + UI assignments)

### UI Hierarchy:
```
Canvas
?? ConnectionPanel (Host, Client, Disconnect buttons)
?? LobbyInfoPanel (Task count, Duration, Disaster, Players)
?? HostControlsPanel (Start Game button)
?? DisconnectButton
```

---

## ?? Usage Flow

### Instructor (Host):
1. Open Lobby ? Click "Host Game"
2. Configure settings (tasks, duration, disaster)
3. Wait for trainees to join
4. Click "Start Game"

### Trainee (Client):
1. Open Lobby ? Enter instructor's IP (if needed)
2. Click "Join Game"
3. View lobby settings
4. Wait for host to start

---

## ?? Testing

### Local Test (Same PC):
1. Build project
2. Run build (Host)
3. Press Play in Editor (Client)
4. Both should connect

### Network Test:
**Host PC:**
- Find IP: `ipconfig` (Windows) or `ifconfig` (Mac/Linux)
- Open port 7777 in firewall
- Run build ? Host Game

**Client PC:**
- Enter host IP
- Join Game

---

## ?? Troubleshooting

| Problem | Solution |
|---------|----------|
| Can't connect | Check firewall, port 7777 |
| Settings don't sync | Verify NetworkLobbyManager has NetworkObject |
| UI doesn't appear | Check all UI assignments in Inspector |
| Netcode missing | Install via Package Manager |

---

## ?? Documentation Quick Links

- **Setup Wizard**: `Setup-Multiplayer.bat`
- **MCP Guide**: `MCP_INTEGRATION_GUIDE.md`
- **Quick Start**: `QUICK_START_CHECKLIST.md` (30 min)
- **Full Guide**: `MULTIPLAYER_SETUP_GUIDE.md`
- **Architecture**: `ARCHITECTURE_DIAGRAM.cs`
- **UI Layout**: `UI_LAYOUT_REFERENCE.cs`

---

## ?? Quick Commands

### Build & Test:
```
File ? Build Settings ? Build
```

### Find IP Address:
```
Windows: ipconfig
Mac: ifconfig
Linux: ip addr
```

### Open Firewall Port 7777:
```
Windows Firewall ? Advanced Settings ? Inbound Rules
? New Rule ? Port ? TCP ? 7777 ? Allow
```

---

## ?? Status Indicators

**? Green** = Working correctly
**?? Yellow** = Warning / needs attention
**? Red** = Error / not working

---

## ?? Success Checklist

- [ ] Netcode package installed
- [ ] All network GameObjects created
- [ ] NetworkLobbyManager has NetworkObject
- [ ] UI elements assigned in Inspector
- [ ] LobbyManager has "Is Multiplayer" checked
- [ ] Local test successful
- [ ] Network test successful

---

## ?? Need Help?

1. Check wizard output for errors
2. Review `QUICK_START_CHECKLIST.md`
3. Read troubleshooting in `MULTIPLAYER_SETUP_GUIDE.md`
4. Verify all files are present

---

## ? Pro Tips

- **Always test locally first** before network testing
- **Firewall issues** are the #1 cause of connection problems
- **Use build for testing** - Editor can have different behavior
- **Check console** for detailed error messages
- **Host controls** only work for the player who clicked "Host Game"

---

**Estimated Setup Time:** 30-45 minutes
**Required Knowledge:** Basic Unity Editor usage
**Difficulty:** Intermediate

**Happy Multiplayer Gaming! ??**
