$con = New-Object System.Data.SqlClient.SqlConnection 'Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Khale\Desktop\Personal\C# Projects\Car Parts Inventory System\CarPartsInventorySystem\CarPartsInventorySystem\bin\Debug\Data\carparts.mdf;Integrated Security=True;Connect Timeout=30;'
$con.Open()
try {
    $cmd = $con.CreateCommand()
    
    # Create categories table
    $cmd.CommandText = @"
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='categories' AND xtype='U')
    CREATE TABLE categories (
        id INT IDENTITY(1,1) PRIMARY KEY,
        category_name NVARCHAR(100) NOT NULL UNIQUE,
        description NVARCHAR(255),
        date_created DATETIME DEFAULT GETDATE()
    )
"@
    $cmd.ExecuteNonQuery()
    Write-Host "Table 'categories' checked/created."

    # Populate default categories if empty
    $cmd.CommandText = "SELECT COUNT(*) FROM categories"
    $count = $cmd.ExecuteScalar()
    
    if ($count -eq 0) {
        $cmd.CommandText = @"
        INSERT INTO categories (category_name, description) VALUES 
        ('Engine', 'Internal engine components'),
        ('Brakes', 'Braking system parts'),
        ('Suspension', 'Shocks, struts, and control arms'),
        ('Electrical', 'Sensors, batteries, wiring'),
        ('Body', 'Panels, bumpers, mirrors'),
        ('Filters', 'Oil, air, and cabin filters')
"@
        $cmd.ExecuteNonQuery()
        Write-Host "Default categories inserted."
    }

    # Add category column to parts table if missing
    $cmd.CommandText = @"
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'category' AND Object_ID = Object_ID(N'parts'))
    BEGIN
        ALTER TABLE parts ADD category NVARCHAR(100);
        PRINT 'Added category column to parts.';
    END
"@
    $cmd.ExecuteNonQuery()

}
catch {
    Write-Host "Error: $($_.Exception.Message)"
}
finally {
    $con.Close()
}
