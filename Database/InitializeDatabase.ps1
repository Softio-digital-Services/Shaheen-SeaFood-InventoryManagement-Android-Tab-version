# Simple Database Initialization Script
# Run this AFTER starting the app once (this creates the .mdf file)

$dbPath = "C:\Users\Khale\Desktop\Personal\C# Projects\School-Management-System-using-CSharp\SchoolMangementSystem\SchoolMangementSystem\bin\Debug\Data\school.mdf"

# Build the connection string matching the app's configuration 
$connectionString = "Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=$dbPath;Integrated Security=True;Connect Timeout=30"

Write-Host "Connecting to database..." -ForegroundColor Yellow
Write-Host "Path: $dbPath" -ForegroundColor Gray
Write-Host ""

try {
    # Load SQL Server SMO
    Add-Type -AssemblyName "Microsoft.SqlServer.Smo, Version=16.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" -ErrorAction SilentlyContinue
    
    # Use ADO.NET instead (simpler)
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    Write-Host "Connected successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Creating tables..." -ForegroundColor Yellow
    
    # Create users table
    $createUsers = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'users')
BEGIN
    CREATE TABLE users (
        id INT IDENTITY(1,1) PRIMARY KEY,
        username NVARCHAR(50) NOT NULL UNIQUE,
        password NVARCHAR(255) NOT NULL,
        date_created DATETIME DEFAULT GETDATE(),
        date_updated DATETIME NULL
    );
    INSERT INTO users (username, password) VALUES ('admin', 'admin');
END
"@
    
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = $createUsers
    $cmd.ExecuteNonQuery() | Out-Null
    Write-Host "  [OK] Users table" -ForegroundColor Green
    
    # Create students table
    $createStudents = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'students')
BEGIN
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
END
"@
    
    $cmd.CommandText = $createStudents
    $cmd.ExecuteNonQuery() | Out-Null
    Write-Host "  [OK] Students table" -ForegroundColor Green
    
    # Create teachers table
    $createTeachers = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'teachers')
BEGIN
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
END
"@
    
    $cmd.CommandText = $createTeachers
    $cmd.ExecuteNonQuery() | Out-Null
    Write-Host "  [OK] Teachers table" -ForegroundColor Green
    
    $connection.Close()
    
    Write-Host ""
    Write-Host "====================================" -ForegroundColor Cyan
    Write-Host " DATABASE SETUP COMPLETE!" -ForegroundColor Green
    Write-Host "====================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "You can now log in with:" -ForegroundColor Yellow
    Write-Host "  Username: admin" -ForegroundColor White
    Write-Host "  Password: admin" -ForegroundColor White
    Write-Host ""
    
}
catch {
    Write-Host ""
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Make sure you:" -ForegroundColor Yellow
    Write-Host "1. Have run the application at least once (this creates the .mdf file)" -ForegroundColor White
    Write-Host "2. Closed the application before running this script" -ForegroundColor White
    Write-Host "3. Have SQL Server LocalDB installed" -ForegroundColor White
}

Write-Host ""
Read-Host "Press Enter to exit"
