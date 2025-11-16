@echo off
REM AGAP-LIKAS Multiplayer Setup - Windows Batch Launcher

echo ========================================
echo  AGAP-LIKAS Multiplayer Setup
echo ========================================
echo.

REM Check if PowerShell is available
where powershell >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: PowerShell not found!
    echo Please install PowerShell to run this script.
    pause
    exit /b 1
)

REM Get the directory where this batch file is located
set SCRIPT_DIR=%~dp0

REM Run the PowerShell script
echo Running setup wizard...
echo.

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_DIR%Setup-Multiplayer.ps1"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Setup script encountered an error.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Setup complete!
pause
