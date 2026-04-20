@echo off
echo ==========================================
echo  School Management System - Launch Script
echo ==========================================
echo.

REM Navigate to project directory
cd /d "%~dp0"

REM Copy required DLLs from NuGet packages to output directory
echo Copying required DLLs...

copy "%USERPROFILE%\.nuget\packages\system.resources.extensions\8.0.0\lib\net462\System.Resources.Extensions.dll" "SchoolMangementSystem\bin\Debug\" /Y >nul 2>&1
copy "%USERPROFILE%\.nuget\packages\system.memory\4.5.5\lib\netstandard2.0\System.Memory.dll" "SchoolMangementSystem\bin\Debug\" /Y >nul 2>&1
copy "%USERPROFILE%\.nuget\packages\system.runtime.compilerservices.unsafe\6.0.0\lib\netstandard2.0\System.Runtime.CompilerServices.Unsafe.dll" "SchoolMangementSystem\bin\Debug\" /Y >nul 2>&1

echo DLLs copied successfully!
echo.

REM Build the project
echo Building project...
dotnet build --no-restore -v:minimal
if %ERRORLEVEL% NEQ 0 (
    echo Build FAILED!
    pause
    exit /b 1
)

echo Build SUCCESS!
echo.

REM Launch the application
echo Launching School Management System...
echo.
echo Login Credentials:
echo   Username: admin
echo   Password: admin
echo.

REM Start the application
start "" "SchoolMangementSystem\bin\Debug\SchoolMangementSystem.exe"

echo Application launched!
echo.
echo Check the logs at:  SchoolMangementSystem\bin\Debug\Logs\
echo.
timeout /t 3
