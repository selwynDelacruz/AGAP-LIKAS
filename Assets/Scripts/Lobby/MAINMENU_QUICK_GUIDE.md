# MainMenu ? Lobby Integration - Quick Guide

## ?? **5-Minute Setup**

### **1. Connect the Button (In Unity)**

**MainMenu scene:**
```
1. Select "Play Game" button
2. Inspector ? Button ? OnClick()
3. Add event: MainMenu.GoToLobby()
```

**That's it for MainMenu!**

---

### **2. Code is Already Updated**

**MainMenu.cs (Line ~18):**
```csharp
public void GoToLobby()
{
    Debug.Log("Loading Lobby Menu scene...");
    SceneManager.LoadScene("LobbyMenu"); // ? Already correct!
}
```

**No code changes needed!** ?

---

### **3. Authentication Already Works**

**AuthManager.cs already sets:**
```csharp
PlayerPrefs.SetString("Type_Of_User", "instructor" or "trainee");
PlayerPrefs.SetString("Name", userName);
```

**LobbyMenuManager.cs automatically reads it:**
```csharp
userRole = PlayerPrefs.GetString("Type_Of_User");
// Shows correct UI based on role
```

**No code changes needed!** ?

---

## ?? **Scene Structure**

### **MainMenu Scene (No Network):**
```
MainMenu
?? AuthManager (handles login)
?? MenuPanel_Instructor
?  ?? PlayGameButton ? MainMenu.GoToLobby()
?? MenuPanel_Trainee
   ?? PlayGameButton ? MainMenu.GoToLobby()
```

### **LobbyMenu Scene (Network Starts Here):**
```
LobbyMenu
?? NetworkManager (DontDestroyOnLoad ?)
?? UnityMainThreadDispatcher
?? LobbyMenuManager
?? Canvas
   ?? CreateLobbyPanel (Instructor)
   ?? JoinLobbyPanel (Trainee)
```

---

## ?? **Complete Flow**

```
1. User opens game
2. User logs in as Instructor/Trainee
   ?? PlayerPrefs.SetString("Type_Of_User", role)
3. User clicks "Play Game"
   ?? MainMenu.GoToLobby()
   ?? SceneManager.LoadScene("LobbyMenu")
4. LobbyMenu loads
   ?? NetworkManager spawns (DontDestroyOnLoad)
   ?? LobbyMenuManager reads PlayerPrefs
   ?? Shows correct UI (Create or Join)
5. Instructor: Create Lobby
   ?? Generates code
   ?? Starts host
   ?? Loads LobbyRoom
6. Trainee: Join Lobby
   ?? Enters code
   ?? Scans LAN
   ?? Connects to host
   ?? Follows to LobbyRoom
```

---

## ? **Verification Steps**

### **Test 1: UI Connection**
1. Play MainMenu scene
2. Login as Instructor
3. Click "Play Game"
4. **Expected:** Console shows "Loading Lobby Menu scene..."
5. **Expected:** LobbyMenu scene loads

### **Test 2: Role Detection**
1. In LobbyMenu, open Console
2. **Expected Console:**
   ```
   [LobbyMenuManager] User role: instructor
   [LobbyMenuManager] Instructor UI enabled - Create Lobby panel shown
   ```
3. **Expected UI:** Create Lobby panel visible, Join panel hidden

### **Test 3: Create Lobby**
1. Click "Create Lobby" button
2. **Expected Console:**
   ```
   [LobbyMenuManager] Generated lobby code: XXXX42
   [LobbyMenuManager] Started as Host on 0.0.0.0:7777
   ```
3. **Expected:** Lobby code displays, then loads LobbyRoom

---

## ?? **What You Need to Do**

### **In Unity (5 minutes):**

**Step 1: Open MainMenu scene**

**Step 2: Find "Play Game" button**
- It's in either:
  - `MenuPanel_Instructor` 
  - `MenuPanel_Trainee`

**Step 3: Connect button**
```
Inspector ? Button component
OnClick()
  ?? + (add event)
     ?? Drag: MainMenu GameObject
     ?? Function: MainMenu ? GoToLobby()
```

**Step 4: Save scene**

**Step 5: Test!**
- Play MainMenu
- Login
- Click "Play Game"
- Should load LobbyMenu

---

## ?? **Common Issues**

### **Button doesn't work**
- ? Check: Button has `onClick` listener
- ? Check: MainMenu GameObject is assigned
- ? Check: Function selected is `GoToLobby()`

### **Wrong scene loads**
- ? Check: `MainMenu.cs` line 18 says `"LobbyMenu"`
- ? Check: `LobbyMenu` scene exists in Build Settings

### **Wrong UI shows in LobbyMenu**
- ? Check: PlayerPrefs has correct role
- ? Add debug: `Debug.Log(PlayerPrefs.GetString("Type_Of_User"));`
- ? Should be exactly "instructor" or "trainee"

---

## ?? **File Organization**

### **Keep These:**
```
Assets/Scripts/
?? MainMenu.cs ? Already updated ?
?? AuthManager.cs ? Already sets PlayerPrefs ?
?? Lobby/
   ?? LobbyMenuManager.cs ? NEW lobby system
   ?? LobbyRoomManager.cs
   ?? [other lobby scripts]
```

### **Old System (Can ignore):**
```
Assets/Scripts/
?? LobbyManager.cs ? OLD system (not used anymore)
```

---

## ?? **Summary**

### **Code Changes: ZERO!** ?
Everything is already coded correctly:
- `MainMenu.cs` ? Already loads "LobbyMenu" ?
- `AuthManager.cs` ? Already sets PlayerPrefs ?
- `LobbyMenuManager.cs` ? Already reads PlayerPrefs ?

### **Unity Work: 5 Minutes**
Just connect the button:
1. Open MainMenu scene
2. Select "Play Game" button
3. Connect onClick ? MainMenu.GoToLobby()
4. Test!

---

**That's it!** Your lobby system is ready to go! ??

For detailed setup instructions, see `MAINMENU_INTEGRATION.md`
For scene setup, see `SETUP_GUIDE.md`
