@echo off
echo ============================================
echo  GenericInventorySystem - Publish Script
echo ============================================
echo.

set PROJECT=c:\Users\Khale\Desktop\Personal\Service Company\GenericInventorySystem
set OUTPUT=%PROJECT%\publish-output

echo [1/3] Building self-contained release...
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

echo.
echo [2/3] Copying required folders...
xcopy /E /I /Y "%PROJECT%\wwwroot"    "%OUTPUT%\wwwroot\"
xcopy /E /I /Y "%PROJECT%\Assets"     "%OUTPUT%\Assets\"
copy  /Y        "%PROJECT%\appsettings.json" "%OUTPUT%\"

echo.
echo [3/3] Done!
echo.
echo Output folder: %OUTPUT%
echo.
echo To deploy to a client PC, copy the entire "%OUTPUT%" folder.
echo The app requires NO installation - just double-click the .exe
echo.
pause
