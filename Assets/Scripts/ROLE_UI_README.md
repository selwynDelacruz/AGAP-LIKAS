# ?? Role-Based UI System Documentation Index

Welcome to the Role-Based UI system for AGAP-LIKAS!

This system ensures that **Instructors (Hosts)** see only instructor-specific UI, while **Trainees (Clients)** see only trainee-specific UI.

---

## ?? Quick Start

### 1. Read This First
Start with **`IMPLEMENTATION_SUMMARY.md`** for an overview of what was implemented.

### 2. Setup Your UI
Follow **`ROLE_UI_SETUP_CHECKLIST.md`** step-by-step to configure your UI panels.

### 3. Learn How It Works
Read **`ROLE_BASED_UI_GUIDE.md`** for detailed explanation and usage.

---

## ?? Documentation Files

| File | Purpose | When to Read |
|------|---------|-------------|
| **IMPLEMENTATION_SUMMARY.md** | Overview of changes | ?? **Start Here** |
| **ROLE_UI_SETUP_CHECKLIST.md** | Step-by-step setup | ?? During setup |
| **ROLE_BASED_UI_GUIDE.md** | Complete guide | ?? Reference |
| **ROLE_UI_TECHNICAL_OVERVIEW.md** | Technical diagrams | ?? Deep dive |
| **ROLE_UI_CODE_EXAMPLES.md** | Code examples | ?? When coding |

---

## ?? Key Features

? **Automatic Role Detection**
- Reads user type from PlayerPrefs (set during login)
- Checks network status (Host vs Client)

? **UI Panel Management**
- Instructor UI shown only to hosts
- Trainee UI shown only to clients
- No manual toggle needed

? **Public API**
```csharp
GameManager.Instance.IsInstructor()  // Check if instructor
GameManager.Instance.IsTrainee()     // Check if trainee
GameManager.Instance.GetCurrentUserRole()  // Get role string
```

? **Debug Support**
- Toggle debug logging in Inspector
- Detailed console messages
- Validation warnings

---

## ??? Setup Overview

### Prerequisites
- Unity project open
- GameManager in scene
- NetworkManager configured
- Firebase authentication working

### Quick Setup (3 Steps)

1. **Create UI Panels**
   ```
   Canvas
   ??? InstructorUI (Panel)
   ??? TraineeUI (Panel)
   ```

2. **Assign in Inspector**
   - Select GameManager
   - Assign Instructor UI Panel
   - Assign Trainee UI Panel

3. **Test**
   - Login as instructor ? Host
   - Login as trainee ? Join
   - Verify correct UI shows

---

## ?? Usage Examples

### Example 1: Check Role
```csharp
if (GameManager.Instance.IsInstructor())
{
    // Instructor-only code
}
```

### Example 2: Instructor-Only Feature
```csharp
void Update()
{
    if (GameManager.Instance.IsInstructor())
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            SpawnVictim();
        }
    }
}
```

### Example 3: Role-Specific UI
```csharp
void UpdateScore(int score)
{
    if (GameManager.Instance.IsInstructor())
    {
        scoreText.text = $"Team: {score}";
    }
    else
    {
        scoreText.text = $"You: {score}";
    }
}
```

More examples in **`ROLE_UI_CODE_EXAMPLES.md`**

---

## ?? How It Works

```
User Login ? PlayerPrefs Set ? Network Connect ? GameManager Detects Role ? Show UI
```

### Role Determination

**Instructor UI Shows When:**
- User type = "instructor" AND
- User is network host

**Trainee UI Shows When:**
- User type = "trainee" OR
- User is network client

---

## ?? Testing

### Single Player
- [x] Login as instructor ? See instructor UI
- [x] Login as trainee ? See trainee UI

### Multiplayer
- [x] Instructor hosts ? See instructor UI
- [x] Trainee joins ? See trainee UI
- [x] Multiple trainees ? All see trainee UI

---

## ?? Troubleshooting

### Both UIs Showing?
1. Check Inspector assignments
2. Enable debug logging
3. Verify NetworkManager exists
4. Check PlayerPrefs has "Type_Of_User"

### No UI Showing?
1. Ensure panels are in scene
2. Check Canvas settings
3. Verify panels not disabled by default
4. Check console for errors

### Wrong UI Showing?
1. Clear PlayerPrefs
2. Verify role matches network status
3. Check console warnings

See **`ROLE_BASED_UI_GUIDE.md`** for detailed troubleshooting.

---

## ?? Code Location

All role-based UI code is in **`Assets\Scripts\GameManager.cs`**

### Key Methods
- `InitializeRoleDetection()` - Detects user role
- `InitializeRoleBasedUI()` - Shows/hides UI
- `ShouldShowInstructorUI()` - Checks if instructor UI needed
- `ShouldShowTraineeUI()` - Checks if trainee UI needed
- `IsInstructor()` - Public API
- `IsTrainee()` - Public API
- `GetCurrentUserRole()` - Public API
- `ToggleRoleUI()` - Manual override (testing)

---

## ?? Integration

### With AuthManager
```
Login ? Firebase Auth ? PlayerPrefs.SetString("Type_Of_User", role)
```

### With NetworkManager
```
Instructor ? Click "Host" ? NetworkManager.StartHost()
Trainee ? Click "Join" ? NetworkManager.StartClient()
```

### With GameManager
```
GameManager.Start() ? Detect Role ? Show Appropriate UI
```

---

## ? Best Practices

1. ? Use role checks for features, not just UI
2. ? Keep instructor/trainee code separate
3. ? Test both roles thoroughly
4. ? Log role info during development
5. ? Don't hardcode roles
6. ? Handle missing role gracefully
7. ? Document role-specific features
8. ? Use ToggleRoleUI() only for testing

---

## ?? Related Files

- `AuthManager.cs` - Firebase authentication
- `NetworkUI.cs` - Network connection UI
- `GameManager.cs` - **Core implementation**
- `LobbyManager.cs` - Pre-game setup

---

## ?? Support

For issues or questions:
1. Check troubleshooting in **`ROLE_BASED_UI_GUIDE.md`**
2. Enable `debugRoleUI` in GameManager Inspector
3. Review console logs
4. Check setup against **`ROLE_UI_SETUP_CHECKLIST.md`**

---

## ? Build Status

- ? Code compiles successfully
- ? No errors or warnings
- ? Ready for production
- ? Documentation complete

---

## ?? You're Ready!

The role-based UI system is fully implemented and documented.

**Start with: `IMPLEMENTATION_SUMMARY.md`**

**Then follow: `ROLE_UI_SETUP_CHECKLIST.md`**

Happy coding! ??
