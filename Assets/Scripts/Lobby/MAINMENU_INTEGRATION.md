# MainMenu Integration Guide

## ?? Goal
Integrate the new LAN-based lobby system into your existing MainMenu after users log in.

---

## ?? Step-by-Step Instructions

### **STEP 1: Clean Up Old Lobby System**

#### **1.1 Remove Old LobbyManager.cs References**

The file `Assets/Scripts/LobbyManager.cs` is your **OLD** lobby system. Here's what to do:

**In Unity:**
1. Search for any scenes using `LobbyManager`:
   - Open each game scene (Flood, Earthquake, TestKen)
   - Find any `LobbyManager` component
   - **Remove it** (or make note that it's different from the new system)

2. **Check if there's an old "Lobby" scene:**
   - File ? Build Settings
   - Look for a scene called "Lobby" (not "LobbyMenu")
   - If it exists and it's the old system, you can remove it from build settings
   - **Don't delete the scene file yet** - just in case you need to reference the UI

**Keep this file for reference but you won't use it anymore:**
- `Assets/Scripts/LobbyManager.cs` - This was for non-networked lobby

---

### **STEP 2: Update MainMenu.cs**

Your `MainMenu.cs` already has the correct `GoToLobby()` method:

```csharp
public void GoToLobby()
{
    Debug.Log("Loading Lobby Menu scene...");
    SceneManager.LoadScene("LobbyMenu"); // ? This is correct now
}
```

**This is already done!** ?

---

### **STEP 3: Connect "Play Game" Button in MainMenu**

#### **In MainMenu Scene (Unity):**

1. **Open MainMenu scene**

2. **Find your "Play Game" button** (the one that appears after login)
   - This button should be in either:
     - `MenuPanel_Instructor` (for instructors)
     - `MenuPanel_Trainee` (for trainees)

3. **Connect the button:**
   - Select the "Play Game" button
   - In Inspector ? Button component ? OnClick()
   - Click the **+** to add a new event
   - Drag the **MainMenu** GameObject (or the object with MainMenu.cs) into the object field
   - Select `MainMenu.GoToLobby` from the dropdown

**Visual Guide:**
```
Button Inspector:
???????????????????????????????
? Button (Script)             ?
?                             ?
? OnClick()                   ?
? ??????????????????????????? ?
? ? Runtime   MainMenu      ? ?
? ? ?? MainMenu.GoToLobby() ? ?
? ??????????????????????????? ?
???????????????????????????????
```

---

### **STEP 4: Verify Authentication Connection**

Your `AuthManager.cs` already sets the user role correctly. Just verify:

**In `AuthManager.cs` - After successful login:**

```csharp
// ? This should already exist in your Login coroutine
PlayerPrefs.SetString("Type_Of_User", "instructor"); // or "trainee"
PlayerPrefs.SetString("Name", userName); // User's name
PlayerPrefs.Save();
```

**Check these lines exist in:**
- `IEnumerator Login()` method
- Around line ~520-580 in your AuthManager.cs

---

### **STEP 5: Scene Setup Checklist**

#### **MainMenu Scene:**
- [ ] Has `MainMenu.cs` script
- [ ] "Play Game" button calls `MainMenu.GoToLobby()`
- [ ] ? **NO** NetworkManager
- [ ] ? **NO** LobbyMenuManager
- [ ] ? **NO** network components

#### **LobbyMenu Scene:**
- [ ] Has `NetworkManager` GameObject with:
  - [ ] `NetworkManager` component (DontDestroyOnLoad ?)
  - [ ] `UnityTransport` component (Port: 7777)
- [ ] Has `UnityMainThreadDispatcher` GameObject
- [ ] Has `LobbyMenuManager` GameObject
- [ ] Has Create Lobby Panel (for instructors)
- [ ] Has Join Lobby Panel (for trainees)
- [ ] All UI references assigned in LobbyMenuManager Inspector
- [ ] Scene is in Build Settings

---

### **STEP 6: Build Settings Order**

**File ? Build Settings ? Scenes In Build:**

```
? 0. MainMenu
? 1. LobbyMenu    ? NEW lobby system
? 2. LobbyRoom
? 3. Flood
? 4. Earthquake
? 5. TestKen
? 6. Result

? X. Lobby (old)  ? Can be removed if it's the old system
```

---

### **STEP 7: Test the Integration**

#### **Test Flow:**

1. **Play MainMenu scene**
2. **Login as Instructor**
   ```
   Expected console:
   - "Logged in as Instructor"
   - PlayerPrefs: Type_Of_User = "instructor"
   ```

3. **Click "Play Game" button**
   ```
   Expected console:
   - "Loading Lobby Menu scene..."
   - Scene loads to LobbyMenu
   ```

4. **In LobbyMenu scene:**
   ```
   Expected console:
   - "[LobbyMenuManager] NetworkManager set to DontDestroyOnLoad"
   - "[LobbyMenuManager] User role: instructor"
   - "[LobbyMenuManager] Instructor UI enabled"
   
   Expected UI:
   - Create Lobby panel is visible
   - Join Lobby panel is hidden
   - "Create Lobby" button is clickable
   ```

5. **Click "Create Lobby"**
   ```
   Expected console:
   - "[LobbyMenuManager] Generated lobby code: XXXX42"
   - "[LobbyMenuManager] Started as Host on 0.0.0.0:7777"
   - "[LobbyBroadcaster] Started broadcasting"
   
   Expected behavior:
   - Lobby code displays immediately
   - Wait 2 seconds
   - Scene loads to LobbyRoom
   ```

6. **In LobbyRoom:**
   ```
   Expected UI:
   - Host panel visible (configuration options)
   - Client panel hidden
   - Lobby code displayed
   - Can configure: Tasks, Disaster, Duration
   - "START GAME" button works
   ```

---

### **STEP 8: What Changed from Old System**

| Aspect | Old (LobbyManager.cs) | New (LobbyMenuManager.cs) |
|--------|----------------------|---------------------------|
| **Scene** | Probably "Lobby" | "LobbyMenu" |
| **Networking** | None (local only) | Full LAN multiplayer |
| **Role Detection** | Manual selection? | Automatic from PlayerPrefs |
| **Lobby Code** | N/A | Generated automatically |
| **Host/Client** | N/A | Host (Instructor) / Client (Trainee) |
| **Settings Sync** | Local only | Synced via NetworkVariables |
| **Scene Loading** | SceneManager | NetworkManager (synced) |

---

### **STEP 9: Update Any References to Old Lobby**

**Search your project for:**

1. **"Lobby" scene references:**
   ```csharp
   // ? OLD
   SceneManager.LoadScene("Lobby");
   
   // ? NEW
   SceneManager.LoadScene("LobbyMenu");
   ```

2. **LobbyManager references:**
   ```csharp
   // ? OLD (if you had this)
   LobbyManager.SelectedTaskCount
   
   // ? NEW (settings come from PlayerPrefs now)
   int taskCount = PlayerPrefs.GetInt("TaskCount");
   ```

3. **Check your game scenes (Flood, Earthquake, TestKen):**
   - Make sure they read settings from PlayerPrefs
   - Settings are set by `LobbyRoomManager` before loading game scene

---

## ?? Quick Migration Summary

### **What to Keep:**
- ? `MainMenu.cs` - Already updated
- ? `AuthManager.cs` - Already sets PlayerPrefs correctly
- ? Your existing game scenes
- ? Your existing authentication flow

### **What to Remove/Update:**
- ? Old `LobbyManager` component from scenes (if used)
- ? References to old "Lobby" scene
- ? Update button to call `MainMenu.GoToLobby()`
- ? Create new `LobbyMenu` scene (if not done yet)

### **What's New:**
- ? `LobbyMenu` scene with NetworkManager
- ? `LobbyMenuManager.cs` handles lobby creation/joining
- ? `LobbyRoom` scene for game configuration
- ? LAN networking for multiplayer

---

## ?? Troubleshooting

### **Issue: Button doesn't load LobbyMenu**
**Check:**
- Button's OnClick() is connected to `MainMenu.GoToLobby()`
- `LobbyMenu` scene exists and is in Build Settings
- Console shows "Loading Lobby Menu scene..."

### **Issue: Wrong UI panel shows in LobbyMenu**
**Check:**
- PlayerPrefs has correct `Type_Of_User` value
- Run: `Debug.Log(PlayerPrefs.GetString("Type_Of_User"));` after login
- Should be exactly "instructor" or "trainee" (lowercase)

### **Issue: NetworkManager not found**
**Check:**
- LobbyMenu scene has NetworkManager GameObject
- NetworkManager has both components (NetworkManager + UnityTransport)
- "Don't Destroy On Load" is CHECKED

---

## ? Final Checklist

Before testing:

- [ ] Old "Lobby" scene removed from Build Settings (or renamed for reference)
- [ ] "Play Game" button in MainMenu connects to `MainMenu.GoToLobby()`
- [ ] `MainMenu.cs` has correct scene name: `"LobbyMenu"`
- [ ] `LobbyMenu` scene created with all required components
- [ ] `LobbyRoom` scene created with all required components
- [ ] All scenes in Build Settings in correct order
- [ ] PlayerPrefs set correctly in AuthManager
- [ ] Tested instructor login ? Create Lobby flow
- [ ] Tested trainee login ? Join Lobby flow

---

## ?? You're Done!

Your new lobby system is now integrated with your authentication system!

**Flow:**
```
Login ? MainMenu ? Click "Play Game" ? LobbyMenu ? Create/Join Lobby ? LobbyRoom ? Game
```

**Next Steps:**
1. Test locally (single player)
2. Build and test on LAN (multiplayer)
3. Customize UI to match your game's theme

---

**Need Help?**
- Check `SETUP_GUIDE.md` for detailed scene setup
- Check `TROUBLESHOOTING.md` for common issues
- Check console logs with Debug Mode enabled
