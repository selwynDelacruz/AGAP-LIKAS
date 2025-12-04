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
    /// Ensure Player Prefab in NetworkManager config is NOT set (to avoid auto lobby spawn).
    /// Assign the playerPrefab in inspector (must have NetworkObject component).
    /// </summary>
    public class PlayerSpawnManager : MonoBehaviour
    {
        [Header("Player Prefab (Must have NetworkObject)")] 
        [SerializeField] private GameObject playerPrefab;

        [Header("Gameplay Scenes That Require Spawning")]
        [SerializeField] private string[] gameplaySceneNames = {"Flood","Earthquake","TestKen"};

        [Header("Spawn Settings")]
        [SerializeField] private Vector3 hostStartPosition = new Vector3(0, 0, 0);
        [SerializeField] private Vector3 clientStartOffset = new Vector3(2, 0, 0); // Offset per client

        [SerializeField] private bool showDebugLogs = true;

        private void Awake()
        {
            if (playerPrefab == null)
            {
                Debug.LogWarning("[PlayerSpawnManager] Player prefab not assigned.");
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
        }

        private bool IsGameplayScene(string sceneName)
        {
            return gameplaySceneNames.Contains(sceneName);
        }

        private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (!NetworkManager.Singleton.IsServer) return; // Only server/host spawns
            if (!IsGameplayScene(sceneName)) return; // Only spawn in gameplay scenes
            if (showDebugLogs) Debug.Log($"[PlayerSpawnManager] Scene '{sceneName}' loaded. Spawning missing players.");
            SpawnAllPlayersIfNeeded();
        }

        private void OnClientConnected(ulong clientId)
        {
            // If a client connects while already in a gameplay scene, spawn them.
            if (!NetworkManager.Singleton.IsServer) return;
            string activeScene = SceneManager.GetActiveScene().name;
            if (!IsGameplayScene(activeScene)) return;
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
            if (playerPrefab == null)
            {
                Debug.LogError("[PlayerSpawnManager] Cannot spawn player, prefab not assigned.");
                return;
            }
            Vector3 spawnPos = GetSpawnPosition(clientId);
            var instance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            var netObj = instance.GetComponent<NetworkObject>();
            if (netObj == null)
            {
                Debug.LogError("[PlayerSpawnManager] Player prefab missing NetworkObject component.");
                Destroy(instance);
                return;
            }
            netObj.SpawnAsPlayerObject(clientId);
            if (showDebugLogs) Debug.Log($"[PlayerSpawnManager] Spawned player for client {clientId} at {spawnPos}.");
        }

        private Vector3 GetSpawnPosition(ulong clientId)
        {
            // Host gets hostStartPosition; other clients offset sequentially.
            if (clientId == NetworkManager.Singleton.LocalClientId && NetworkManager.Singleton.IsHost)
            {
                return hostStartPosition;
            }
            // Order clients deterministically by clientId for offsets.
            var orderedIds = NetworkManager.Singleton.ConnectedClients.Keys.OrderBy(id => id).ToList();
            int index = orderedIds.IndexOf(clientId);
            // First index (host) at hostStartPosition, others offset.
            if (index <= 0) return hostStartPosition;
            return hostStartPosition + (clientStartOffset * index);
        }
    }
}
