@echo off
REM Tejimola: The Blossom From Clay — Setup Verification Script
REM This script checks if your Windows PC is ready to run the game

setlocal enabledelayedexpansion
color 0A
cls

echo.
echo ╔═══════════════════════════════════════════════════════════════╗
echo ║  Tejimola: The Blossom From Clay - Setup Verification       ║
echo ║  Windows Environment Check                                   ║
echo ╚═══════════════════════════════════════════════════════════════╝
echo.

set CHECKS_PASSED=0
set CHECKS_TOTAL=0

REM Check 1: Windows Version
echo [Checking System Requirements]
echo.
for /f "tokens=3" %%i in ('ver ^| find "Windows"') do set OS_VERSION=%%i
echo OS Version: %OS_VERSION%
if "%OS_VERSION%"=="" (
    echo ✓ Windows OS detected
) else (
    echo ✓ Windows OS detected: %OS_VERSION%
)
set /a CHECKS_PASSED+=1
set /a CHECKS_TOTAL+=1
echo.

REM Check 2: Available Disk Space (checking C: drive)
echo [Checking Disk Space]
for /f "tokens=3" %%A in ('dir C:\ ^| find "bytes free"') do set FREE_SPACE=%%A
echo Free space on C: drive: %FREE_SPACE%
if "%FREE_SPACE%"=="" (
    echo ⚠ Could not determine free space
    echo   (Ensure at least 2 GB available on your drive)
) else (
    echo ✓ Disk space check complete
    set /a CHECKS_PASSED+=1
)
set /a CHECKS_TOTAL+=1
echo.

REM Check 3: Unity Installation
echo [Checking Unity Installation]
if exist "%ProgramFiles%\Unity\Hub\Editor" (
    echo ✓ Unity Hub found at %ProgramFiles%\Unity\Hub\Editor
    set /a CHECKS_PASSED+=1
) else if exist "%ProgramFiles(x86)%\Unity\Hub\Editor" (
    echo ✓ Unity Hub found at %ProgramFiles(x86)%\Unity\Hub\Editor
    set /a CHECKS_PASSED+=1
) else (
    echo ✗ Unity Hub NOT found
    echo   → Download from https://unity.com/download
)
set /a CHECKS_TOTAL+=1
echo.

REM Check 4: Project Folder
echo [Checking Project Folder]
if exist "TejimolaBlossom\Assets" (
    echo ✓ Project folder structure verified
    if exist "TejimolaBlossom\Assets\_Project\Scripts" (
        echo ✓ Scripts folder found
        set /a CHECKS_PASSED+=1
    )
    if exist "TejimolaBlossom\Assets\_Project\Art" (
        echo ✓ Art assets folder found
        set /a CHECKS_PASSED+=1
    )
    if exist "TejimolaBlossom\Assets\_Project\Audio" (
        echo ✓ Audio folder found
        set /a CHECKS_PASSED+=1
    )
    if exist "TejimolaBlossom\Assets\_Project\Resources\Dialogue" (
        echo ✓ Dialogue files folder found
        set /a CHECKS_PASSED+=1
    )
) else (
    echo ✗ Project folder NOT found in current directory
    echo   → Copy TejimolaBlossom folder to: %cd%
)
set /a CHECKS_TOTAL+=4
echo.

REM Check 5: Visual C++ Redistributable
echo [Checking Visual C++ Redistributable]
reg query "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall" /s | find /i "Visual C++" >nul
if %errorlevel% equ 0 (
    echo ✓ Visual C++ Redistributable found
    set /a CHECKS_PASSED+=1
) else (
    echo ⚠ Visual C++ Redistributable NOT found (optional but recommended)
    echo   → Download: https://support.microsoft.com/en-us/help/2977003
)
set /a CHECKS_TOTAL+=1
echo.

REM Check 6: .NET Framework
echo [Checking .NET Requirements]
reg query "HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4" >nul 2>&1
if %errorlevel% equ 0 (
    echo ✓ .NET Framework detected
    set /a CHECKS_PASSED+=1
) else (
    echo ⚠ .NET Framework status unknown
)
set /a CHECKS_TOTAL+=1
echo.

REM Display Results
echo ╔═══════════════════════════════════════════════════════════════╗
echo ║                    VERIFICATION RESULTS                       ║
echo ╚═══════════════════════════════════════════════════════════════╝
echo.
echo Checks Passed: %CHECKS_PASSED%/%CHECKS_TOTAL%
echo.

if %CHECKS_PASSED% geq 6 (
    echo [SUCCESS] ✓ Your system is ready!
    echo.
    echo Next steps:
    echo 1. Open Unity Hub
    echo 2. Click "Add" → "Add project from disk"
    echo 3. Select TejimolaBlossom folder
    echo 4. Click to open in Unity 2022 LTS
    echo 5. Wait for asset import (5-10 minutes)
    echo 6. Press Play (Ctrl+P) to test
    echo.
) else (
    echo [WARNING] Some checks failed or were incomplete.
    echo.
    echo Missing requirements:
    if not exist "%ProgramFiles%\Unity\Hub\Editor" (
        echo • Unity Hub (Required)
        echo   → Download: https://unity.com/download
    )
    if not exist "TejimolaBlossom" (
        echo • TejimolaBlossom project folder (Required)
        echo   → Copy to: %cd%
    )
    echo.
)

echo ═══════════════════════════════════════════════════════════════
echo.
echo For detailed setup instructions, see: WINDOWS_SETUP_GUIDE.md
echo For quick start, see: QUICK_START_CHECKLIST.md
echo For gameplay demo, see: GAME_DEMO_WALKTHROUGH.txt
echo.
pause
