# Quick Reference - Three-Scene Lobby System

## 🎯 Scene Flow
```
Main Menu → Lobby → Game
(Code Gen)  (Settings) (Play)
```

## 📱 Main Menu Scene

### Components Needed:
- `NetworkManager` + `Unity Transport`
- `NetworkLobbyManager`
- `MainMenuLobbyController`

### Instructor UI:
- Generate Code Button
- Code Display Text
- Continue Button

### Trainee UI:
- Code Input Field
- Join Button

## 🏠 Lobby Scene

### Components Needed:
- `LobbySceneManager`

### UI Elements:
- Game Settings Panel (Instructor only)
  - Task count (+/- buttons)
  - Disaster dropdown
  - Duration dropdown
  - Start Game button
- Player List
  - Scroll View with Content
  - Player List Item Prefab
  - Shows: "INSTRUCTOR: name" or "TRAINEE: name"

## 🎮 Game Scenes

### Access Settings:
```csharp
int tasks = PlayerPrefs.GetInt("TaskCount");
string disaster = PlayerPrefs.GetString("DisasterType");
int duration = PlayerPrefs.GetInt("GameDuration");
```

## 🔑 Key PlayerPrefs

| Key | Set By | Value |
|-----|--------|-------|
| Type_Of_User | AuthManager | "instructor" / "trainee" |
| PlayerName | MainMenuLobbyController | User's full name |
| PlayerUsername | MainMenuLobbyController | User's username |
| CurrentLobbyCode | NetworkLobbyManager | 6-char code |
| TaskCount | LobbySceneManager | 0-8 |
| DisasterType | LobbySceneManager | "Flood"/"Earthquake"/"TestKen" |
| GameDuration | LobbySceneManager | Seconds (300/480/600) |

## 🎨 Required Prefabs

### Player Prefab:
- NetworkObject component
- NetworkPlayer component
- Add to NetworkManager → Network Prefabs

### Player List Item:
- UI Panel
- TMP_Text component
- Assign to LobbySceneManager

## 🔄 Instructor Workflow

```
1. Login as Instructor
2. Click "Generate Code"
3. Get code (e.g., A3X7B2)
4. Click "Continue"
5. Configure settings
6. Click "Start Game"
```

## 🔄 Trainee Workflow

```
1. Login as Trainee
2. Enter code
3. Click "Join"
4. Auto-go to lobby
5. Wait for instructor
6. Game auto-starts
```

## 🐛 Quick Troubleshooting

| Problem | Solution |
|---------|----------|
| No lobby panels | Call ShowLobbyCodePanel() |
| Names not showing | Check AuthManager data |
| Can't join | Verify code is 6 chars |
| Settings don't work | Check isInstructor flag |
| Scene won't load | Add to Build Settings |

## 📞 Quick Code Snippets

### Show Lobby Panel After Login:
```csharp
MainMenuLobbyController.Instance.ShowLobbyCodePanel();
```

### Get Player Data:
```csharp
string name = AuthManager.Instance.Current_Name;
string username = AuthManager.Instance.Current_Username;
```

### Check Role:
```csharp
bool isInstructor = PlayerPrefs.GetString("Type_Of_User") == "instructor";
```

### Get Lobby Code:
```csharp
string code = NetworkLobbyManager.Instance.GetCurrentLobbyCode();
```

## ✅ Setup Checklist

- [ ] Main Menu: NetworkManager + NetworkLobbyManager
- [ ] Main Menu: MainMenuLobbyController
- [ ] Main Menu: Instructor/Trainee panels
- [ ] Lobby: LobbySceneManager
- [ ] Lobby: Game settings UI
- [ ] Lobby: Player list UI
- [ ] Prefab: Player with NetworkObject + NetworkPlayer
- [ ] Prefab: Player List Item
- [ ] Build Settings: All 5 scenes added

## 📄 Documentation Files

- `UPDATED_SETUP_GUIDE.md` - Full setup guide
- `THREE_SCENE_SUMMARY.md` - Implementation summary
- `README.md` - Original documentation
- `QUICK_SETUP.md` - Original quick setup
- `ARCHITECTURE.md` - System architecture

---

**Version:** 2.0  
**Date:** Nov 16, 2025  
**Status:** ✅ Ready
