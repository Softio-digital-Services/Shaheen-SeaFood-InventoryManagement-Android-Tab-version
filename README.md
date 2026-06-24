# Car Parts Inventory Management System

## ðŸ“‹ Overview

Complete inventory management system for car parts businesses with POS, customer management, supplier tracking, and comprehensive reporting.

## ðŸš€ Quick Start

### Prerequisites
- Windows 7 or later
- .NET Framework 4.7.2 or higher
- SQL Server LocalDB (included with Visual Studio)

### Running the Application

**Option 1: Run Executable (Recommended)**
```
bin\Debug\CarPartsShaheen_InventoryManagement_Android.exe
```

**Option 2: Build from Source**
```powershell
dotnet build CarPartsShaheen_InventoryManagement_Android.csproj
```

### First-Time Setup

1. **License Activation**
   - On first launch, you'll see the License Activation dialog
   - Enter a valid license key OR start a 30-day trial
   - License keys are generated using the separate License Generator tool

2. **Default Login**
   - Username: `admin`
   - Password: `admin123`

3. **Database Setup**
   - Database automatically creates on first run
   - Location: `bin\Debug\Data\carparts.mdf`
   - To reset database, run: `Database\SetupDatabase.bat`

## âœ¨ Features

### Core Modules
- **Dashboard** - Real-time analytics and KPIs
- **Inventory Management** - Parts tracking with low-stock alerts
- **Point of Sale (POS)** - Quick sales processing
- **Customer Management** - Customer profiles and purchase history
- **Supplier Management** - Supplier tracking and orders
- **Reports** - Sales, inventory, and financial reports
- **User Management** - Role-based access control

### Import/Export
- **CSV Import/Export** - Parts, Customers, Suppliers
- **Excel Compatible** - TSV format for Excel compatibility
- **Templates** - Sample templates in `Templates\` folder
- **Duplicate Detection** - Automatic skip of existing records
- **Auto-Create Categories** - Categories created during import

### Licensing System
- **Yearly Subscriptions** - 1-10 year licenses
- **30-Day Trial** - Full feature access
- **Hardware-Locked** - Prevents casual copying
- **Offline Activation** - No internet required

## ðŸ“ Project Structure

```
Shaheen-InventoryManagement-Android-Final/
â”œâ”€â”€ bin/Debug/
â”‚   â”œâ”€â”€ CarPartsShaheen_InventoryManagement_Android.exe    â† Main Application
â”‚   â”œâ”€â”€ Data/carparts.mdf              â† Database
â”‚   â”œâ”€â”€ Assets/                        â† UI Icons
â”‚   â””â”€â”€ Logs/                          â† Error Logs
â”œâ”€â”€ Database/
â”‚   â”œâ”€â”€ SetupDatabase.bat              â† Database Setup
â”‚   â”œâ”€â”€ CreateCarPartsDatabase.sql     â† Schema
â”‚   â””â”€â”€ *.ps1                          â† Helper Scripts
â”œâ”€â”€ Forms/                             â† UI Forms
â”œâ”€â”€ Helpers/                           â† Utility Classes
â”œâ”€â”€ Services/                          â† Business Logic
â”œâ”€â”€ Controls/                          â† Custom Controls
â”œâ”€â”€ Data/                              â† Data Models
â””â”€â”€ Templates/
    â””â”€â”€ Parts_Import_Template.csv      â† Import Template
```

## ðŸ”§ Configuration

### Database Connection
Edit `Helpers\DatabaseConfig.cs`:
```csharp
public static string ConnectionString = 
    @"Data Source=(LocalDB)\MSSQLLocalDB;
      AttachDbFilename=|DataDirectory|\Data\carparts.mdf;
      Integrated Security=True";
