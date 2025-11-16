# AGAP-LIKAS Multiplayer Setup Automation Script
# PowerShell script for automated setup

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " AGAP-LIKAS Multiplayer Setup Wizard" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$ProjectPath = "C:\Users\Non Admin\Documents\GitHub\AGAP-LIKAS"
$AssetsPath = Join-Path $ProjectPath "Assets"
$NetcodePath = Join-Path $AssetsPath "Scripts\Netcode"

Write-Host "?? Project Path: $ProjectPath" -ForegroundColor Yellow
Write-Host ""

# Step 1: Verify Unity Project
Write-Host "Step 1: Verifying Unity Project Structure..." -ForegroundColor Green
if (Test-Path $AssetsPath) {
    Write-Host "  ? Assets folder found" -ForegroundColor Green
} else {
    Write-Host "  ? Assets folder not found!" -ForegroundColor Red
    exit 1
}

if (!(Test-Path $NetcodePath)) {
    New-Item -ItemType Directory -Path $NetcodePath -Force | Out-Null
    Write-Host "  ? Created Scripts\Netcode directory" -ForegroundColor Green
} else {
    Write-Host "  ? Scripts\Netcode directory exists" -ForegroundColor Green
}

# Step 2: Check for Required Files
Write-Host ""
Write-Host "Step 2: Checking Required Files..." -ForegroundColor Green

$RequiredFiles = @(
    "Assets\Scripts\Netcode\NetworkLobbyManager.cs",
    "Assets\Scripts\Netcode\EnhancedNetworkUI.cs",
    "Assets\Scripts\Netcode\NetworkConnectionManager.cs",
    "Assets\Scripts\Netcode\NetworkPlayerManager.cs",
    "Assets\Scripts\LobbyManager.cs",
    "Assets\Scripts\Netcode\QUICK_START_CHECKLIST.md",
    "Assets\Scripts\Netcode\MULTIPLAYER_SETUP_GUIDE.md"
)

$AllFilesPresent = $true
foreach ($file in $RequiredFiles) {
    $fullPath = Join-Path $ProjectPath $file
    if (Test-Path $fullPath) {
        Write-Host "  ? $file" -ForegroundColor Green
    } else {
        Write-Host "  ? $file (MISSING)" -ForegroundColor Red
        $AllFilesPresent = $false
    }
}

if (!$AllFilesPresent) {
    Write-Host ""
    Write-Host "??  Some required files are missing!" -ForegroundColor Yellow
    Write-Host "Please ensure all multiplayer scripts are in place." -ForegroundColor Yellow
}

# Step 3: Check Netcode Package
Write-Host ""
Write-Host "Step 3: Checking for Unity Netcode Package..." -ForegroundColor Green
$ManifestPath = Join-Path $ProjectPath "Packages\manifest.json"

if (Test-Path $ManifestPath) {
    $manifest = Get-Content $ManifestPath -Raw | ConvertFrom-Json
    if ($manifest.dependencies.PSObject.Properties.Name -contains "com.unity.netcode.gameobjects") {
        $version = $manifest.dependencies.'com.unity.netcode.gameobjects'
        Write-Host "  ? Netcode package found: $version" -ForegroundColor Green
    } else {
        Write-Host "  ? Netcode package NOT found" -ForegroundColor Red
        Write-Host ""
        Write-Host "  ?? To install Netcode for GameObjects:" -ForegroundColor Yellow
        Write-Host "     1. Open Unity Editor" -ForegroundColor White
        Write-Host "     2. Window ? Package Manager" -ForegroundColor White
        Write-Host "     3. Unity Registry ? Search 'Netcode for GameObjects'" -ForegroundColor White
        Write-Host "     4. Click Install" -ForegroundColor White
    }
} else {
    Write-Host "  ??  manifest.json not found" -ForegroundColor Yellow
}

# Step 4: Scene Setup Instructions
Write-Host ""
Write-Host "Step 4: Scene Setup Required..." -ForegroundColor Green
Write-Host ""
Write-Host "?? LOBBY SCENE SETUP CHECKLIST:" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""

$SceneSetup = @"
1??  NetworkManager GameObject:
   - Create empty GameObject named 'NetworkManager'
   - Add Component: NetworkManager
   - Add Component: UnityTransport
   - Enable 'Scene Management' in NetworkManager
   - Assign UnityTransport to Transport field

2??  NetworkLobbyManager GameObject:
   - Create empty GameObject named 'NetworkLobbyManager'
   - Add Component: NetworkObject
   - Add Component: NetworkLobbyManager
   - Set Max Players to 4

