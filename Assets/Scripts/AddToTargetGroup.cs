using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;

public class AddCameraTarget : NetworkBehaviour
{
    private CinemachineTargetGroup targetGroup;
    
    [Tooltip("Assign the PlayerCameraRoot transform here in the prefab")]
    public Transform cameraRoot; // Assign PlayerCameraRoot here in the prefab

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (!IsOwner) return; // Only owner should be target of the camera

        // Find the target group in the scene
        targetGroup = Object.FindFirstObjectByType<CinemachineTargetGroup>();   

        if (targetGroup != null && cameraRoot != null)
        {
            // Add this player's camera root to the target group
            targetGroup.AddMember(cameraRoot, 1f, 2f);
            Debug.Log($"[AddCameraTarget] Added camera target for player {OwnerClientId}");
        }
        else
        {
            if (targetGroup == null)
            {
                Debug.LogWarning("[AddCameraTarget] CinemachineTargetGroup not found in scene!");
            }
            if (cameraRoot == null)
            {
                Debug.LogWarning("[AddCameraTarget] Camera root not assigned!");
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        // Remove this player's camera target when they despawn
        if (targetGroup != null && cameraRoot != null)
        {
            targetGroup.RemoveMember(cameraRoot);
            Debug.Log($"[AddCameraTarget] Removed camera target for player {OwnerClientId}");
        }
    }
}
