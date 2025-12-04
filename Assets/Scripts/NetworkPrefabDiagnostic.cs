using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

/// <summary>
/// Diagnostic script to check NetworkManager configuration
/// Attach this to NetworkManager GameObject temporarily to diagnose map spawning issues
/// </summary>
public class NetworkPrefabDiagnostic : MonoBehaviour
{
    private void Start()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[Diagnostic] NetworkManager.Singleton is NULL!");
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;

        Debug.Log("=== NETWORK PREFAB DIAGNOSTIC ===");
        Debug.Log($"Current Scene: {currentScene}");
        Debug.Log($"Total Network Prefabs Registered: {NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs.Count}");

        Debug.Log("\n--- Registered Prefabs ---");
        for (int i = 0; i < NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs.Count; i++)
        {
            var prefab = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs[i];
            Debug.Log($"{i}: {prefab.Prefab.name} (NetworkObject: {prefab.Prefab.GetComponent<NetworkObject>() != null})");
        }

        // Only check MapSpawner in gameplay scenes
        string[] gameplayScenes = { "TestKen", "Flood", "Earthquake" };
        bool isGameplayScene = System.Array.IndexOf(gameplayScenes, currentScene) >= 0;

        if (!isGameplayScene)
        {
            Debug.Log($"\n[Diagnostic] Current scene '{currentScene}' is NOT a gameplay scene.");
            Debug.Log("[Diagnostic] MapSpawner check skipped - only runs in: TestKen, Flood, Earthquake");
            Debug.Log("[Diagnostic] To test map spawning:");
            Debug.Log("  1. Go to LobbyMenu scene");
            Debug.Log("  2. Play as Host ? Create Lobby ? Start Game");
            Debug.Log("  3. Check console in gameplay scene for map spawn logs");
            Debug.Log("=== END DIAGNOSTIC ===\n");
            return;
        }

        Debug.Log("\n--- MapSpawner Configuration ---");
        var mapSpawner = FindObjectOfType<MapSpawner>();
        if (mapSpawner == null)
        {
            Debug.LogError($"[Diagnostic] MapSpawner NOT FOUND in '{currentScene}' scene!");
            Debug.LogError("[Diagnostic] >>> FIX: Add MapSpawner GameObject to this scene:");
            Debug.LogError("  1. Create Empty GameObject ? name it 'MapSpawner'");
            Debug.LogError("  2. Add Component ? MapSpawner script");
            Debug.LogError("  3. Add Component ? NetworkObject");
            Debug.LogError("  4. Assign map prefabs in Inspector");
            Debug.Log("=== END DIAGNOSTIC ===\n");
            return;
        }

        // Use reflection to access public fields
        var mapPrefabsField = typeof(MapSpawner).GetField("mapPrefabs", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var safeZoneField = typeof(MapSpawner).GetField("safeZonePrefab", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        if (mapPrefabsField != null)
        {
            GameObject[] mapPrefabs = (GameObject[])mapPrefabsField.GetValue(mapSpawner);
            if (mapPrefabs == null || mapPrefabs.Length == 0)
            {
                Debug.LogError("[Diagnostic] MapSpawner.mapPrefabs is EMPTY or NULL!");
                Debug.LogError("[Diagnostic] >>> FIX: Assign map prefabs in MapSpawner Inspector");
            }
            else
            {
                Debug.Log($"Map Prefabs Assigned: {mapPrefabs.Length}");
                int missingNetworkObject = 0;
                int notRegistered = 0;

                for (int i = 0; i < mapPrefabs.Length; i++)
                {
                    if (mapPrefabs[i] == null)
                    {
                        Debug.LogError($"  Map Prefab {i}: NULL!");
                    }
                    else
                    {
                        bool hasNetObj = mapPrefabs[i].GetComponent<NetworkObject>() != null;
                        bool isRegistered = NetworkManager.Singleton.NetworkConfig.Prefabs.Contains(mapPrefabs[i]);
                        
                        Debug.Log($"  Map Prefab {i}: {mapPrefabs[i].name}");
                        Debug.Log($"    - Has NetworkObject: {hasNetObj}");
                        Debug.Log($"    - Registered in NetworkManager: {isRegistered}");
                        
                        if (!hasNetObj)
                        {
                            missingNetworkObject++;
                            Debug.LogError($"    >>> MISSING NetworkObject! Add it to prefab {mapPrefabs[i].name}");
                        }
                        if (!isRegistered)
                        {
                            notRegistered++;
                            Debug.LogError($"    >>> NOT REGISTERED! Add {mapPrefabs[i].name} to NetworkManager's Network Prefabs List");
                        }
                    }
                }

                if (missingNetworkObject == 0 && notRegistered == 0)
                {
                    Debug.Log("\n? All map prefabs correctly configured!");
                }
                else
                {
                    Debug.LogError($"\n? ISSUES FOUND: {missingNetworkObject} missing NetworkObject, {notRegistered} not registered");
                }
            }
        }

        if (safeZoneField != null)
        {
            GameObject safeZone = (GameObject)safeZoneField.GetValue(mapSpawner);
            if (safeZone == null)
            {
                Debug.LogWarning("[Diagnostic] SafeZone prefab is NULL!");
            }
            else
            {
                bool hasNetObj = safeZone.GetComponent<NetworkObject>() != null;
                bool isRegistered = NetworkManager.Singleton.NetworkConfig.Prefabs.Contains(safeZone);
                
                Debug.Log($"\nSafe Zone Prefab: {safeZone.name}");
                Debug.Log($"  - Has NetworkObject: {hasNetObj}");
                Debug.Log($"  - Registered in NetworkManager: {isRegistered}");
                
                if (!hasNetObj)
                {
                    Debug.LogError($"  >>> MISSING NetworkObject! Add it to {safeZone.name}");
                }
                if (!isRegistered)
                {
                    Debug.LogError($"  >>> NOT REGISTERED! Add {safeZone.name} to NetworkManager's Network Prefabs List");
                }
            }
        }

        Debug.Log("=== END DIAGNOSTIC ===\n");
    }
}