3??  NetworkConnectionManager GameObject:
   - Create empty GameObject named 'NetworkConnectionManager'
   - Add Component: NetworkConnectionManager
   - Set Lobby Scene Name to 'Lobby'
   - Set Main Menu Scene Name to 'MainMenu'

4??  EnhancedNetworkUI GameObject:
   - Create empty GameObject named 'EnhancedNetworkUI'
   - Add Component: EnhancedNetworkUI
   - Assign all UI elements (see UI_LAYOUT_REFERENCE.cs)

5??  Update existing LobbyManager:
   - Find LobbyManager GameObject
   - Check 'Is Multiplayer' checkbox in Inspector
"@

Write-Host $SceneSetup -ForegroundColor White

# Step 5: UI Setup
Write-Host ""
Write-Host "Step 5: UI Elements Required..." -ForegroundColor Green
Write-Host ""
Write-Host "?? UI SETUP CHECKLIST:" -ForegroundColor Cyan
Write-Host "======================" -ForegroundColor Cyan
Write-Host ""

$UISetup = @"
Canvas ? Create the following UI hierarchy:

?? ConnectionPanel (initially visible)
   ?? Host Button
   ?? Client Button
   ?? IP Address InputField

?? LobbyInfoPanel (initially hidden)
   ?? Task Count Text (TMP)
   ?? Duration Text (TMP)
   ?? Disaster Type Text (TMP)
   ?? Player Count Text (TMP)
   ?? Connection Status Text (TMP)

?? HostControlsPanel (initially hidden)
   ?? Start Game Button

?? DisconnectButton (initially hidden)

See: Assets\Scripts\Netcode\UI_LAYOUT_REFERENCE.cs for detailed layout
"@

Write-Host $UISetup -ForegroundColor White

# Step 6: Testing Instructions
Write-Host ""
Write-Host "Step 6: Testing Setup..." -ForegroundColor Green
Write-Host ""
Write-Host "?? TESTING CHECKLIST:" -ForegroundColor Cyan
Write-Host "=====================" -ForegroundColor Cyan
Write-Host ""

$TestSetup = @"
LOCAL TEST (Same Computer):
1. File ? Build Settings ? Build
2. Run the build (acts as Host)
3. Press Play in Unity Editor (acts as Client)
4. In build: Click 'Host Game'
5. In editor: Click 'Join Game'
6. Both should see lobby info
7. Host can change settings
8. Client sees updates
9. Host clicks 'Start Game'
10. Both load into game scene

NETWORK TEST (Different Computers):
Host Computer:
1. Find IP address (ipconfig in cmd)
2. Open port 7777 in firewall
3. Run build
4. Click 'Host Game'

Client Computer:
1. Run build
2. Enter host IP in input field
3. Click 'Join Game'
4. Should connect and see lobby info
"@

Write-Host $TestSetup -ForegroundColor White

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " ?? SETUP SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "? Files Status: " -NoNewline
if ($AllFilesPresent) {
    Write-Host "All Present" -ForegroundColor Green
} else {
    Write-Host "Some Missing" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "?? Next Steps:" -ForegroundColor Yellow
Write-Host "1. Follow the checklist above" -ForegroundColor White
Write-Host "2. Open: Assets\Scripts\Netcode\QUICK_START_CHECKLIST.md" -ForegroundColor White
Write-Host "3. Test locally first" -ForegroundColor White
Write-Host "4. Then test on network" -ForegroundColor White

Write-Host ""
Write-Host "?? Documentation Files:" -ForegroundColor Yellow
Write-Host "  - QUICK_START_CHECKLIST.md (Start here!)" -ForegroundColor White
Write-Host "  - MULTIPLAYER_SETUP_GUIDE.md (Detailed guide)" -ForegroundColor White
Write-Host "  - ARCHITECTURE_DIAGRAM.cs (How it works)" -ForegroundColor White
Write-Host "  - UI_LAYOUT_REFERENCE.cs (UI structure)" -ForegroundColor White
Write-Host "  - README.md (Complete index)" -ForegroundColor White

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Setup wizard complete! Follow the checklists above." -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Offer to open documentation
$response = Read-Host "Would you like to open the Quick Start Checklist now? (Y/N)"
if ($response -eq 'Y' -or $response -eq 'y') {
    $checklistPath = Join-Path $NetcodePath "QUICK_START_CHECKLIST.md"
    if (Test-Path $checklistPath) {
        Start-Process $checklistPath
        Write-Host "Opening Quick Start Checklist..." -ForegroundColor Green
    } else {
        Write-Host "Checklist file not found at: $checklistPath" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
