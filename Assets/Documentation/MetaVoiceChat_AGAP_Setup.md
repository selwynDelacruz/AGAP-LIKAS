# AGAP-LIKAS MetaVoiceChat Setup Guide

Complete guide for setting up MetaVoiceChat voice communication in AGAP-LIKAS.

---

## Quick Overview

MetaVoiceChat provides peer-to-peer voice chat that integrates directly with Unity Netcode for GameObjects. Voice automatically works when players spawn - no additional servers required!

---

## Step 1: Add Scripting Define Symbol

**CRITICAL: This must be done first!**

1. Go to **Edit > Project Settings > Player**
2. Scroll to **Other Settings > Script Compilation**
3. Find **Scripting Define Symbols**
4. Add: `METAVC_NGO`
5. Click **Apply**
6. Wait for Unity to recompile

> ?? Without this symbol, the NGONetProvider component won't compile!

---

## Step 2: Configure Audio Settings

1. Go to **Edit > Project Settings > Audio**
2. Change **DSP Buffer Size** from "Best performance" to **"Best latency"**

This reduces voice chat delay.

---

## Step 3: Setup Player Prefabs

You need to add voice chat to BOTH player prefabs:
- Walking Player Prefab (Earthquake mode)
- Boat Player Prefab (Flood mode)

### For Each Prefab:

#### 3.1 Create Voice Chat GameObject

1. Open the player prefab in Prefab Editor
2. Create a new child GameObject: **Right-click > Create Empty**
3. Name it: `VoiceChat`

#### 3.2 Add Voice Chat Components

Select the `VoiceChat` GameObject and add these components:

| Component | Menu Path |
|-----------|-----------|
| **MetaVc** | Add Component > MetaVoiceChat > MetaVc |
| **VcMicAudioInput** | Add Component > MetaVoiceChat > Input > Mic > VcMicAudioInput |
| **VcAudioSourceOutput** | Add Component > MetaVoiceChat > Output > AudioSource > VcAudioSourceOutput |
| **NGONetProvider** | Add Component > MetaVoiceChat > NetProviders > NGO > NGONetProvider |

#### 3.3 Create Audio Output

1. Under `VoiceChat`, create another child: **Right-click > Create Empty**
2. Name it: `VoiceChatAudioSource`
3. Add an **AudioSource** component

#### 3.4 Configure AudioSource

Set these values on the AudioSource:

| Setting | Value | Why |
|---------|-------|-----|
| **Play On Awake** | ? Unchecked | MetaVoiceChat controls playback |
| **Loop** | ? Unchecked | MetaVoiceChat handles looping |
| **Spatial Blend** | `1` | 3D positional audio (proximity chat) |
| **Doppler Level** | `0` | Must be 0 for voice chat |
| **Max Distance** | `50` | How far voice carries |
| **Volume Rolloff** | Logarithmic | Natural falloff |

> ?? Set Spatial Blend to `0` for global voice chat (everyone hears everyone at full volume)

#### 3.5 Connect References

1. Select `VcMicAudioInput` component
   - Drag `MetaVc` component to the `Meta Vc` field

2. Select `VcAudioSourceOutput` component
   - Drag `MetaVc` component to the `Meta Vc` field
   - Drag `VoiceChatAudioSource` to the `Audio Source` field

3. Select `MetaVc` component
   - Drag `VcMicAudioInput` to the `Audio Input` field
   - Drag `VcAudioSourceOutput` to the `Audio Output` field

#### 3.6 Final Prefab Hierarchy

```
PlayerPrefab (NetworkObject)
??? ... (existing components like CharacterController, Animator, etc.)
??? VoiceChat
    ??? MetaVc
    ??? VcMicAudioInput  
    ??? VcAudioSourceOutput
    ??? NGONetProvider
    ??? VoiceChatAudioSource
         ??? AudioSource (component)
```

---

## Step 4: Add Voice Chat UI (Optional)

### 4.1 Create UI Elements

In your game HUD canvas:

1. Create a **Button** for mute toggle
2. (Optional) Create a **Text** for status display
3. (Optional) Create an **Image** for speaking indicator

### 4.2 Add VoiceChatUI Script

1. Add the `VoiceChatUI` component to your HUD panel
2. Connect references:
   - Mute Button
   - Mute Button Image
   - Mic On/Off Sprites
   - Status Text
   - Speaking Indicator

---

## Step 5: Testing

### In Editor (Single Player)

1. Enable **Is Echo Enabled** on the `MetaVc` component
2. Play the game
3. You should hear your own voice played back

### Multiplayer Testing

