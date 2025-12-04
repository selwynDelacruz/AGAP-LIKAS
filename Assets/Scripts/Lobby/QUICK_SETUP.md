# Quick Setup Guide - NetworkManager in LobbyMenu Only

## ?? Key Difference from Original Setup

**OLD APPROACH:**
- NetworkManager initialized in MainMenu via NetworkManagerInitializer
- Required components in MainMenu scene

**NEW APPROACH (CURRENT):**
- NetworkManager initialized in LobbyMenu via LobbyMenuManager
- MainMenu has NO network components
- Simpler, cleaner, more logical

---

## ? Quick Setup Checklist

### ? 1. Create NetworkManager Prefab
- [ ] Create NetworkManager GameObject (anywhere, temporarily)
- [ ] Add `NetworkManager` component
- [ ] Add `UnityTransport` component
- [ ] Configure both components (Port: 7777, DontDestroyOnLoad: ?)
- [ ] Drag to `Assets/Prefabs/` to create prefab
- [ ] **DELETE from scene hierarchy**

### ? 2. MainMenu Scene
- [ ] ? **NO network components needed!**
- [ ] Just ensure `GoToLobby()` loads "LobbyMenu" scene

### ? 3. LobbyMenu Scene
- [ ] Create `UnityMainThreadDispatcher` GameObject
  - Add `UnityMainThreadDispatcher` component
- [ ] Create `LobbyMenuManager` GameObject
  - Add `LobbyMenuManager` component
  - **Assign NetworkManager prefab** to `Network Manager Prefab` field
- [ ] Create UI:
  - Create Lobby Panel (Instructor)
  - Join Lobby Panel (Trainee)
- [ ] Assign all UI references to LobbyMenuManager
- [ ] Add scene to Build Settings

### ? 4. LobbyRoom Scene
- [ ] Create `LobbyRoomManager` GameObject
  - Add `LobbyRoomManager` component
  - **Add `NetworkObject` component** ?? Required!
- [ ] Create UI:
  - Host Panel (configuration)
  - Client Panel (waiting)
  - Status Panel (shared info)
- [ ] Assign all UI references to LobbyRoomManager
- [ ] Add scene to Build Settings

### ? 5. Game Scenes
- [ ] Optional: Add `NetworkManagerDebugger` for F1 debug info
- [ ] Add to Build Settings

---

## ?? What Happens at Runtime

```
1. User plays MainMenu
   ?? No network initialization

2. User clicks "Play Game"
   ?? Loads LobbyMenu scene

3. LobbyMenu.Awake()
   ?? LobbyMenuManager.InitializeNetworkManager()
      ?? Spawns NetworkManager from prefab
      ?? Calls DontDestroyOnLoad(networkManager)
      ?? NetworkManager persists from now on

4. User creates/joins lobby
   ?? NetworkManager.StartHost() or StartClient()

5. Loads LobbyRoom
   ?? NetworkManager still exists (DontDestroyOnLoad)

6. Loads Game Scene
   ?? NetworkManager still exists

7. Loads Result Scene
   ?? NetworkManager.Shutdown() when returning to main menu
```

---

## ?? Verification Steps

### After Setup, Test:

1. **Play MainMenu:**
   - Press F12 (open Console)
   - Should NOT see any network initialization logs

2. **Click "Play Game" ? LobbyMenu loads:**
   - Console should show:
     ```
     [LobbyMenuManager] NetworkManager initialized successfully
     [LobbyMenuManager] Transport: UnityTransport
     [LobbyMenuManager] Default Port: 7777
     ```

3. **While in LobbyMenu, check Hierarchy:**
   - Look for `DontDestroyOnLoad` folder
   - Should contain:
     - `NetworkManager (Persistent)`
     - `UnityMainThreadDispatcher`

4. **Create or Join Lobby:**
   - Console should show host/client start messages
   - Press F1 to see debug info (if NetworkManagerDebugger added)

---

## ?? Common Mistakes to Avoid

### ? DON'T:
1. Add NetworkManagerInitializer to MainMenu (not needed!)
2. Place NetworkManager GameObject in ANY scene
3. Have multiple NetworkManager prefabs
4. Forget to assign prefab to LobbyMenuManager
5. Forget NetworkObject on LobbyRoomManager

### ? DO:
1. Only have NetworkManager as a prefab in Assets/Prefabs/
2. Assign prefab to LobbyMenuManager in LobbyMenu scene
3. Ensure DontDestroyOnLoad is checked on prefab
4. Add NetworkObject to LobbyRoomManager
5. Test with Debug Mode enabled

---

## ?? File Structure

```
Assets/
?? Prefabs/
?  ?? NetworkManager.prefab ? ONLY location of NetworkManager
?
?? Scenes/
?  ?? MainMenu.unity ? No network components
?  ?? LobbyMenu.unity ? Has LobbyMenuManager + UnityMainThreadDispatcher
?  ?? LobbyRoom.unity ? Has LobbyRoomManager (with NetworkObject)
?  ?? Flood.unity
?  ?? Earthquake.unity
?  ?? TestKen.unity
?
?? Scripts/
   ?? Lobby/
   ?  ?? LobbyCodeGenerator.cs
   ?  ?? LobbyBroadcaster.cs
   ?  ?? LobbyScanner.cs
   ?  ?? UnityMainThreadDispatcher.cs
   ?  ?? LobbyMenuManager.cs ? Initializes NetworkManager
   ?  ?? LobbyRoomManager.cs
   ?
   ?? Network/
      ?? NetworkManagerInitializer.cs ? NOT USED (optional for other scenarios)
      ?? NetworkManagerDebugger.cs ? Optional debug tool
```

---

## ?? That's It!

With this approach:
- ? Simpler setup (fewer components)
- ? Clear separation (login vs networking)
- ? NetworkManager only exists when needed
- ? Easier to understand and debug

For detailed step-by-step instructions, see **SETUP_GUIDE.md**
