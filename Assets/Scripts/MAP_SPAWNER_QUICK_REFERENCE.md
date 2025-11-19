# MapSpawner Network - Quick Reference Card

## ?? Implementation Status: ? COMPLETE

---

## ?? **3-Minute Setup**

### **1. Add NetworkObject to 7 Prefabs** (2 min)
```
? Map Prefab 1 + NetworkObject ? Destroy With Scene
? Map Prefab 2 + NetworkObject ? Destroy With Scene
? Map Prefab 3 + NetworkObject ? Destroy With Scene
? Map Prefab 4 + NetworkObject ? Destroy With Scene
? Map Prefab 5 + NetworkObject ? Destroy With Scene
? Map Prefab 6 + NetworkObject ? Destroy With Scene
? SafeZone Prefab + NetworkObject ? Destroy With Scene
```

### **2. Register in NetworkManager** (30 sec)
```
NetworkManager ? Network Prefabs ? Click [+] 7 times
Drag all 7 prefabs into the list
```

### **3. Setup MapSpawner GameObject** (30 sec)
```
MapSpawner GameObject:
??? Add NetworkObject component
??? MapSpawner script should show "NetworkBehaviour"
??? Assign all prefabs in Inspector
```

---

## ?? **How It Works**

| Role | What Happens |
|------|--------------|
| **Host** | Spawns 4 random maps + safe zone ? syncs to all |
| **Client** | Waits for server ? receives spawned maps automatically |
| **Result** | Everyone sees SAME map layout ? |

---

## ? **Code Changes Summary**

| Before | After |
|--------|-------|
| `MonoBehaviour` | `NetworkBehaviour` |
| `void Start()` | `public override void OnNetworkSpawn()` |
| `Instantiate(prefab)` | `Instantiate(prefab)` + `networkObject.Spawn()` |
| No server check | `if (!IsServer) return;` |

---

## ?? **Quick Test**

### **Expected Result:**
1. Host clicks "Host" ? sees 4 maps spawn
2. Client clicks "Join" ? sees SAME 4 maps
3. Both see safe zone in same location

### **Console Check:**
- **Host:** `[MapSpawner] Server - spawning maps`
- **Client:** `[MapSpawner] Client - waiting for server`

---

## ?? **Common Issues - Fast Fixes**

| Error | Fix |
|-------|-----|
| "GameObject X not registered" | Add prefab to NetworkManager list |
| "Maps don't spawn on client" | Check all prefabs have NetworkObject |
| "Different maps on host/client" | Verify only server spawns (IsServer check) |
| "NullReferenceException" | Assign prefabs in MapSpawner Inspector |

---

## ?? **Files Modified/Created**

? **Modified:**
- `Assets\Scripts\MapSpawner.cs` - Now uses NetworkBehaviour

? **Created:**
- `Assets\Scripts\MAP_SPAWNER_NETWORK_SETUP.md` - Full guide
- `Assets\Scripts\MAP_SPAWNER_VISUAL_GUIDE.md` - Visual walkthrough
- `Assets\Scripts\MAP_SPAWNER_QUICK_REFERENCE.md` - This file

---

## ?? **Key Points**

1. **Only server spawns** - Clients receive automatically
2. **NetworkObject required** - On all prefabs + MapSpawner
3. **Register prefabs** - In NetworkManager's prefab list
4. **Destroy With Scene** - Must be checked for maps/safe zone
5. **Same random seed** - Server's shuffle applies to all

---

## ?? **Pro Tips**

- ? Enable Debug Mode during development
- ? Test single-player first, then multiplayer
- ? Check Console logs for errors
- ? Verify prefab registration before building
- ? Keep NetworkManager in all networked scenes

---

## ?? **Related Systems**

Your networked systems:
- ? GameManager - Role-based UI (instructor/trainee)
- ? Player spawning - Host/client differentiation
- ? MapSpawner - Random map generation (NEW!)
- ? Victim spawning - May need network sync
- ? Rescue mechanics - May need network sync

---

## ? **Final Checklist Before Testing**

```
PREFABS:
[?] All 6 map prefabs have NetworkObject
[?] Safe zone prefab has NetworkObject
[?] "Destroy With Scene" checked on all

NETWORK MANAGER:
[?] All 7 prefabs registered in "Network Prefabs" list
[?] NetworkManager exists in game scene

MAPSPAWNER:
[?] MapSpawner has NetworkObject component
[?] Script shows as "NetworkBehaviour"
[?] All prefabs assigned in Inspector
[?] Debug Mode enabled

TESTING:
[?] Build successful (no errors)
[?] Host can spawn maps
[?] Client sees same maps
[?] Console logs look correct
```

---

## ?? **You're Ready!**

Everything is implemented and documented. Follow the setup checklist above and test your networked map spawning system!

**Next Steps:**
1. Complete the setup checklist
2. Test in single-player (host only)
3. Test in multiplayer (host + client)
4. Verify both see same maps
5. Start playing your multiplayer rescue simulation!

---

**Need Help?** Check the detailed guides:
- `MAP_SPAWNER_NETWORK_SETUP.md` - Complete setup instructions
- `MAP_SPAWNER_VISUAL_GUIDE.md` - Visual walkthrough
