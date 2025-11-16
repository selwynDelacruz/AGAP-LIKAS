#!/usr/bin/env node

/**
 * AGAP-LIKAS Multiplayer Setup - MCP Server
 * Automates the setup of Unity Netcode for GameObjects multiplayer lobby
 */

const fs = require('fs');
const path = require('path');

const UNITY_PROJECT_PATH = process.env.UNITY_PROJECT_PATH || process.cwd();

class MultiplayerSetupServer {
    constructor() {
        this.setupSteps = [];
        this.completedSteps = [];
    }

    /**
     * Main setup orchestrator
     */
    async runFullSetup() {
        console.log('?? AGAP-LIKAS Multiplayer Setup Starting...\n');
        
        this.setupSteps = [
            { name: 'Verify Unity Project', fn: this.verifyUnityProject },
            { name: 'Check Netcode Package', fn: this.checkNetcodePackage },
            { name: 'Setup Lobby Scene Structure', fn: this.setupLobbySceneStructure },
            { name: 'Create Network GameObjects', fn: this.createNetworkGameObjects },
            { name: 'Configure UI Elements', fn: this.configureUIElements },
            { name: 'Validate Setup', fn: this.validateSetup }
        ];

        for (const step of this.setupSteps) {
            await this.executeStep(step);
        }

        console.log('\n? Setup Complete!');
        this.printSummary();
    }

    async executeStep(step) {
        console.log(`\n?? ${step.name}...`);
        try {
            await step.fn.call(this);
            this.completedSteps.push(step.name);
            console.log(`   ? ${step.name} completed`);
        } catch (error) {
            console.error(`   ? ${step.name} failed:`, error.message);
            throw error;
        }
    }

    /**
     * Step 1: Verify Unity Project Structure
     */
    async verifyUnityProject() {
        const assetsPath = path.join(UNITY_PROJECT_PATH, 'Assets');
        const scriptsPath = path.join(assetsPath, 'Scripts');
        const netcodePath = path.join(scriptsPath, 'Netcode');

        if (!fs.existsSync(assetsPath)) {
            throw new Error('Assets folder not found. Are you in a Unity project?');
        }

        // Ensure Netcode folder exists
        if (!fs.existsSync(netcodePath)) {
            fs.mkdirSync(netcodePath, { recursive: true });
            console.log('   Created Scripts/Netcode directory');
        }

        console.log('   Unity project structure verified');
    }

    /**
     * Step 2: Check for Netcode Package
     */
    async checkNetcodePackage() {
        const manifestPath = path.join(UNITY_PROJECT_PATH, 'Packages', 'manifest.json');
        
        if (fs.existsSync(manifestPath)) {
            const manifest = JSON.parse(fs.readFileSync(manifestPath, 'utf8'));
            const hasNetcode = manifest.dependencies && 
                               manifest.dependencies['com.unity.netcode.gameobjects'];
            
            if (hasNetcode) {
                console.log(`   Netcode package found: ${manifest.dependencies['com.unity.netcode.gameobjects']}`);
            } else {
                console.log('   ??  Netcode package not found in manifest.json');
                console.log('   Please install via Package Manager:');
                console.log('   Window ? Package Manager ? Unity Registry ? "Netcode for GameObjects"');
            }
        }
    }

    /**
     * Step 3: Setup Lobby Scene Structure
     */
    async setupLobbySceneStructure() {
        const sceneInstructions = {
            name: 'Lobby Scene Setup',
            gameObjects: [
                {
                    name: 'NetworkManager',
                    components: ['NetworkManager', 'UnityTransport'],
                    instructions: [
                        'Add NetworkManager component',
                        'Add UnityTransport component',
                        'Enable Scene Management in NetworkManager',
                        'Set Transport to UnityTransport'
                    ]
                },
                {
                    name: 'NetworkLobbyManager',
                    components: ['NetworkObject', 'NetworkLobbyManager'],
                    instructions: [
                        'Add NetworkObject component',
                        'Add NetworkLobbyManager script',
                        'Set Max Players to 4'
                    ]
                },
                {
                    name: 'NetworkConnectionManager',
                    components: ['NetworkConnectionManager'],
                    instructions: [
                        'Add NetworkConnectionManager script',
                        'Set Lobby Scene Name to "Lobby"',
                        'Set Main Menu Scene Name to "MainMenu"'
                    ]
                },
                {
                    name: 'EnhancedNetworkUI',
                    components: ['EnhancedNetworkUI'],
                    instructions: [
                        'Add EnhancedNetworkUI script',
                        'Assign UI elements in Inspector (see UI_LAYOUT_REFERENCE.cs)'
                    ]
                }
            ]
        };

        console.log('   Scene structure requirements:');
        sceneInstructions.gameObjects.forEach(go => {
            console.log(`\n   GameObject: ${go.name}`);
            go.instructions.forEach(inst => {
                console.log(`     - ${inst}`);
            });
        });

        // Create scene setup script
        this.createUnityEditorScript();
    }

