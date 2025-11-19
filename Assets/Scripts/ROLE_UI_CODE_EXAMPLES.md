# Role-Based UI - Code Examples Reference

> **Note:** This is a documentation file with example code snippets.
> Copy and adapt these examples for your own scripts.

---

## Example 1: Basic Role Check

```csharp
using UnityEngine;

public class MyScript : MonoBehaviour
{
    void Start()
    {
        if (GameManager.Instance.IsInstructor())
        {
            Debug.Log("I am the instructor!");
        }

        if (GameManager.Instance.IsTrainee())
        {
            Debug.Log("I am a trainee!");
        }

        string role = GameManager.Instance.GetCurrentUserRole();
        Debug.Log($"My role: {role}");
    }
}
```

---

## Example 2: Instructor-Only Feature

```csharp
using UnityEngine;

public class VictimSpawner : MonoBehaviour
{
    [SerializeField] private GameObject victimPrefab;

    void Update()
    {
        // Only instructor can spawn victims
        if (GameManager.Instance.IsInstructor())
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                SpawnVictim();
            }
        }
    }

    private void SpawnVictim()
    {
        Vector3 pos = transform.position + Random.insideUnitSphere * 10f;
        Instantiate(victimPrefab, pos, Quaternion.identity);
    }
}
```

---

## Example 3: Different Camera Per Role

```csharp
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private GameObject firstPersonCamera;
    [SerializeField] private GameObject spectatorCamera;

    void Start()
    {
        if (GameManager.Instance.IsInstructor())
        {
            spectatorCamera.SetActive(true);
            firstPersonCamera.SetActive(false);
        }
        else
        {
            firstPersonCamera.SetActive(true);
            spectatorCamera.SetActive(false);
        }
    }
}
```

---

## Example 4: Role-Specific UI Updates

```csharp
using UnityEngine;
using TMPro;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    public void UpdateScore(int score)
    {
        if (GameManager.Instance.IsInstructor())
        {
            // Detailed stats for instructor
            scoreText.text = $"Team Score: {score}\n" +
                           $"Average: {score / 3}\n" +
                           $"Best: 100";
        }
        else
        {
            // Simple score for trainee
            scoreText.text = $"Your Score: {score}";
        }
    }
}
```

---

## Example 5: Conditional Input Handling

```csharp
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    void Update()
    {
        if (GameManager.Instance.IsInstructor())
        {
            HandleInstructorInput();
        }

        if (GameManager.Instance.IsTrainee())
        {
            HandleTraineeInput();
        }
    }

    private void HandleInstructorInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Reset simulation");
        }
    }

    private void HandleTraineeInput()
    {
        if (Input.GetKey(KeyCode.W))
        {
            Debug.Log("Move forward");
        }
    }
}
```

---

## Example 6: Role-Based Menu

```csharp
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject instructorMenu;
    [SerializeField] private GameObject traineeMenu;

    public void OpenMenu()
    {
        Time.timeScale = 0f;

        if (GameManager.Instance.IsInstructor())
        {
            instructorMenu.SetActive(true);
            traineeMenu.SetActive(false);
        }
        else
        {
            instructorMenu.SetActive(false);
            traineeMenu.SetActive(true);
        }
    }

    public void CloseMenu()
    {
        Time.timeScale = 1f;
        instructorMenu.SetActive(false);
        traineeMenu.SetActive(false);
    }
}
```

---

## Example 7: Feature Unlock

```csharp
using UnityEngine;

public class FeatureManager : MonoBehaviour
{
    [SerializeField] private GameObject instructorTools;
    [SerializeField] private GameObject traineeHUD;

    void Start()
    {
        instructorTools.SetActive(GameManager.Instance.IsInstructor());
        traineeHUD.SetActive(GameManager.Instance.IsTrainee());
    }
}
```

---

## Example 8: Networked Command (Instructor Only)

```csharp
using UnityEngine;
using Unity.Netcode;

public class NetworkCommand : NetworkBehaviour
{
    public void TriggerEvent()
    {
        if (GameManager.Instance.IsInstructor() && IsHost)
        {
            TriggerEventServerRpc();
        }
    }

    [ServerRpc]
    private void TriggerEventServerRpc()
    {
        TriggerEventClientRpc();
    }

    [ClientRpc]
    private void TriggerEventClientRpc()
    {
        Debug.Log("Event triggered!");
    }
}
```

---

## Example 9: Role-Specific Objectives

```csharp
using UnityEngine;
using TMPro;

public class ObjectivesPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text objectiveText;

    void Start()
    {
        if (GameManager.Instance.IsInstructor())
        {
            objectiveText.text = 
                "• Monitor trainee progress\n" +
                "• Manage simulation\n" +
                "• Evaluate performance";
        }
        else
        {
            objectiveText.text = 
                "• Rescue all victims\n" +
                "• Return to safe zone\n" +
                "• Complete on time";
        }
    }
}
```

---

## Example 10: Manual UI Toggle (Testing)

```csharp
using UnityEngine;

public class UITester : MonoBehaviour
{
    void Update()
    {
        // F1 = Show instructor UI
        if (Input.GetKeyDown(KeyCode.F1))
        {
            GameManager.Instance.ToggleRoleUI(true, false);
        }

        // F2 = Show trainee UI
        if (Input.GetKeyDown(KeyCode.F2))
        {
            GameManager.Instance.ToggleRoleUI(false, true);
        }

        // F3 = Restore automatic
        if (Input.GetKeyDown(KeyCode.F3))
        {
            bool showInst = GameManager.Instance.IsInstructor();
            bool showTrain = GameManager.Instance.IsTrainee();
            GameManager.Instance.ToggleRoleUI(showInst, showTrain);
        }
    }
}
```

---

## Tips and Best Practices

1. ? Always check if `GameManager.Instance` is not null
2. ? Use role checks for functionality, not just UI
3. ? Keep instructor and trainee code in separate methods
4. ? Don't hardcode roles - use the API
5. ? Test both single and multiplayer
6. ? Use `ToggleRoleUI()` only for testing
7. ? Remember: Instructor = Host, Trainee = Client
8. ? Log role info during development
9. ? Add fallbacks if role detection fails
10. ? Document role-specific features

---

## Common Patterns

### Pattern 1: Role-Based Initialization
```csharp
void Start()
{
    if (GameManager.Instance.IsInstructor())
    {
        InitializeInstructorMode();
    }
    else
    {
        InitializeTraineeMode();
    }
}
```

### Pattern 2: Conditional Feature Access
```csharp
public void PerformAction()
{
    if (!GameManager.Instance.IsInstructor())
    {
        Debug.LogWarning("Only instructor can do this!");
        return;
    }
    
    // Instructor-only code here
}
```

### Pattern 3: Dynamic UI Text
```csharp
string GetRoleSpecificText()
{
    return GameManager.Instance.IsInstructor() 
        ? "Instructor Mode" 
        : "Trainee Mode";
}
```

---

For complete documentation, see:
- `ROLE_BASED_UI_GUIDE.md`
- `ROLE_UI_SETUP_CHECKLIST.md`
- `ROLE_UI_TECHNICAL_OVERVIEW.md`
