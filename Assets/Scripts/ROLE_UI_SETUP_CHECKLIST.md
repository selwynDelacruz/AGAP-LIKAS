# Quick Setup Checklist: Role-Based UI

## ? Pre-Setup Verification

- [ ] Unity project is open
- [ ] GameManager exists in your game scene
- [ ] You have separate UI panels for Instructor and Trainee
- [ ] NetworkManager is set up in the scene
- [ ] Firebase authentication is working

---

## ?? Setup Steps

### 1. Create UI Panels (if not already created)

- [ ] Create a Canvas (if not exists)
- [ ] Create `InstructorUI` Panel as child of Canvas
  - Add instructor-specific UI elements (monitoring tools, controls, etc.)
- [ ] Create `TraineeUI` Panel as child of Canvas
  - Add trainee-specific UI elements (HUD, objectives, etc.)

### 2. Assign Panels to GameManager

- [ ] Select GameManager GameObject in Hierarchy
- [ ] Find "Role-Based UI Panels" section in Inspector
- [ ] Drag `InstructorUI` panel to **Instructor UI Panel** field
- [ ] Drag `TraineeUI` panel to **Trainee UI Panel** field
- [ ] ? Check **Debug Role UI** for testing

### 3. Test Configuration

#### Single Player Test:
- [ ] Login as Instructor
- [ ] Run the game
- [ ] Verify only Instructor UI is visible
- [ ] Logout and login as Trainee
- [ ] Run the game
- [ ] Verify only Trainee UI is visible

#### Multiplayer Test:
- [ ] **Build 1** (Instructor):
  - [ ] Login as instructor
  - [ ] Click "Host" in NetworkUI
  - [ ] Load game scene
  - [ ] Verify only Instructor UI visible
  
- [ ] **Build 2** (Trainee):
  - [ ] Login as trainee
  - [ ] Click "Join" in NetworkUI
  - [ ] Load game scene
  - [ ] Verify only Trainee UI visible

### 4. Verify Console Logs

Look for these messages in the Console:
```
? [GameManager] Role Detection - UserType: [instructor/trainee], IsHost: [True/False]
? [GameManager] Instructor UI Panel: [ENABLED/DISABLED]
? [GameManager] Trainee UI Panel: [ENABLED/DISABLED]
```

---

## ?? Common Issues Checklist

### Both UIs showing?
- [ ] Check if UI panels are assigned in GameManager Inspector
- [ ] Verify NetworkManager exists before GameManager in script execution order
- [ ] Check if PlayerPrefs has "Type_Of_User" set (check in AuthManager)

### No UI showing?
- [ ] Ensure UI Panel GameObjects are in the scene (not prefabs)
- [ ] Check Canvas settings (should be Screen Space - Overlay or Camera)
- [ ] Verify UI panels are not disabled in the scene by default

### Wrong UI showing?
- [ ] Clear PlayerPrefs: `PlayerPrefs.DeleteAll()` in a test script
- [ ] Verify instructor is hosting AND logged in as instructor
- [ ] Verify trainee is client AND logged in as trainee

---

## ?? Optional Customization

- [ ] Adjust `debugRoleUI` setting (disable in production)
- [ ] Customize role detection logic in `ShouldShowInstructorUI()` / `ShouldShowTraineeUI()`
- [ ] Add role-specific features in your custom scripts using `GameManager.Instance.IsInstructor()`

---

## ?? Testing Notes

**Test Date:** ___________

**Tester:** ___________

**Results:**
- [ ] Instructor UI works correctly
- [ ] Trainee UI works correctly
- [ ] No UI overlap issues
- [ ] Console logs are clear
- [ ] Multiplayer tested successfully

**Issues Found:**
___________________________________________
___________________________________________
___________________________________________

---

## ? You're Done!

Once all checkboxes are complete, your role-based UI system is ready for production.

For detailed information, see: `ROLE_BASED_UI_GUIDE.md`