```

### Theme Customization
Edit `Helpers\ThemeConfig.cs`:
```csharp
public static Color PrimaryColor = Color.FromArgb(59, 130, 246);
public static Color AccentColor = Color.FromArgb(16, 185, 129);
```

## ðŸ“Š Import/Export Guide

### Exporting Data

1. Navigate to Parts/Customers/Suppliers tab
2. Click **ðŸ“¤ Export** button
3. Choose **Export to CSV** or **Export to Excel**
4. Select save location
5. File is created with timestamp

### Importing Data

1. Prepare CSV file with required columns:
   - **Parts**: PartNumber, PartName, Category, Quantity, MinimumStock, UnitPrice, Location, Status
   - **Customers**: CustomerName, Email, Phone, Address, City, PostalCode, Notes
   - **Suppliers**: SupplierName, ContactPerson, Email, Phone, Address, City, PostalCode, Website, Notes

2. Click **ðŸ“¥ Import** button
3. Choose **Import from CSV** or **Import from Excel**
4. Select file
5. Review import summary (imported/skipped counts)

**Sample CSV (Parts):**
```csv
PartNumber,PartName,Category,Quantity,MinimumStock,UnitPrice,Location,Status
P001,Brake Pad Set,Brakes,50,10,45.99,A1-Shelf2,Active
P002,Oil Filter,Filters,100,20,8.99,B2-Shelf1,Active
```

## ðŸ”‘ License Management

### Viewing License Info
1. Click user avatar (top-right)
2. Select "ðŸ“„ License Info"
3. View license type, expiration, and status

### Renewing License
1. Obtain new license key from administrator
2. Click "License Info"
3. Click "Renew License"
4. Enter new key

### Trial Period
- 30 days full access
- All features unlocked
- Upgrade to full license anytime

## ðŸ—„ï¸ Database Management

### Backup Database
```powershell
copy "bin\Debug\Data\carparts.mdf" "Backup\carparts_backup_$(Get-Date -Format 'yyyyMMdd').mdf"
```

### Reset Database
```powershell
cd Database
.\SetupDatabase.bat
```

### Initialize Sample Data
```powershell
cd Database
.\InitCategories.ps1
```

## ðŸ› Troubleshooting

### "Database connection failed"
- Ensure SQL Server LocalDB is installed
- Check database file exists: `bin\Debug\Data\carparts.mdf`
- Run `Database\SetupDatabase.bat`

### "License activation failed"
- Verify license key is copied correctly (no spaces)
- Check hardware hasn't changed significantly
- Try "Start 30-Day Trial" option

### "Import failed"
- Verify CSV format matches template
- Check for required columns (PartNumber, PartName)
- Ensure no duplicate part numbers

### Application won't start
- Check error logs: `bin\Debug\Logs\error_log_YYYYMMDD.txt`
- Verify .NET Framework 4.7.2+ is installed
- Run as Administrator if needed

## ðŸ“ Development

### Building
```powershell
dotnet build CarPartsShaheen_InventoryManagement_Android.csproj
```

### Running Tests
```powershell
# Test database connection
.\Database\CheckSchema.ps1

# Test data integrity
.\Database\CheckConstraints.ps1
```

### Adding New Features
1. Create feature branch
2. Add forms in `Forms\`
3. Add business logic in `Services\`
4. Update `MainForm.cs` navigation
5. Test thoroughly
6. Build and verify

## ðŸ“š Documentation

- **User Guide**: See `README.md` (this file)
- **Database Schema**: `Database\CreateCarPartsDatabase.sql`
- **Import Templates**: `Templates\Parts_Import_Template.csv`
- **Error Logs**: `bin\Debug\Logs\`

## ðŸ” Security

- Passwords hashed with SHA256
- License data encrypted (AES)
- Hardware-locked licenses
- Role-based access control
- Audit logging for critical operations

## ðŸ“ž Support

For issues or questions:
1. Check error logs: `bin\Debug\Logs\`
2. Review troubleshooting section above
3. Contact system administrator

## ðŸ“„ License

This software requires a valid license key for commercial use.
- Trial: 30 days
- Yearly: 1-10 years
- Contact administrator for license keys

---

**Version:** 2.0  
**Last Updated:** February 2026  
**Framework:** .NET Framework 4.7.2
