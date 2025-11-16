using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Example: Network Player Manager for spawning and managing players in the game scene
/// This script should be placed in your game scene (e.g., TestKen, Flood, Earthquake)
/// </summary>
public class NetworkPlayerManager : NetworkBehaviour
{
    [Header("Player Prefab")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    private void Start()
    {
        // Only run if we're connected to a network session
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            // If we're the server/host, spawn players
            if (IsServer)
            {
                SpawnAllPlayers();
            }
        }
    }

    /// <summary>
    /// Spawn players for all connected clients (Server only)
    /// </summary>
    private void SpawnAllPlayers()
    {
        if (!IsServer) return;

        int spawnIndex = 0;
        
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            // Get spawn position
            Vector3 spawnPosition = GetSpawnPosition(spawnIndex);
            Quaternion spawnRotation = GetSpawnRotation(spawnIndex);

            // Instantiate player
            GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, spawnRotation);
            
            // Get NetworkObject component
            NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
            
            if (networkObject != null)
            {
                // Spawn as player object for this client
                networkObject.SpawnAsPlayerObject(client.Key);
                Debug.Log($"[NetworkPlayerManager] Spawned player for client {client.Key}");
            }
            else
            {
                Debug.LogError("[NetworkPlayerManager] Player prefab is missing NetworkObject component!");
                Destroy(playerInstance);
            }

            spawnIndex++;
        }
    }

    /// <summary>
    /// Get spawn position for player
    /// </summary>
    private Vector3 GetSpawnPosition(int index)
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            // Wrap around if more players than spawn points
            int spawnPointIndex = index % spawnPoints.Length;
            return spawnPoints[spawnPointIndex].position;
        }

        // Default spawn position with offset
        return new Vector3(index * 2f, 0f, 0f);
    }

    /// <summary>
    /// Get spawn rotation for player
    /// </summary>
    private Quaternion GetSpawnRotation(int index)
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int spawnPointIndex = index % spawnPoints.Length;
            return spawnPoints[spawnPointIndex].rotation;
        }

        return Quaternion.identity;
    }

    /// <summary>
    /// Example: Handle player disconnection
    /// </summary>
    public void OnPlayerDisconnected(ulong clientId)
    {
        if (!IsServer) return;

        Debug.Log($"[NetworkPlayerManager] Player {clientId} disconnected");
        
        // Find and destroy the player's NetworkObject
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            var playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            if (playerObject != null)
            {
                playerObject.Despawn();
                Debug.Log($"[NetworkPlayerManager] Despawned player object for client {clientId}");
            }
        }
    }
}

/* 
 * ============================================================================
 * PLAYER PREFAB SETUP INSTRUCTIONS
 * ============================================================================
 * 
 * To create a networked player prefab:
 * 
 * 1. Create your player GameObject with all necessary components:
 *    - Character Controller or Rigidbody
 *    - AgapThirdPersonController (your existing controller)
 *    - Animator
 *    - Camera setup
 *    - Any other player-specific components
 * 
 * 2. Add Netcode Components:
 *    - Add "NetworkObject" component
 *    - Add "NetworkTransform" component (syncs position/rotation)
 *    - Add "NetworkAnimator" component (syncs animations)
 * 
 * 3. Configure NetworkObject:
 *    - Check "Synchronize Transform" if using NetworkTransform
 *    - Set "Ownership" to "Owner Authoritative"
 * 
 * 4. Add to NetworkManager:
 *    - Save the prefab in a Resources folder or reference it directly
 *    - Add to NetworkManager's "Network Prefabs" list
 * 
 * 5. Update Player Controller:
 *    - Modify AgapThirdPersonController to check IsOwner before accepting input:
 *    
 *      public override void OnNetworkSpawn()
 *      {
 *          base.OnNetworkSpawn();
 *          
 *          // Only enable input for the local player
 *          if (!IsOwner)
 *          {
 *              GetComponent<PlayerInputs>().enabled = false;
 *              GetComponent<Camera>().enabled = false;
 *          }
 *      }
 * 
 * 6. Place NetworkPlayerManager in game scene:
 *    - Create empty GameObject called "NetworkPlayerManager"
 *    - Add this script
 *    - Assign player prefab
 *    - Assign spawn point transforms
 * 
 * ============================================================================
 * INTEGRATION WITH EXISTING GAME
 * ============================================================================
 * 
 * VictimSpawner:
 *   - Only the host should spawn victims
 *   - Add check: if (NetworkManager.Singleton.IsServer)
 *   - Victims need NetworkObject component to be visible to all clients
 * 
 * RoomManager:
 *   - Can store instructor/trainee role
 *   - Sync role using NetworkVariable or RPC
 * 
 * Game State:
 *   - Create NetworkGameManager to sync timer, score, etc.
 *   - Use NetworkVariables for synchronized state
 *   - Use RPCs for actions (rescuing victims, etc.)
 * 
 * ============================================================================
 */
