# MCP Server Integration Guide for AGAP-LIKAS Multiplayer Setup

## Overview

This guide explains how to use the Model Context Protocol (MCP) Server to automate the setup of the multiplayer lobby system in AGAP-LIKAS.

## What is MCP?

MCP (Model Context Protocol) is a protocol that allows AI assistants to interact with external tools and services. In this case, we've created automated setup scripts that can be triggered via MCP.

## Quick Start - Automated Setup

### Option 1: Windows Batch File (Easiest)

1. **Navigate to** `Assets\Scripts\Netcode\`
2. **Double-click** `Setup-Multiplayer.bat`
3. **Follow** the on-screen wizard

### Option 2: PowerShell Direct

1. **Open PowerShell** in the project directory
2. **Run:**
   ```powershell
   cd "Assets\Scripts\Netcode"
   .\Setup-Multiplayer.ps1
   ```

### Option 3: Node.js MCP Server

1. **Install Node.js** (if not already installed)
2. **Navigate to** `Assets\Scripts\Netcode\`
3. **Run:**
   ```bash
   node mcp-server.js
   ```

## What the Automated Setup Does

The MCP server and automation scripts will:

1. **Verify Project Structure**
   - Check for Unity project folders
   - Ensure Scripts/Netcode directory exists

2. **Check Dependencies**
   - Verify Unity Netcode for GameObjects package
   - List required files and their status

3. **Provide Setup Instructions**
   - Scene setup checklist
   - UI hierarchy requirements
   - Testing procedures

4. **Create Helper Scripts**
   - Unity Editor menu item for quick setup
   - Automated GameObject creation (partial)

## Setup Steps Breakdown

### Phase 1: Verification (Automated)
- ? Check Unity project structure
- ? Verify netcode package installation
- ? Validate required script files

### Phase 2: Scene Setup (Manual - Guided)
The wizard provides detailed instructions for:
- Creating NetworkManager GameObject
- Setting up NetworkLobbyManager
- Configuring NetworkConnectionManager
- Adding EnhancedNetworkUI

### Phase 3: UI Creation (Manual - Guided)
Step-by-step UI hierarchy instructions:
- Connection panel
- Lobby info panel
- Host controls
- Status displays

### Phase 4: Testing (Manual - Guided)
- Local testing procedure
- Network testing procedure
- Troubleshooting tips

## MCP Configuration Files

### `mcp-config.json`
Defines available MCP tools and commands:

```json
{
  "tools": {
    "setup_lobby_scene": "Automatically set up lobby scene",
    "verify_netcode_setup": "Verify Netcode installation",
    "create_network_prefab": "Create networked player prefab"
  }
}
```

### `mcp-server.js`
Node.js server that orchestrates the setup process.

### `Setup-Multiplayer.ps1`
PowerShell script with comprehensive setup wizard.

### `Setup-Multiplayer.bat`
Windows batch file for easy launching.

## Using with AI Assistants

If you're using an AI assistant that supports MCP:

1. **Point to the MCP config:**
   ```
   Assets/Scripts/Netcode/mcp-config.json
   ```

2. **Available Commands:**
   - "Setup multiplayer lobby" - Runs full setup wizard
   - "Verify netcode" - Checks installation
   - "Create network prefab" - Guides player prefab creation

## Manual Setup Alternative

If automated setup doesn't work, follow these guides in order:

1. **QUICK_START_CHECKLIST.md** - Step-by-step setup (30 min)
2. **MULTIPLAYER_SETUP_GUIDE.md** - Detailed explanations
3. **UI_LAYOUT_REFERENCE.cs** - UI structure reference
4. **ARCHITECTURE_DIAGRAM.cs** - System architecture

## Troubleshooting

### "PowerShell is not recognized"
- Install PowerShell from: https://aka.ms/powershell
- Or use Option 3 (Node.js) instead

### "Script execution is disabled"
Run in PowerShell as Administrator:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### "Netcode package not found"
1. Open Unity Editor
2. Window ? Package Manager
3. Unity Registry ? Search "Netcode for GameObjects"
4. Click Install

### "Files missing"
Ensure you have all files from the multiplayer implementation:
- NetworkLobbyManager.cs
- EnhancedNetworkUI.cs
- NetworkConnectionManager.cs
- NetworkPlayerManager.cs
- Updated LobbyManager.cs

## Progress Tracking

The setup wizard tracks your progress through:

- ? **Completed**: Green checkmarks
- ?? **Warning**: Yellow warnings
- ? **Failed**: Red errors

Follow the wizard output and complete each step before proceeding.

## Next Steps After Automation

1. **Open Unity Editor**
2. **Open Lobby Scene** (or create one)
3. **Follow** the checklist from the wizard output
4. **Assign UI elements** in Inspector
5. **Test** locally first
6. **Test** on network

## Getting Help

If you encounter issues:

1. **Check the wizard output** for specific errors
2. **Review** QUICK_START_CHECKLIST.md
3. **Read** troubleshooting section in MULTIPLAYER_SETUP_GUIDE.md
4. **Verify** all files are present and unchanged

## Advanced: Creating Custom MCP Tools

You can extend the MCP server by adding custom tools to `mcp-server.js`:

```javascript
async customSetupStep() {
    // Your custom automation logic here
    console.log('   Running custom setup...');
}
```

Then add it to the setup steps:

```javascript
this.setupSteps.push({
    name: 'Custom Setup',
    fn: this.customSetupStep
});
```

## Summary

The MCP automation provides:

- ? **Verification** of project structure
- ? **Guided instructions** for manual steps
- ? **Validation** of setup completion
- ? **Testing procedures**
- ? **Troubleshooting help**

**Simply run** `Setup-Multiplayer.bat` and follow the wizard!

---

**Estimated Time:**
- Automated checks: 1 minute
- Manual scene setup: 15-20 minutes
- UI setup: 10-15 minutes
- Testing: 5-10 minutes
- **Total: ~30-45 minutes**

**Remember:** The automation guides you through the process but some steps require manual work in Unity Editor due to the nature of scene setup and UI design.
