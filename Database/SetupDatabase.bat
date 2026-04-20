@echo off
echo ==========================================
echo  Database Setup - School Management System
echo ==========================================
echo.

REM Get the database path
set DB_PATH=%~dp0SchoolMangementSystem\bin\Debug\Data\school.mdf
set LOG_PATH=%~dp0SchoolMangementSystem\bin\Debug\Data\school_log.ldf

echo Database will be created at:
echo %DB_PATH%
echo.

REM Create a temp SQL file
echo Creating database and tables...

(
echo -- Create tables if database exists
echo IF NOT EXISTS ^(SELECT * FROM sys.tables WHERE name = 'users'^)
echo BEGIN
echo     CREATE TABLE users ^(
echo         id INT IDENTITY^(1,1^) PRIMARY KEY,
echo         username NVARCHAR^(50^) NOT NULL UNIQUE,
echo         password NVARCHAR^(255^) NOT NULL,
echo         date_created DATETIME DEFAULT GETDATE^(^),
echo         date_updated DATETIME NULL
echo     ^);
echo     INSERT INTO users ^(username, password^) VALUES ^('admin', 'admin'^);
echo     PRINT 'Admin user created'^;
echo END
echo GO
echo.
echo IF NOT EXISTS ^(SELECT * FROM sys.tables WHERE name = 'students'^)
echo BEGIN
echo     CREATE TABLE students ^(
echo         id INT IDENTITY^(1,1^) PRIMARY KEY,
echo         student_id NVARCHAR^(50^) NOT NULL UNIQUE,
echo         student_name NVARCHAR^(100^) NOT NULL,
echo         student_gender NVARCHAR^(10^) NOT NULL,
echo         student_address NVARCHAR^(255^) NOT NULL,
echo         student_grade NVARCHAR^(20^) NOT NULL,
echo         student_section NVARCHAR^(20^) NOT NULL,
echo         student_image NVARCHAR^(500^) NULL,
echo         student_status NVARCHAR^(20^) NOT NULL,
echo         date_insert DATETIME DEFAULT GETDATE^(^),
echo         date_update DATETIME NULL,
echo         date_delete DATETIME NULL
echo     ^);
echo     PRINT 'Students table created'^;
echo END
echo GO
echo.
echo IF NOT EXISTS ^(SELECT * FROM sys.tables WHERE name = 'teachers'^)
echo BEGIN
echo     CREATE TABLE teachers ^(
echo         id INT IDENTITY^(1,1^) PRIMARY KEY,
echo         teacher_id NVARCHAR^(50^) NOT NULL UNIQUE,
echo         teacher_name NVARCHAR^(100^) NOT NULL,
echo         teacher_gender NVARCHAR^(10^) NOT NULL,
echo         teacher_address NVARCHAR^(255^) NOT NULL,
echo         teacher_image NVARCHAR^(500^) NULL,
echo         teacher_status NVARCHAR^(20^) NOT NULL,
echo         date_insert DATETIME DEFAULT GETDATE^(^),
echo         date_update DATETIME NULL,
echo         date_delete DATETIME NULL
echo     ^);
echo     PRINT 'Teachers table created'^;
echo END
echo GO
) > setup_temp.sql

REM Run SQL using the connection string format
sqlcmd -S "(LocalDB)\MSSQLLocalDB" -d "master" -i setup_temp.sql -E 2>nul

if %ERRORLEVEL% EQU 0 (
    echo.
    echo SUCCESS! Database setup complete.
    echo.
    echo You can now log in with:
    echo   Username: admin
    echo   Password: admin
    echo.
) else (
    echo.
    echo There was an issue. The database tables will be created
    echo automatically when you first run the application.
    echo Just make sure you create the admin user manually!
    echo.
)

REM Cleanup
del setup_temp.sql 2>nul

echo Press any key to exit...
pause > nul
