# Car Parts Inventory - Database Setup Script
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " Car Parts Inventory - Database Setup" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

$projectDir = Get-Location
$dbPath = Join-Path $projectDir "SchoolMangementSystem\bin\Debug\Data\carparts.mdf"
$logPath = Join-Path $projectDir "SchoolMangementSystem\bin\Debug\Data\carparts_log.ldf"

Write-Host "Creating database at: $dbPath" -ForegroundColor Yellow
Write-Host ""

# Create Data directory
$dataDir = Split-Path $dbPath
if (!(Test-Path $dataDir)) {
    New-Item -ItemType Directory -Path $dataDir -Force | Out-Null
    Write-Host "Created Data directory" -ForegroundColor Green
}

# Drop existing database
try {
    sqlcmd -S "(LocalDB)\MSSQLLocalDB" -Q "IF DB_ID('CarPartsTemp') IS NOT NULL DROP DATABASE CarPartsTemp;" 2>$null
}
catch {}

Write-Host "Creating database file..." -ForegroundColor Yellow

# Create the database
$createDbSql = @"
CREATE DATABASE CarPartsTemp
ON PRIMARY (
    NAME = carparts,
    FILENAME = '$dbPath',
    SIZE = 10MB,
    MAXSIZE = 100MB,
    FILEGROWTH = 5MB
)
LOG ON (
    NAME = carparts_log,
    FILENAME = '$logPath',
    SIZE = 5MB,
    MAXSIZE = 25MB,
    FILEGROWTH = 5MB
);
"@

$createDbSql | sqlcmd -S "(LocalDB)\MSSQLLocalDB" 2>&1 | Out-Null

if ($LASTEXITCODE -eq 0) {
    Write-Host "  [OK] Database file created!" -ForegroundColor Green
}
else {
    Write-Host "  [ERR] Failed to create database" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit
}

# Run the schema script
Write-Host "Creating tables and sample data..." -ForegroundColor Yellow
sqlcmd -S "(LocalDB)\MSSQLLocalDB" -d "CarPartsTemp" -i "CreateCarPartsDatabase.sql" 2>&1 | Out-Null

if ($LASTEXITCODE -eq 0) {
    Write-Host "  [OK] Tables and data created!" -ForegroundColor Green
}
else {
    Write-Host "  [WARN] Some issues creating tables" -ForegroundColor Yellow
}

# Detach database
Write-Host "`nFinalizing..." -ForegroundColor Yellow
sqlcmd -S "(LocalDB)\MSSQLLocalDB" -Q "ALTER DATABASE CarPartsTemp SET SINGLE_USER WITH ROLLBACK IMMEDIATE; EXEC sp_detach_db 'CarPartsTemp';" 2>&1 | Out-Null

if (Test-Path $dbPath) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host " DATABASE CREATED SUCCESSFULLY!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Database: $dbPath" -ForegroundColor White
    Write-Host ""
    Write-Host "Login Credentials:" -ForegroundColor Yellow
    Write-Host "  Username: admin" -ForegroundColor White
    Write-Host "  Password: admin" -ForegroundColor White
    Write-Host ""
    Write-Host "Sample Data Included:" -ForegroundColor Yellow
    Write-Host "  - 15 car parts" -ForegroundColor White
    Write-Host "  - 8 categories" -ForegroundColor White
    Write-Host "  - 3 suppliers" -ForegroundColor White
    Write-Host ""
    Write-Host "Ready to run! Press F5 in Visual Studio" -ForegroundColor Green
}
else {
    Write-Host "ERROR: Database file not created" -ForegroundColor Red
}

Read-Host "`nPress Enter to exit"
