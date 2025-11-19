# MapSpawner Network Setup Guide

## ? Implementation Complete!

Your `MapSpawner.cs` has been updated to work with Unity Netcode for GameObjects. The host will spawn maps and they'll automatically sync to all clients.

---

## ?? **Setup Checklist**

### **Step 1: Add NetworkObject to Map Prefabs**

For **each of your 6 map prefabs**:

1. **Locate the prefab** in your Project window (usually in `Assets/Prefabs/` or similar)
2. **Click on the prefab** to select it
3. **In the Inspector**, click **"Add Component"**
4. Search for **"Network Object"** and add it
5. **Configure NetworkObject settings:**
   - ? **Check "Destroy With Scene"** (important!)
   - ? Leave "Is Player Object" unchecked
   - ? Leave "Sync Position Threshold" at default

**Repeat for all 6 map prefabs**

---

### **Step 2: Add NetworkObject to Safe Zone Prefab**

1. **Select your Safe Zone prefab** in Project window
2. **Add "Network Object" component**
3. **Configure settings:**
   - ? **Check "Destroy With Scene"**
   - ? Leave "Is Player Object" unchecked

---

### **Step 3: Register Prefabs in NetworkManager**

#### **Find Your NetworkManager:**
1. **Open your game scene** (e.g., TestKen, Flood, or Earthquake scene)
2. **Look in Hierarchy** for a GameObject with `NetworkManager` component
   - Usually named "NetworkManager" or "NetworkManagerObject"

#### **Register the Prefabs:**
1. **Select the NetworkManager** GameObject
2. **In Inspector**, find the `NetworkManager` component
3. **Scroll down to "Network Prefabs"** section
4. **Click the "+" button** to add slots
5. **Drag each prefab** from Project window:
   - Drag Map Prefab 1
   - Drag Map Prefab 2
   - Drag Map Prefab 3
   - Drag Map Prefab 4
   - Drag Map Prefab 5
   - Drag Map Prefab 6
   - Drag Safe Zone Prefab

**Result:**
```
NetworkManager
??? Network Prefabs List (7 items)
    ??? [0] Map1Prefab
    ??? [1] Map2Prefab
    ??? [2] Map3Prefab
    ??? [3] Map4Prefab
    ??? [4] Map5Prefab
    ??? [5] Map6Prefab
    ??? [6] SafeZonePrefab
```

---

### **Step 4: Add NetworkObject to MapSpawner GameObject**

1. **In your game scene**, find the `MapSpawner` GameObject
2. **Add "Network Object" component** to it
3. **Configure settings:**
   - ? Leave "Destroy With Scene" unchecked (it should persist)
   - ? Leave "Is Player Object" unchecked

---

### **Step 5: Update MapSpawner Script Reference**

1. **Select MapSpawner GameObject** in Hierarchy
2. **In Inspector**, verify the script has updated to `NetworkBehaviour`
3. **Assign your prefabs** if not already assigned:
   - Map Prefabs array (size 6)
   - Safe Zone Prefab

---

## ?? **How It Works**

### **Network Flow:**

```
Host Clicks "Host" Button
    ?
Scene Loads
    ?
MapSpawner.OnNetworkSpawn() is called
    ?
IsServer check: Only HOST continues
    ?
Host shuffles and selects 4 random maps
    ?
Host spawns 4 maps using networkObject.Spawn()
    ?
Netcode automatically replicates maps to ALL clients
    ?
All players see SAME maps in SAME positions ?
```

### **Client Flow:**

```
Client Clicks "Join" Button
    ?
Scene Loads
    ?
MapSpawner.OnNetworkSpawn() is called
    ?
IsServer check: FALSE, returns immediately
    ?
Client waits for server-spawned objects
    ?
Netcode receives spawned maps from server
    ?
Client sees the same maps as host ?
```

---

## ?? **Testing**

### **Single Player Test:**
1. Click "Host" button
2. Maps should spawn
3. Check Console for: `[MapSpawner] Server - starting map spawn process`
4. Should see 4 different maps in 2x2 grid
5. Safe zone should appear in one of the maps

### **Multiplayer Test:**

**Host Machine:**
1. Click "Host" button
2. Note which maps spawned and where
3. Console should show server spawn messages

**Client Machine:**
1. Click "Join" button
2. Should see **exact same maps** in **exact same positions** as host
3. Console should show: `[MapSpawner] Client - waiting for server to spawn maps`

---

## ?? **Common Issues & Solutions**

### ? **Issue: "GameObject X has not been registered as a spawnable"**
**Solution:**
- The prefab is missing from NetworkManager's prefab list
- Go to NetworkManager ? Network Prefabs ? Add the missing prefab

### ? **Issue: "Maps don't spawn on client"**
**Solutions:**
1. Check if all prefabs have `NetworkObject` component
2. Verify prefabs are registered in NetworkManager
3. Ensure MapSpawner GameObject has `NetworkObject` component
4. Check if `IsServer` check is working (only host should spawn)

### ? **Issue: "Different maps spawn for host and client"**
**Solution:**
- The random shuffle is happening on both host AND client
- Make sure only server spawns: Check the `if (!IsServer) return;` line exists

### ? **Issue: "Cannot find NetworkObject component"**
**Solution:**
- The code automatically adds NetworkObject if missing
- But it's better to add it manually to prefabs beforehand

