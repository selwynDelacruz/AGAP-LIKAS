# Simplified Setup Guide - NetworkManager in LobbyMenu Scene

## ?? Approach: NetworkManager Lives in LobbyMenu Scene

**This is the SIMPLEST approach:**
- NetworkManager exists ONLY in LobbyMenu scene
- No prefabs needed
- No spawning at runtime
- Just set it to DontDestroyOnLoad and it persists

---

## ? Super Quick Setup (3 Steps!)

### Step 1: Add NetworkManager to LobbyMenu Scene

1. **Open LobbyMenu scene**

2. **Create NetworkManager:**
   ```
   Hierarchy ? Right-click ? Create Empty ? Name: "NetworkManager"
   ```

3. **Add Components:**
   - Click "Add Component" ? `NetworkManager`
   - Click "Add Component" ? `UnityTransport`

4. **Configure NetworkManager:**
   ```
   NetworkManager Component:
   ? Don't Destroy On Load: CHECKED ? IMPORTANT!
   Network Transport: UnityTransport
   ```

5. **Configure UnityTransport:**
   ```
   UnityTransport Component:
   Address: 127.0.0.1
   Port: 7777
   Server Listen Address: 0.0.0.0
   ```

**That's it for NetworkManager!** LobbyMenuManager will handle the rest.

---

### Step 2: Add UnityMainThreadDispatcher

Still in LobbyMenu scene:

```
Hierarchy ? Right-click ? Create Empty ? Name: "UnityMainThreadDispatcher"
Add Component ? UnityMainThreadDispatcher (from Lobby namespace)
```

This handles thread-safe Unity API calls for the network scanning.

---

### Step 3: Setup LobbyMenuManager

1. **Create LobbyMenuManager:**
   ```
   Hierarchy ? Right-click ? Create Empty ? Name: "LobbyMenuManager"
   Add Component ? LobbyMenuManager (from Lobby namespace)
   ```

2. **Create your UI** (Create/Join panels, buttons, etc.) as described in main setup guide

3. **Assign UI references** to LobbyMenuManager in Inspector

4. **Configure settings:**
   - Lobby Room Scene Name: "LobbyRoom"
   - Port: 7777
   - ? Show Debug Logs

---

## ?? Final LobbyMenu Scene Structure

```
LobbyMenu Scene:
?? Canvas
?  ?? CreateLobbyPanel
?  ?  ?? CreateButton
?  ?  ?? LobbyCodeDisplay
?  ?  ?? BackButton
?  ?
?  ?? JoinLobbyPanel
?     ?? LobbyCodeInput
?     ?? JoinButton
?     ?? StatusText
?     ?? BackButton
?
?? NetworkManager ? Lives here permanently
?  ?? NetworkManager component (DontDestroyOnLoad: ?)
?  ?? UnityTransport component
?
?? UnityMainThreadDispatcher
?  ?? UnityMainThreadDispatcher component
?
?? LobbyMenuManager
   ?? LobbyMenuManager component
      ?? (All UI references assigned)
```

---

## ?? How It Works

```
1. User logs into MainMenu (no network)
   ?
2. User clicks "Play Game"
   ?
3. LobbyMenu scene loads
   ?
4. NetworkManager exists in scene
   ?
5. LobbyMenuManager.Awake()
   ?? Calls DontDestroyOnLoad(NetworkManager)
   ?? NetworkManager persists from now on
   ?
6. User creates/joins lobby
   ?? NetworkManager.StartHost() or StartClient()
   ?
7. Load LobbyRoom (NetworkManager stays alive)
   ?
8. Load Game Scene (NetworkManager stays alive)
   ?
9. Load Result (NetworkManager stays alive)
   ?
10. User returns to MainMenu
    ?? NetworkManager.Shutdown() and cleanup
```

---

## ? Advantages of This Approach

1. **Simplest possible setup**
   - No prefabs to manage
   - No runtime spawning
   - Just place it in the scene

2. **Visual and obvious**
   - You can see NetworkManager in the hierarchy
   - Easy to find and configure
   - Beginner-friendly

3. **No references needed**
   - LobbyMenuManager doesn't need prefab reference
   - NetworkManager.Singleton just works

4. **Easier debugging**
   - NetworkManager is visible in hierarchy
   - Can see settings in Inspector while playing
   - F1 debug overlay works immediately

---

## ?? Important Notes

### MainMenu Scene
- ? NO NetworkManager
- ? NO network components
- ? Just authentication and UI

### LobbyMenu Scene
- ? NetworkManager (with DontDestroyOnLoad checked)
- ? UnityMainThreadDispatcher
- ? LobbyMenuManager
- ? Lobby UI

### LobbyRoom Scene
- ? NO NetworkManager (uses existing one from LobbyMenu)
- ? LobbyRoomManager (with NetworkObject)
- ? Lobby Room UI

### Game Scenes
- ? NO NetworkManager (uses existing one from LobbyMenu)
- ? GameManager
- ? Game objects

---

## ?? Testing

1. **Play LobbyMenu scene directly:**
   - NetworkManager should be visible in hierarchy
   - Check "DontDestroyOnLoad" is checked in Inspector

2. **Play MainMenu scene:**
   - No network components
   - Click "Play Game" ? LobbyMenu loads
   - Check hierarchy for "DontDestroyOnLoad" folder
   - NetworkManager should appear there

3. **Create lobby:**
   - Click "Create Lobby"
   - Console: `[LobbyMenuManager] NetworkManager set to DontDestroyOnLoad`
   - Console: `[LobbyMenuManager] Started as Host`

4. **Press F1:**
   - Debug overlay shows network status
   - Should show "Is Host: True"

---

## ?? Complete Checklist

### In LobbyMenu Scene:
- [ ] NetworkManager GameObject exists
- [ ] NetworkManager has `NetworkManager` component
- [ ] NetworkManager has `UnityTransport` component
- [ ] `DontDestroyOnLoad` is CHECKED on NetworkManager component
- [ ] Port is set to 7777
- [ ] UnityMainThreadDispatcher GameObject exists
- [ ] LobbyMenuManager GameObject exists
- [ ] All UI is created and assigned to LobbyMenuManager
- [ ] Scene is in Build Settings

### In MainMenu Scene:
- [ ] NO NetworkManager (should be empty of network components)
- [ ] `GoToLobby()` loads "LobbyMenu" scene

### In LobbyRoom Scene:
- [ ] NO NetworkManager in scene
- [ ] LobbyRoomManager exists with NetworkObject component
- [ ] Lobby Room UI is created
- [ ] Scene is in Build Settings

---

## ?? Why This is the Best Approach for You

1. You can see and configure NetworkManager directly
2. No prefab management needed
3. Simpler code (no spawning logic)
4. NetworkManager appears exactly when needed (LobbyMenu)
5. Visual feedback in hierarchy
6. Easier for team collaboration

---

## ?? If You Need to Edit NetworkManager Settings:

1. Open LobbyMenu scene
2. Select NetworkManager in hierarchy
3. Modify settings in Inspector
4. Save scene
5. Done! ?

(With prefab approach, you'd need to: Open prefab ? Edit ? Apply ? Check all scenes ? More steps)

---

**This is the recommended approach for your project!** ??

Simple, visual, and works perfectly for your use case where networking starts at the lobby.
