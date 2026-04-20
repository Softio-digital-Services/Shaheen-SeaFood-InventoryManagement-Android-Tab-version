# Create Database Manually
Write-Host "Creating database manually..." -ForegroundColor Yellow
Write-Host ""

$dbPath = "C:\Users\Khale\Desktop\Personal\C# Projects\School-Management-System-using-CSharp\SchoolMangementSystem\SchoolMangementSystem\bin\Debug\Data\school.mdf"
$logPath = "C:\Users\Khale\Desktop\Personal\C# Projects\School-Management-System-using-CSharp\SchoolMangementSystem\SchoolMangementSystem\bin\Debug\Data\school_log.ldf"

# Make sure Data directory exists
$dataDir = Split-Path $dbPath
if (!(Test-Path $dataDir)) {
    New-Item -ItemType Directory -Path $dataDir -Force | Out-Null
    Write-Host "Created Data directory" -ForegroundColor Green
}

# Drop database if it exists
try {
    sqlcmd -S "(LocalDB)\MSSQLLocalDB" -Q "IF DB_ID('TempSchoolDB') IS NOT NULL ALTER DATABASE TempSchoolDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE; IF DB_ID('TempSchoolDB') IS NOT NULL DROP DATABASE TempSchoolDB;" 2>$null
}
catch {}

Write-Host "Creating database file..." -ForegroundColor Yellow

# Create the database
$createDbScript = @"
CREATE DATABASE TempSchoolDB
ON PRIMARY (
    NAME = school,
    FILENAME = '$dbPath',
    SIZE = 10MB,
    MAXSIZE = 100MB,
    FILEGROWTH = 5MB
)
LOG ON (
    NAME = school_log,
    FILENAME = '$logPath',
    SIZE = 5MB,
    MAXSIZE = 25MB,
    FILEGROWTH = 5MB
);
"@

$createDbScript | sqlcmd -S "(LocalDB)\MSSQLLocalDB" 2>&1 | Out-Null

if ($LASTEXITCODE -eq 0) {
    Write-Host "  [OK] Database file created!" -ForegroundColor Green
}
else {
    Write-Host "  [ERR] Failed to create database" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit
}

# Now create tables
Write-Host "Creating tables..." -ForegroundColor Yellow

$createTablesScript = @"
USE TempSchoolDB;

CREATE TABLE users (
    id INT IDENTITY(1,1) PRIMARY KEY,
    username NVARCHAR(50) NOT NULL UNIQUE,
    password NVARCHAR(255) NOT NULL,
    date_created DATETIME DEFAULT GETDATE(),
    date_updated DATETIME NULL
);

INSERT INTO users (username, password) VALUES ('admin', 'admin');

CREATE TABLE students (
    id INT IDENTITY(1,1) PRIMARY KEY,
    student_id NVARCHAR(50) NOT NULL UNIQUE,
    student_name NVARCHAR(100) NOT NULL,
    student_gender NVARCHAR(10) NOT NULL,
    student_address NVARCHAR(255) NOT NULL,
    student_grade NVARCHAR(20) NOT NULL,
    student_section NVARCHAR(20) NOT NULL,
    student_image NVARCHAR(500) NULL,
    student_status NVARCHAR(20) NOT NULL,
    date_insert DATETIME DEFAULT GETDATE(),
    date_update DATETIME NULL,
    date_delete DATETIME NULL
);

CREATE TABLE teachers (
    id INT IDENTITY(1,1) PRIMARY KEY,
    teacher_id NVARCHAR(50) NOT NULL UNIQUE,
    teacher_name NVARCHAR(100) NOT NULL,
    teacher_gender NVARCHAR(10) NOT NULL,
    teacher_address NVARCHAR(255) NOT NULL,
    teacher_image NVARCHAR(500) NULL,
    teacher_status NVARCHAR(20) NOT NULL,
    date_insert DATETIME DEFAULT GETDATE(),
    date_update DATETIME NULL,
    date_delete DATETIME NULL
);
"@

$createTablesScript | sqlcmd -S "(LocalDB)\MSSQLLocalDB" 2>&1 | Out-Null

if ($LASTEXITCODE -eq 0) {
    Write-Host "  [OK] Tables created!" -ForegroundColor Green
    Write-Host "  [OK] Admin user created!" -ForegroundColor Green
}
else {
    Write-Host "  [ERR] Failed to create tables" -ForegroundColor Red
}

# Detach the database so the app can attach it
Write-Host "`nDetaching database..." -ForegroundColor Yellow
sqlcmd -S "(LocalDB)\MSSQLLocalDB" -Q "ALTER DATABASE TempSchoolDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE; EXEC sp_detach_db 'TempSchoolDB';" 2>&1 | Out-Null

if (Test-Path $dbPath) {
    Write-Host "  [OK] Database ready at: $dbPath" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host " DATABASE CREATED SUCCESSFULLY!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "📁 Location: bin\Debug\Data\school.mdf" -ForegroundColor White
    Write-Host ""
    Write-Host "🔑 Login credentials:" -ForegroundColor Yellow
    Write-Host "   Username: admin" -ForegroundColor White
    Write-Host "   Password: admin" -ForegroundColor White
    Write-Host ""
    Write-Host "✅ Now run the app from Visual Studio (F5)" -ForegroundColor Green
    Write-Host "   and you should be able to login!" -ForegroundColor Green
    Write-Host ""
}
else {
    Write-Host ""
    Write-Host "Something went wrong - database file not found" -ForegroundColor Red
}

Read-Host "`nPress Enter to exit"