1. Build the game
2. Run one instance from Unity Editor
3. Run another from the built executable
4. Both players should hear each other when near

---

## MetaVc Settings Reference

On the `MetaVc` component:

| Setting | Default | Description |
|---------|---------|-------------|
| **Is Echo Enabled** | Off | Hear your own voice (for testing) |
| **Is Sine Override Enabled** | Off | Replace voice with test tone |
| **Complexity** | 10 | Opus quality (0=fast, 10=best) |
| **Frame Size** | 20ms | Audio chunk size |

### Recommended Settings by Use Case

| Scenario | Complexity | Frame Size |
|----------|------------|------------|
| High-end devices, few players | 10 | 20ms |
| Many players (10+) | 5-7 | 20ms |
| Mobile/low-end devices | 3-5 | 40ms |
| Lowest latency | 10 | 10ms |

---

## Controlling Voice Chat from Code

### Mute Yourself (Others Can't Hear You)

```csharp
// Get the local player's MetaVc
var metaVc = NGONetProvider.LocalPlayerInstance?.MetaVc;
if (metaVc != null)
{
    metaVc.isInputMuted.Value = true;  // Mute
    metaVc.isInputMuted.Value = false; // Unmute
}
```

### Deafen Yourself (You Can't Hear Others)

```csharp
var metaVc = NGONetProvider.LocalPlayerInstance?.MetaVc;
if (metaVc != null)
{
    metaVc.isDeafened.Value = true;  // Deafen
    metaVc.isDeafened.Value = false; // Undeafen
}
```

### Check if Speaking

```csharp
var metaVc = NGONetProvider.LocalPlayerInstance?.MetaVc;
if (metaVc != null && metaVc.isSpeaking.Value)
{
    // Player is currently speaking
}
```

### Mute a Specific Remote Player

```csharp
// On the remote player's MetaVc component:
remotePlayerMetaVc.isOutputMuted.Value = true;
```

---

## Troubleshooting

### "NGONetProvider" component not showing up

- Make sure `METAVC_NGO` is in Scripting Define Symbols
- Wait for Unity to finish recompiling
- Restart Unity if needed

### No audio / can't hear other players

1. Check AudioSource is assigned in VcAudioSourceOutput
2. Verify AudioSource volume is not 0
3. Check Max Distance on AudioSource (try increasing to 100)
4. Ensure both players are in the same network session

### Crackling or choppy audio

1. Lower Complexity setting (try 5-7)
2. Increase Frame Size to 40ms
3. Check CPU usage - Opus encoding may be too heavy

### Echo / feedback

- Disable "Is Echo Enabled" on MetaVc
- Make sure players are using headphones, not speakers

### "Player" microphone not detected

1. Check Windows/macOS microphone permissions
2. Ensure a microphone is connected and set as default
3. Check VcMicAudioInput component for errors

---

## Architecture Summary

```
???????????????????????????????????????????????????????????
?                     Player Prefab                        ?
?  ?????????????????????????????????????????????????????  ?
?  ?                   VoiceChat                        ?  ?
?  ?                                                    ?  ?
?  ?  ???????????  ????????????????????                ?  ?
?  ?  ? MetaVc  ???? VcMicAudioInput  ? Microphone     ?  ?
?  ?  ?(Core)   ?  ????????????????????                ?  ?
?  ?  ?         ?                                      ?  ?
?  ?  ?         ?  ????????????????????                ?  ?
?  ?  ?         ????VcAudioSourceOutput???AudioSource  ?  ?
?  ?  ?         ?  ????????????????????    (Speaker)   ?  ?
?  ?  ?         ?                                      ?  ?
?  ?  ?         ?  ????????????????????                ?  ?
?  ?  ?         ????  NGONetProvider  ? Network Sync   ?  ?
?  ?  ???????????  ????????????????????                ?  ?
?  ?????????????????????????????????????????????????????  ?
???????????????????????????????????????????????????????????

Audio Flow:
Microphone ? Opus Encode ? Network RPC ? Opus Decode ? AudioSource
```

---

## Files Created

| File | Purpose |
|------|---------|
| `Assets/Scripts/VoiceChat/VoiceChatUI.cs` | UI controller for mute/unmute |
| `Assets/Documentation/MetaVoiceChat_AGAP_Setup.md` | This guide |

---

## Support

- MetaVoiceChat GitHub: https://github.com/Metater/MetaVoiceChat
- MetaVoiceChat Discord: https://discord.gg/k4ZtGAA2Nt
- Unity Netcode Docs: https://docs-multiplayer.unity3d.com/
