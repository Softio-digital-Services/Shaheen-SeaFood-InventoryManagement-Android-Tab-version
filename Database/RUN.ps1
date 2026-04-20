# School Management System - Run Script
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " School Management System - Launch" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

$projectDir = "c:\Users\Khale\Desktop\Personal\C# Projects\School-Management-System-using-CSharp\SchoolMangementSystem"
$outputDir = "$projectDir\SchoolMangementSystem\bin\Debug"
$nugetPackages = "$env:USERPROFILE\.nuget\packages"

# Copy all required DLLs
Write-Host "Copying required dependencies..." -ForegroundColor Yellow

$dlls = @{
    "system.resources.extensions\8.0.0\lib\net462\System.Resources.Extensions.dll" = "System.Resources.Extensions.dll"
    "system.memory\4.5.5\lib\netstandard2.0\System.Memory.dll" = "System.Memory.dll"
    "system.runtime.compilerservices.unsafe\6.0.0\lib\netstandard2.0\System.Runtime.CompilerServices.Unsafe.dll" = "System.Runtime.CompilerServices.Unsafe.dll"
    "system.numerics.vectors\4.5.0\lib\netstandard2.0\System.Numerics.Vectors.dll" = "System.Numerics.Vectors.dll"
    "system.buffers\4.5.1\lib\netstandard2.0\System.Buffers.dll" = "System.Buffers.dll"
}

foreach ($dll in $dlls.Keys) {
    $sourcePath = Join-Path $nugetPackages $dll
    $destPath = Join-Path $outputDir $dlls[$dll]
    
    if (Test-Path $sourcePath) {
        Copy-Item $sourcePath $destPath -Force
        Write-Host "  [OK] Copied $($dlls[$dll])" -ForegroundColor Green
    } else {
        Write-Host "  [ERR] Not found: $dll" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Launching application..." -ForegroundColor Yellow
Write-Host ""
Write-Host "Login Credentials:" -ForegroundColor Cyan
Write-Host "  Username: admin" -ForegroundColor White
Write-Host "  Password: admin" -ForegroundColor White
Write-Host ""

# Launch the application
Start-Process "$outputDir\SchoolMangementSystem.exe" -WorkingDirectory $outputDir

Write-Host "Application launched!" -ForegroundColor Green
