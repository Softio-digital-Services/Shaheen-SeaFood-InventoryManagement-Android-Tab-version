# Test Both Applications

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Testing Car Parts Inventory System" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: License Generator
Write-Host "[1/2] Testing License Generator..." -ForegroundColor Yellow
$licenseGenPath = ".\LicenseGeneratorApp\bin\Debug\LicenseKeyGenerator.exe"

if (Test-Path $licenseGenPath) {
    Write-Host "✓ License Generator found!" -ForegroundColor Green
    Write-Host "  Location: $licenseGenPath" -ForegroundColor Gray
    Write-Host "  Launching License Generator..." -ForegroundColor Gray
    Start-Process $licenseGenPath
    Start-Sleep -Seconds 2
} else {
    Write-Host "✗ License Generator NOT found!" -ForegroundColor Red
    Write-Host "  Expected: $licenseGenPath" -ForegroundColor Gray
}

Write-Host ""

# Test 2: Inventory Application
Write-Host "[2/2] Testing Inventory Application..." -ForegroundColor Yellow
$inventoryPath = ".\InventoryApp\bin\Debug\CarPartsInventorySystem.exe"

if (Test-Path $inventoryPath) {
    Write-Host "✓ Inventory Application found!" -ForegroundColor Green
    Write-Host "  Location: $inventoryPath" -ForegroundColor Gray
    Write-Host "  Launching Inventory Application..." -ForegroundColor Gray
    Start-Process $inventoryPath
    Start-Sleep -Seconds 2
} else {
    Write-Host "✗ Inventory Application NOT found!" -ForegroundColor Red
    Write-Host "  Expected: $inventoryPath" -ForegroundColor Gray
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Test Complete!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "NEXT STEPS:" -ForegroundColor Yellow
Write-Host "1. Use License Generator to create a license key" -ForegroundColor White
Write-Host "2. Copy the generated key" -ForegroundColor White
Write-Host "3. Enter it in the Inventory Application when prompted" -ForegroundColor White
Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
