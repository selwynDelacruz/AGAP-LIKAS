using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lobby
{
    /// <summary>
    /// Spawns player prefabs only when a gameplay scene (e.g. TestKen) loads.
    /// Attach this to the persistent NetworkManager object.
    /// Supports different player prefabs based on disaster mode (Flood = Boat, Earthquake = Walking)
    /// Also spawns instructor spectator for instructor users (instructors do NOT get player prefabs).
    /// </summary>
    public class PlayerSpawnManager : MonoBehaviour
    {
        [Header("Player Prefabs (Must have NetworkObject)")] 
        [Tooltip("Player prefab for walking mode (Earthquake)")]
        [SerializeField] private GameObject walkingPlayerPrefab;
        
        [Tooltip("Player prefab for boat mode (Flood)")]
        [SerializeField] private GameObject boatPlayerPrefab;

        [Header("Instructor Spectator")]
        [Tooltip("Instructor spectator prefab (non-networked, local only)")]
        [SerializeField] private GameObject instructorSpectatorPrefab;

        [Header("Gameplay Scenes That Require Spawning")]
        [SerializeField] private string[] gameplaySceneNames = {"Flood","Earthquake","TestKen"};

        [Header("Spawn Settings")]
        [SerializeField] private Vector3 hostStartPosition = new Vector3(0, 0, 0);
        [SerializeField] private Vector3 clientStartOffset = new Vector3(2, 0, 0); // Offset per client

        [Header("Flood Mode Spawn Adjustment")]
        [Tooltip("Y offset for boat spawning in Flood mode (should match MapSpawner's floodYAdjustment)")]
        [SerializeField] private float floodYOffset = 1f;

        [Header("Map Spawn Waiting")]
        [Tooltip("Wait for MapSpawner to finish before spawning players")]
        [SerializeField] private bool waitForMaps = true;
        
        [Tooltip("Maximum time to wait for maps before spawning anyway (seconds)")]
        [SerializeField] private float maxWaitTime = 5f;

        [SerializeField] private bool showDebugLogs = true;

        private bool pendingSpawn = false;
        private string pendingSceneName = "";
        private GameObject _instructorSpectatorInstance;

        // Current disaster type cached for spawn position calculations
        private string currentDisasterType = "Earthquake";

        // Cache to track which clients are instructors (clientId -> isInstructor)
        private Dictionary<ulong, bool> _clientInstructorStatus = new Dictionary<ulong, bool>();

        private void Awake()
        {
            if (walkingPlayerPrefab == null)
            {
                Debug.LogWarning("[PlayerSpawnManager] Walking player prefab not assigned.");
            }
            
            if (boatPlayerPrefab == null)
            {
                Debug.LogWarning("[PlayerSpawnManager] Boat player prefab not assigned.");
            }

            if (instructorSpectatorPrefab == null)
            {
                Debug.LogWarning("[PlayerSpawnManager] Instructor spectator prefab not assigned. Will attempt to load from Resources.");
            }
        }

        private void OnEnable()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null)
            {
                Debug.LogWarning("[PlayerSpawnManager] NetworkManager.Singleton is null on OnEnable. Will retry on first Update.");
                return;
            }
            nm.OnClientConnectedCallback += OnClientConnected;
            // SceneManager may be null until NetworkManager initializes modules
            if (nm.SceneManager != null)
            {
                nm.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
            }
            else
            {
                // Subscribe once server starts so SceneManager is available
                nm.OnServerStarted += SubscribeSceneEventsSafely;
            }

            // Subscribe to MapSpawner event
            MapSpawner.OnMapsSpawned += OnMapsSpawned;
        }

        private void SubscribeSceneEventsSafely()
        {
            var nm = NetworkManager.Singleton;
            if (nm != null && nm.SceneManager != null)
            {
                nm.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted; // avoid duplicate
                nm.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
                if (showDebugLogs) Debug.Log("[PlayerSpawnManager] SceneManager subscription added after server start.");
            }
            if (nm != null)
            {
                nm.OnServerStarted -= SubscribeSceneEventsSafely;
            }
        }

        private void OnDisable()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null) return;
            nm.OnClientConnectedCallback -= OnClientConnected;
            if (nm.SceneManager != null)
            {
                nm.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
            }
            nm.OnServerStarted -= SubscribeSceneEventsSafely;

            // Unsubscribe from MapSpawner event
            MapSpawner.OnMapsSpawned -= OnMapsSpawned;
        }

        private void OnDestroy()
        {
            // Clean up instructor spectator instance
            if (_instructorSpectatorInstance != null)
            {
                Destroy(_instructorSpectatorInstance);
                _instructorSpectatorInstance = null;
            }

            // Clear instructor status cache
            _clientInstructorStatus.Clear();
        }

        private bool IsGameplayScene(string sceneName)
        {
            return gameplaySceneNames.Contains(sceneName);
        }

        private void OnMapsSpawned()
        {
            if (!NetworkManager.Singleton.IsServer) return;
            
            if (showDebugLogs) Debug.Log("[PlayerSpawnManager] Maps spawned event received. Spawning players now.");
            
            if (pendingSpawn)
            {
                pendingSpawn = false;
                SpawnAllPlayersIfNeeded();
            }
        }

        private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (!NetworkManager.Singleton.IsServer) return; // Only server/host spawns
            if (!IsGameplayScene(sceneName)) return; // Only spawn in gameplay scenes
            
            if (showDebugLogs) Debug.Log($"[PlayerSpawnManager] Scene '{sceneName}' loaded.");

            // Cache the disaster type for spawn position calculations
            currentDisasterType = PlayerPrefs.GetString("DisasterType", "Earthquake");
            if (showDebugLogs) Debug.Log($"[PlayerSpawnManager] Disaster type: {currentDisasterType}");

            // Determine if local user (host) is an instructor
            string localUserType = PlayerPrefs.GetString("Type_Of_User", "");
            bool isLocalInstructor = localUserType == "instructor";
            
            // Cache the instructor status for the local client (host)
            if (NetworkManager.Singleton.IsHost)
            {
                ulong hostClientId = NetworkManager.Singleton.LocalClientId;
                _clientInstructorStatus[hostClientId] = isLocalInstructor;
                
                if (showDebugLogs)
                {
                    Debug.Log($"[PlayerSpawnManager] Host client {hostClientId} is instructor: {isLocalInstructor}");
                }
            }

            // Spawn instructor spectator if local user is instructor (only on host/server)
            if (isLocalInstructor)
            {
                SpawnInstructorSpectatorIfNeeded();
            }

            if (waitForMaps && !MapSpawner.MapsReady)
            {
                if (showDebugLogs) Debug.Log("[PlayerSpawnManager] Waiting for maps to spawn before spawning players...");
                pendingSpawn = true;
                pendingSceneName = sceneName;
                
                // Start a coroutine to timeout if maps take too long
                StartCoroutine(WaitForMapsTimeout());
                return;
            }

            if (showDebugLogs) Debug.Log($"[PlayerSpawnManager] Spawning players immediately (maps ready or not waiting).");
            SpawnAllPlayersIfNeeded();
        }

        /// <summary>
        /// Spawns the instructor spectator prefab if the current user is an instructor.
        /// This is a local (non-networked) object for camera control only.
        /// </summary>
        private void SpawnInstructorSpectatorIfNeeded()
        {
            // Check if user is instructor
            string userType = PlayerPrefs.GetString("Type_Of_User", "");
            if (userType != "instructor")
            {
                if (showDebugLogs) Debug.Log("[PlayerSpawnManager] User is not an instructor. Skipping spectator spawn.");
                return;
            }

            // Check if already spawned
            if (_instructorSpectatorInstance != null)
            {
                if (showDebugLogs) Debug.Log("[PlayerSpawnManager] Instructor spectator already spawned. Skipping.");
                return;
            }

            GameObject spectatorPrefab = instructorSpectatorPrefab;

            // Try loading from Resources if not assigned
            if (spectatorPrefab == null)
            {
                if (showDebugLogs) Debug.Log("[PlayerSpawnManager] Attempting to load InstructorSpectator from Resources...");
                spectatorPrefab = Resources.Load<GameObject>("InstructorSpectator");
            }

            if (spectatorPrefab == null)
            {
                Debug.LogError("[PlayerSpawnManager] Cannot spawn instructor spectator: Prefab not assigned and not found in Resources/InstructorSpectator.prefab");
                return;
            }

            // Instantiate the spectator (local only, not networked)
            _instructorSpectatorInstance = Instantiate(spectatorPrefab, Vector3.zero, Quaternion.identity);
            _instructorSpectatorInstance.name = "InstructorSpectator";

            if (showDebugLogs) 
            {
                Debug.Log("[PlayerSpawnManager] ✓ Spawned Instructor Spectator successfully!");
            }
        }

        private IEnumerator WaitForMapsTimeout()
        {
            float elapsed = 0f;
            while (pendingSpawn && elapsed < maxWaitTime)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (pendingSpawn)
            {
                Debug.LogWarning($"[PlayerSpawnManager] Timed out waiting for maps after {maxWaitTime}s. Spawning players anyway.");
                pendingSpawn = false;
                SpawnAllPlayersIfNeeded();
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            // If a client connects while already in a gameplay scene, spawn them.
            if (!NetworkManager.Singleton.IsServer) return;
            string activeScene = SceneManager.GetActiveScene().name;
            if (!IsGameplayScene(activeScene)) return;
            
            // If we're still waiting for maps, don't spawn yet - they'll be spawned when maps are ready
            if (waitForMaps && !MapSpawner.MapsReady)
            {
                if (showDebugLogs) Debug.Log($"[PlayerSpawnManager] Client {clientId} connected but maps not ready. Will spawn when maps are ready.");
                pendingSpawn = true;
                return;
            }
            
            if (showDebugLogs) Debug.Log($"[PlayerSpawnManager] Client {clientId} connected in gameplay scene '{activeScene}'. Spawning if needed.");
            SpawnPlayerIfNeeded(clientId);
        }

        private void SpawnAllPlayersIfNeeded()
        {
            foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
            {
                SpawnPlayerIfNeeded(kvp.Key);
            }
        }

        /// <summary>
        /// Checks if a specific client is an instructor.
        /// For the host/server, this checks PlayerPrefs.
        /// For other clients, this would need to be communicated via RPC (not implemented here).
        /// </summary>
        private bool IsClientInstructor(ulong clientId)
        {
            // Check cached status first
            if (_clientInstructorStatus.TryGetValue(clientId, out bool isInstructor))
            {
                return isInstructor;
            }

            // Only the host/server can check their own PlayerPrefs
            // For remote clients, we assume they are trainees unless told otherwise
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                string userType = PlayerPrefs.GetString("Type_Of_User", "");
                isInstructor = (userType == "instructor");
                _clientInstructorStatus[clientId] = isInstructor;
                return isInstructor;
            }

            // Default: assume trainee for remote clients
            // NOTE: If you need to support remote instructors, you'd need to implement
            // an RPC system where clients send their user type to the server
            return false;
        }

        private void SpawnPlayerIfNeeded(ulong clientId)
        {
            var nm = NetworkManager.Singleton;
            if (nm == null)
            {
                Debug.LogWarning("[PlayerSpawnManager] NetworkManager null during SpawnPlayerIfNeeded.");
                return;
            }
            if (!nm.ConnectedClients.TryGetValue(clientId, out var clientData))
            {
                if (showDebugLogs) Debug.LogWarning($"[PlayerSpawnManager] ConnectedClients does not contain client {clientId} yet.");
                return;
            }
            if (clientData.PlayerObject != null)
            {
                if (showDebugLogs) Debug.Log($"[PlayerSpawnManager] PlayerObject already exists for client {clientId}. Skipping.");
                return;
            }

            // ===== CRITICAL: Check if this client is an instructor =====
            if (IsClientInstructor(clientId))
            {
                if (showDebugLogs) 
                {
                    Debug.Log($"[PlayerSpawnManager] Client {clientId} is an INSTRUCTOR. Skipping player prefab spawn (spectator only).");
                }
                return; // DON'T spawn a player prefab for instructors
            }

            // Only spawn player prefabs for trainees
            if (showDebugLogs)
            {
                Debug.Log($"[PlayerSpawnManager] Client {clientId} is a TRAINEE. Spawning player prefab...");
            }

            // Determine which prefab to use based on DisasterType
            GameObject selectedPrefab = GetPlayerPrefabForDisasterMode();
            
            if (selectedPrefab == null)
            {
                Debug.LogError("[PlayerSpawnManager] Cannot spawn player, no valid prefab for current disaster mode.");
                return;
            }
            
            Vector3 spawnPos = GetSpawnPosition(clientId);
            Quaternion spawnRot = GetSpawnRotation();
            
            var instance = Instantiate(selectedPrefab, spawnPos, spawnRot);
            var netObj = instance.GetComponent<NetworkObject>();
            if (netObj == null)
            {
                Debug.LogError("[PlayerSpawnManager] Player prefab missing NetworkObject component.");
                Destroy(instance);
                return;
            }
            netObj.SpawnAsPlayerObject(clientId);
            if (showDebugLogs) Debug.Log($"[PlayerSpawnManager] Spawned {selectedPrefab.name} for TRAINEE client {clientId} at {spawnPos}.");
        }

        /// <summary>
        /// Determines which player prefab to use based on the disaster mode from PlayerPrefs
        /// </summary>
        private GameObject GetPlayerPrefabForDisasterMode()
        {
            string disasterType = PlayerPrefs.GetString("DisasterType", "Earthquake");
            currentDisasterType = disasterType; // Cache for spawn position

            GameObject selectedPrefab = null;
            
            switch (disasterType)
            {
                case "Flood":
                    selectedPrefab = boatPlayerPrefab;
                    if (showDebugLogs)
                    {
                        Debug.Log("[PlayerSpawnManager] Flood mode detected - using Boat player prefab");
                    }
                    break;
                    
                case "Earthquake":
                case "TestKen":
                    selectedPrefab = walkingPlayerPrefab;
                    if (showDebugLogs)
                    {
                        Debug.Log($"[PlayerSpawnManager] {disasterType} mode detected - using Walking player prefab");
                    }
                    break;
                    
                default:
                    Debug.LogWarning($"[PlayerSpawnManager] Unknown disaster type: {disasterType}. Defaulting to walking player.");
                    selectedPrefab = walkingPlayerPrefab;
                    break;
            }

            return selectedPrefab;
        }

        private Vector3 GetSpawnPosition(ulong clientId)
        {
            Vector3 basePosition = hostStartPosition;
            
            // Adjust Y position for Flood mode (boats need to spawn at water level)
            if (currentDisasterType == "Flood")
            {
                basePosition.y = floodYOffset;
                if (showDebugLogs)
                {
                    Debug.Log($"[PlayerSpawnManager] Flood mode: Adjusted spawn Y to {floodYOffset}");
                }
            }

            // Host gets basePosition; other clients offset sequentially.
            if (clientId == NetworkManager.Singleton.LocalClientId && NetworkManager.Singleton.IsHost)
            {
                return basePosition;
            }
            
            // Order clients deterministically by clientId for offsets.
            var orderedIds = NetworkManager.Singleton.ConnectedClients.Keys.OrderBy(id => id).ToList();
            int index = orderedIds.IndexOf(clientId);
            
            // First index (host) at basePosition, others offset.
            if (index <= 0) return basePosition;
            
            return basePosition + (clientStartOffset * index);
        }

        /// <summary>
        /// Gets the spawn rotation - boats may need a specific rotation to be stable
        /// </summary>
        private Quaternion GetSpawnRotation()
        {
            // For boats, spawn with no rotation to ensure stability
            // You can adjust this if boats need a specific initial facing direction
            return Quaternion.identity;
        }
    }
}