    /**
     * Step 4: Create Network GameObjects Instructions
     */
    async createNetworkGameObjects() {
        console.log('   Network GameObject requirements documented');
        console.log('   See: MULTIPLAYER_SETUP_GUIDE.md for detailed instructions');
    }

    /**
     * Step 5: Configure UI Elements
     */
    async configureUIElements() {
        const uiElements = [
            'Connection Panel (Host, Client, Disconnect buttons)',
            'Lobby Info Panel (Task count, Duration, Disaster type, Player count)',
            'Host Controls Panel (Start Game button)',
            'Connection Status Text'
        ];

        console.log('   Required UI elements:');
        uiElements.forEach(el => console.log(`     - ${el}`));
        console.log('   See: UI_LAYOUT_REFERENCE.cs for detailed layout');
    }

    /**
     * Step 6: Validate Setup
     */
    async validateSetup() {
        const requiredFiles = [
            'Assets/Scripts/Netcode/NetworkLobbyManager.cs',
            'Assets/Scripts/Netcode/EnhancedNetworkUI.cs',
            'Assets/Scripts/Netcode/NetworkConnectionManager.cs',
            'Assets/Scripts/Netcode/NetworkPlayerManager.cs',
            'Assets/Scripts/LobbyManager.cs'
        ];

        console.log('   Validating required files:');
        requiredFiles.forEach(file => {
            const fullPath = path.join(UNITY_PROJECT_PATH, file);
            const exists = fs.existsSync(fullPath);
            console.log(`     ${exists ? '?' : '?'} ${file}`);
        });
    }

    /**
     * Create Unity Editor Script for automated scene setup
     */
    createUnityEditorScript() {
        const editorScript = `using UnityEngine;
using UnityEditor;
using Unity.Netcode;

namespace Sketchfab.Setup
{
    public class MultiplayerLobbySetup : EditorWindow
    {
        [MenuItem("AGAP-LIKAS/Setup Multiplayer Lobby")]
        public static void SetupLobby()
        {
            // Create NetworkManager
            GameObject networkManager = new GameObject("NetworkManager");
            networkManager.AddComponent<NetworkManager>();
            networkManager.AddComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            
            // Create NetworkLobbyManager
            GameObject lobbyManager = new GameObject("NetworkLobbyManager");
            lobbyManager.AddComponent<NetworkObject>();
            // Add NetworkLobbyManager script - requires manual script attachment
            
            // Create NetworkConnectionManager
            GameObject connectionManager = new GameObject("NetworkConnectionManager");
            // Add NetworkConnectionManager script - requires manual script attachment
            
            // Create EnhancedNetworkUI
            GameObject networkUI = new GameObject("EnhancedNetworkUI");
            // Add EnhancedNetworkUI script - requires manual script attachment
            
            Debug.Log("[MultiplayerSetup] Created base network GameObjects");
            Debug.Log("Please add the required scripts and configure them manually");
            EditorUtility.DisplayDialog("Setup Started", 
                "Base network GameObjects created.\\n" +
                "Please add scripts and configure in Inspector.\\n" +
                "See QUICK_START_CHECKLIST.md for details.", 
                "OK");
        }
    }
}`;

        const editorPath = path.join(UNITY_PROJECT_PATH, 'Assets', 'Scripts', 'Netcode', 'Editor');
        if (!fs.existsSync(editorPath)) {
            fs.mkdirSync(editorPath, { recursive: true });
        }

        const scriptPath = path.join(editorPath, 'MultiplayerLobbySetup.cs');
        fs.writeFileSync(scriptPath, editorScript);
        console.log(`   Created Editor script: ${scriptPath}`);
        console.log('   Access via: AGAP-LIKAS ? Setup Multiplayer Lobby');
    }

    /**
     * Print setup summary
     */
    printSummary() {
        console.log('\n' + '='.repeat(60));
        console.log('?? SETUP SUMMARY');
        console.log('='.repeat(60));
        console.log(`\n? Completed Steps: ${this.completedSteps.length}/${this.setupSteps.length}`);
        
        console.log('\n?? Next Steps:');
        console.log('1. Open Unity Editor');
        console.log('2. Go to AGAP-LIKAS ? Setup Multiplayer Lobby');
        console.log('3. Follow QUICK_START_CHECKLIST.md');
        console.log('4. Assign UI elements in Inspector');
        console.log('5. Test local multiplayer');
        
        console.log('\n?? Documentation:');
        console.log('- Quick Start: Assets/Scripts/Netcode/QUICK_START_CHECKLIST.md');
        console.log('- Full Guide: Assets/Scripts/Netcode/MULTIPLAYER_SETUP_GUIDE.md');
        console.log('- Architecture: Assets/Scripts/Netcode/ARCHITECTURE_DIAGRAM.cs');
        
        console.log('\n' + '='.repeat(60));
    }
}

// Run setup if called directly
if (require.main === module) {
    const server = new MultiplayerSetupServer();
    server.runFullSetup().catch(error => {
        console.error('\n? Setup failed:', error.message);
        process.exit(1);
    });
}

module.exports = MultiplayerSetupServer;
