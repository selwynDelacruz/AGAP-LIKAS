using TMPro;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ModeSwitcher : MonoBehaviour
{
    [Header("GameObjects to Switch")]
    [SerializeField] private GameObject firstGameObject;
    [SerializeField] private GameObject secondGameObject;
    
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI modeText;
    
    [Header("Mode Names")]
    [SerializeField] private string firstModeName = "First Mode";
    [SerializeField] private string secondModeName = "Second Mode";
    
    private GameObject currentActiveObject;
    
    private void Start()
    {
        // Initialize - ensure first object is active
        if (firstGameObject != null)
        {
            currentActiveObject = firstGameObject;
            firstGameObject.SetActive(true);
            if (secondGameObject != null)
            {
                secondGameObject.SetActive(false);
            }
            UpdateModeText(firstModeName);
        }
    }
    
    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            SwitchMode();
        }
#else
        if (Input.GetKeyDown(KeyCode.T))
        {
            SwitchMode();
        }
#endif
    }
    
    private void SwitchMode()
    {
        if (firstGameObject == null || secondGameObject == null)
        {
            Debug.LogWarning("GameObjects not assigned in ModeSwitcher!");
            return;
        }
        
        GameObject previousObject;
        GameObject nextObject;
        string nextModeName;
        
        // Determine which object is currently active
        if (currentActiveObject == firstGameObject)
        {
            previousObject = firstGameObject;
            nextObject = secondGameObject;
            nextModeName = secondModeName;
        }
        else
        {
            previousObject = secondGameObject;
            nextObject = firstGameObject;
            nextModeName = firstModeName;
        }
        
        // Copy position and rotation
        nextObject.transform.position = previousObject.transform.position;
        nextObject.transform.rotation = previousObject.transform.rotation;
        
        // Switch active states
        previousObject.SetActive(false);
        nextObject.SetActive(true);
        
        // Update current active object reference
        currentActiveObject = nextObject;
        
        // Update UI
        UpdateModeText(nextModeName);
    }
    
    private void UpdateModeText(string modeName)
    {
        if (modeText != null)
        {
            modeText.text = modeName;
        }
    }
}