### ? **Issue: "Safe zone doesn't spawn"**
**Solutions:**
1. Check if safe zone prefab is assigned in Inspector
2. Verify safe zone prefab has `NetworkObject` component
3. Check if ExitPoint exists in map chunk prefabs
4. Look for warning in Console about missing ExitPoint

---

## ?? **Configuration Options**

### **In MapSpawner Inspector:**

| Setting | Description | Default |
|---------|-------------|---------|
| **Map Prefabs** | Array of 6 map prefabs | (empty) |
| **Map Size** | Size of each map chunk | 121 |
| **Safe Zone Prefab** | Prefab for safe zone | (empty) |
| **Debug Mode** | Enable detailed logging | ? true |

---

## ?? **What Changed in the Code**

### **Before (Non-Networked):**
```csharp
public class MapSpawner : MonoBehaviour
{
    void Start()
    {
        // Spawned maps locally only
    }
}
```

### **After (Networked):**
```csharp
public class MapSpawner : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        // Only server spawns
        if (!IsServer) return;
        
        // Maps automatically sync to all clients
        networkObject.Spawn(true);
    }
}
```

---

## ?? **Key Concepts**

### **1. NetworkBehaviour vs MonoBehaviour**
- `NetworkBehaviour` extends `MonoBehaviour`
- Provides network-specific callbacks like `OnNetworkSpawn()`
- Required for networked objects

### **2. Server Authority**
- Only the **server/host** spawns maps
- Clients **receive** the spawned objects automatically
- This ensures **all players see the same map layout**

### **3. NetworkObject Component**
- Required on **all networked prefabs**
- Handles synchronization automatically
- Manages spawn/despawn across network

### **4. Destroy With Scene**
- ? Check this for maps and safe zone
- Objects are cleaned up when scene changes
- Prevents map persistence across scene loads

---

## ?? **Best Practices**

### ? **DO:**
- Always add `NetworkObject` to prefabs **before** using them
- Register all networked prefabs in NetworkManager
- Use `IsServer` check to prevent duplicate spawning
- Enable debug mode during development
- Test with both host and client

### ? **DON'T:**
- Don't spawn network objects from clients
- Don't forget to register prefabs in NetworkManager
- Don't use `Start()` for networked spawning (use `OnNetworkSpawn()`)
- Don't assume instantiated objects are automatically networked

---

## ?? **Verification Checklist**

Before testing in multiplayer:

- [ ] All 6 map prefabs have `NetworkObject` component
- [ ] Safe zone prefab has `NetworkObject` component
- [ ] "Destroy With Scene" is checked on all map/safe zone NetworkObjects
- [ ] All 7 prefabs are registered in NetworkManager's "Network Prefabs" list
- [ ] MapSpawner GameObject has `NetworkObject` component
- [ ] MapSpawner script shows as `NetworkBehaviour` in Inspector
- [ ] Debug mode is enabled for testing
- [ ] NetworkManager exists in the scene

---

## ?? **Expected Console Output**

### **Host Console:**
```
[GameManager] Role Detection - UserType: instructor, IsHost: True
[MapSpawner] Server - starting map spawn process
[MapSpawner] Spawned networked map: Map1 at position (0, 0, 0)
[MapSpawner] Spawned networked map: Map3 at position (0, 0, 121)
[MapSpawner] Spawned networked map: Map5 at position (121, 0, 0)
[MapSpawner] Spawned networked map: Map2 at position (121, 0, 121)
[MapSpawner] Spawned networked safe zone at (121, 0, 121)
[MapSpawner] Map spawning complete!
```

### **Client Console:**
```
[GameManager] Role Detection - UserType: trainee, IsHost: False
[MapSpawner] Client - waiting for server to spawn maps
```

---

## ?? **Next Steps**

1. ? Complete the setup checklist above
2. ?? Test in single-player (host only)
3. ?? Test in multiplayer (host + 1 client)
4. ?? Check console logs match expected output
5. ? Verify both players see same map layout
6. ?? Play test the full simulation

---

## ?? **Still Having Issues?**

If you encounter problems:

1. **Enable Debug Mode:**
   - Select MapSpawner in Inspector
   - Check "Debug Mode" checkbox
   - Check console for detailed logs

2. **Verify NetworkManager Setup:**
   - Select NetworkManager
   - Check "Network Prefabs" section
   - Ensure all 7 prefabs are listed

3. **Check Prefab Components:**
   - Select each map prefab
   - Verify `NetworkObject` component exists
   - Check "Destroy With Scene" is enabled

4. **Test Network Connection:**
   - Verify host can start successfully
   - Check if client can join
   - Look for connection errors in console

---

## ? **Summary**

Your MapSpawner is now fully networked and ready for multiplayer!

**What it does:**
- ? Host spawns 4 random maps from your 6 prefabs
- ? Maps automatically sync to all connected clients
- ? All players see the exact same map layout
- ? Safe zone spawns in the same location for everyone
- ? Maps are cleaned up when scene changes

**Setup required:**
1. Add `NetworkObject` to all prefabs (7 total)
2. Register prefabs in NetworkManager
3. Add `NetworkObject` to MapSpawner GameObject
4. Test!

---

**Ready to test? Follow the checklist above and you're good to go!** ??
