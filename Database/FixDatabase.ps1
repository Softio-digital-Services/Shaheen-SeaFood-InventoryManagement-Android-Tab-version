# Check and Fix Database
$dbPath = "C:\Users\Khale\Desktop\Personal\C# Projects\School-Management-System-using-CSharp\SchoolMangementSystem\SchoolMangementSystem\bin\Debug\Data\school.mdf"
$connectionString = "Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=$dbPath;Integrated Security=True;Connect Timeout=30"

Write-Host "Checking database at: $dbPath" -ForegroundColor Yellow
Write-Host ""

if (!(Test-Path $dbPath)) {
    Write-Host "Database file does NOT exist yet!" -ForegroundColor Red
    Write-Host "The app needs to run once to create it." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Please:" -ForegroundColor Cyan
    Write-Host "1. Start the app from Visual Studio" -ForegroundColor White
    Write-Host "2. Wait for login screen to appear" -ForegroundColor White
    Write-Host "3. Close the app" -ForegroundColor White
    Write-Host "4. Run this script again" -ForegroundColor White
    Read-Host "`nPress Enter to exit"
    exit
}

Write-Host "Database file exists!" -ForegroundColor Green
Write-Host ""

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    Write-Host "Connected to database!" -ForegroundColor Green
    Write-Host ""
    
    # Check if users table exists
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'users'"
    $tableExists = $cmd.ExecuteScalar()
    
    if ($tableExists -eq 0) {
        Write-Host "Creating users table..." -ForegroundColor Yellow
        $cmd.CommandText = @"
CREATE TABLE users (
    id INT IDENTITY(1,1) PRIMARY KEY,
    username NVARCHAR(50) NOT NULL UNIQUE,
    password NVARCHAR(255) NOT NULL,
    date_created DATETIME DEFAULT GETDATE(),
    date_updated DATETIME NULL
);
"@
        $cmd.ExecuteNonQuery() | Out-Null
        Write-Host "  [OK] Users table created" -ForegroundColor Green
    }
    else {
        Write-Host "Users table exists" -ForegroundColor Green
    }
    
    # Check if admin user exists
    $cmd.CommandText = "SELECT COUNT(*) FROM users WHERE username = 'admin'"
    $adminExists = $cmd.ExecuteScalar()
    
    if ($adminExists -eq 0) {
        Write-Host "Creating admin user..." -ForegroundColor Yellow
        $cmd.CommandText = "INSERT INTO users (username, password) VALUES ('admin', 'admin')"
        $cmd.ExecuteNonQuery() | Out-Null
        Write-Host "  [OK] Admin user created!" -ForegroundColor Green
    }
    else {
        # Show existing admin
        Write-Host "Admin user already exists" -ForegroundColor Green
        $cmd.CommandText = "SELECT username, password FROM users WHERE username = 'admin'"
        $reader = $cmd.ExecuteReader()
        if ($reader.Read()) {
            Write-Host "  Username: $($reader['username'])" -ForegroundColor White
            Write-Host "  Password: $($reader['password'])" -ForegroundColor White
        }
        $reader.Close()
    }
    
    # Check students table
    $cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'students'"
    $tableExists = $cmd.ExecuteScalar()
    
    if ($tableExists -eq 0) {
        Write-Host "`nCreating students table..." -ForegroundColor Yellow
        $cmd.CommandText = @"
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
"@
        $cmd.ExecuteNonQuery() | Out-Null
        Write-Host "  [OK] Students table created" -ForegroundColor Green
    }
    
    # Check teachers table
    $cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'teachers'"
    $tableExists = $cmd.ExecuteScalar()
    
    if ($tableExists -eq 0) {
        Write-Host "Creating teachers table..." -ForegroundColor Yellow
        $cmd.CommandText = @"
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
        $cmd.ExecuteNonQuery() | Out-Null
        Write-Host "  [OK] Teachers table created" -ForegroundColor Green
    }
    
    $connection.Close()
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host " DATABASE IS READY!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Login with:" -ForegroundColor Yellow
    Write-Host "  Username: admin" -ForegroundColor White
    Write-Host "  Password: admin" -ForegroundColor White
    Write-Host ""
    Write-Host "Now restart your application and try logging in!" -ForegroundColor Green
    
}
catch {
    Write-Host ""
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Try this:" -ForegroundColor Yellow
    Write-Host "1. Make sure the app is closed" -ForegroundColor White
    Write-Host "2. Delete the Data folder and restart the app" -ForegroundColor White
    Write-Host "3. Then run this script again" -ForegroundColor White
}

Write-Host ""
Read-Host "Press Enter to exit"
