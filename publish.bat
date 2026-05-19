@echo off
echo ============================================
echo  GenericInventorySystem - Publish Script
echo ============================================
echo.

set PROJECT=c:\Users\Khale\Desktop\Personal\Service Company\GenericInventorySystem
set OUTPUT=%PROJECT%\publish-output

echo [1/4] Cleaning previous publish output...
if exist "%OUTPUT%" rd /s /q "%OUTPUT%"
echo.
echo [2/4] Building self-contained release...
dotnet publish "%PROJECT%" ^
  -c Release ^
  -r win-x64 ^
  --self-contained true ^
  -p:PublishSingleFile=true ^
  -p:IncludeNativeLibrariesForSelfExtract=true ^
  -o "%OUTPUT%"

if %ERRORLEVEL% neq 0 (
  echo ERROR: Build failed!
  pause
  exit /b 1
)

echo [3/4] Copying required folders...
xcopy /E /I /Y "%PROJECT%\wwwroot"    "%OUTPUT%\wwwroot\"
xcopy /E /I /Y "%PROJECT%\Assets"     "%OUTPUT%\Assets\"
copy  /Y        "%PROJECT%\appsettings.json" "%OUTPUT%\"

echo.
echo [4/4] Done building self-contained application!
echo.
echo Output folder: %OUTPUT%
echo.

set INNO_SETUP=""
if exist "C:\Program Files\Inno Setup 7\ISCC.exe" (
    set INNO_SETUP="C:\Program Files\Inno Setup 7\ISCC.exe"
) else if exist "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" (
    set INNO_SETUP="C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
)

if not %INNO_SETUP%=="" (
    echo [4/4] Creating Windows Installer using Inno Setup...
    %INNO_SETUP% "%PROJECT%\installer.iss"
    if %ERRORLEVEL% neq 0 (
        echo ERROR: Installer creation failed!
    ) else (
        echo.
        echo SUCCESS: Installer created in "%PROJECT%\InstallerOutput"
    )
) else (
    echo.
    echo --- OPTIONAL: Create an Installer ---
    echo To create a single .exe setup file for your clients:
    echo 1. Download and install Inno Setup from: https://jrsoftware.org/isdl.php
    echo 2. Run this script again, or double-click "installer.iss" and click "Compile"
    echo.
    echo For now, you can just copy the "%OUTPUT%" folder to a client PC.
)

echo.
pause
