using TMPro;
using UnityEngine;
using Unity.Cinemachine;
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
    
    [Header("Cinemachine")]
    [SerializeField] private CinemachineTargetGroup targetGroup;
    
    private GameObject currentActiveObject;
    private GameObject firstInstance;
    private GameObject secondInstance;

    // Public properties to access the instances
    public GameObject FirstInstance => firstInstance;
    public GameObject SecondInstance => secondInstance;

    private void Awake()
    {
        // Ensure to initialize both prefab objects as inactive
        if (firstGameObject != null)
        {
            firstInstance = Instantiate(firstGameObject);
            firstInstance.SetActive(false);
            
            // Add boat camera target to target group if it exists
            Transform BoatCameraTarget = firstInstance.transform.Find("BoatCameraTarget");
            if (BoatCameraTarget != null && targetGroup != null)
            {
                targetGroup.AddMember(BoatCameraTarget, 1f, 1f);
            }
        }
        
        if (secondGameObject != null)
        {
            secondInstance = Instantiate(secondGameObject);
            secondInstance.SetActive(false);

            // Add player camera target to target group if it exists
            Transform PlayerCameraRoot = secondInstance.transform.Find("PlayerCameraRoot");
            if (PlayerCameraRoot != null && targetGroup != null)
            {
                targetGroup.AddMember(PlayerCameraRoot, 1f, 1f);
            }
        }
    }
    
    private void Start()
    {
        // Initialize - ensure first object is active
        if (firstInstance != null)
        {
            currentActiveObject = firstInstance;
            firstInstance.SetActive(true);
            if (secondInstance != null)
            {
                secondInstance.SetActive(false);
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
        if (firstInstance == null || secondInstance == null)
        {
            Debug.LogWarning("GameObjects not assigned in ModeSwitcher!");
            return;
        }
        
        GameObject previousObject;
        GameObject nextObject;
        string nextModeName;
        
        // Determine which object is currently active
        if (currentActiveObject == firstInstance)
        {
            previousObject = firstInstance;
            nextObject = secondInstance;
            nextModeName = secondModeName;
        }
        else
        {
            previousObject = secondInstance;
            nextObject = firstInstance;
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