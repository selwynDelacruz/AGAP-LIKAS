using UnityEngine;
using Unity.Cinemachine;

public class InstructorCamera : MonoBehaviour
{
    [Header("Manual Rotation Settings")]
    [SerializeField] private float manualRotationSpeed = 100f; // Degrees per second
    [SerializeField] private KeyCode rotateLeftKey = KeyCode.Q;
    [SerializeField] private KeyCode rotateRightKey = KeyCode.E;
    [SerializeField] private float mouseRotationSpeed = 5f;
    [SerializeField] private bool enableMouseRotation = true;
    
    private CinemachineCamera cinemachineCamera;
    private CinemachineOrbitalFollow orbitalTransposer;

    void Start()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();
        
        if (cinemachineCamera != null)
        {
            orbitalTransposer = cinemachineCamera.GetComponent<CinemachineOrbitalFollow>();
            
            if (orbitalTransposer == null)
            {
                Debug.LogWarning("CinemachineOrbitalFollow component not found. Please set Position Control to 'Orbital Transposer' in the Inspector.");
            }
        }
        else
        {
            Debug.LogError("CinemachineCamera component not found on this GameObject.");
        }
    }

    void Update()
    {
        if (orbitalTransposer == null) return;

        // Keyboard rotation
        float keyboardInput = 0f;
        if (Input.GetKey(rotateLeftKey))
            keyboardInput = -1f;
        else if (Input.GetKey(rotateRightKey))
            keyboardInput = 1f;

        if (keyboardInput != 0f)
        {
            orbitalTransposer.HorizontalAxis.Value += keyboardInput * manualRotationSpeed * Time.deltaTime;
        }

        // Mouse rotation (hold right mouse button and drag)
        if (enableMouseRotation && Input.GetMouseButton(1)) // Right mouse button
        {
            float mouseX = Input.GetAxis("Mouse X");
            orbitalTransposer.HorizontalAxis.Value += mouseX * mouseRotationSpeed;
        }

        // Keep value within 0-360 range
        if (orbitalTransposer.HorizontalAxis.Value > 360f)
            orbitalTransposer.HorizontalAxis.Value -= 360f;
        else if (orbitalTransposer.HorizontalAxis.Value < 0f)
            orbitalTransposer.HorizontalAxis.Value += 360f;
    }
}
