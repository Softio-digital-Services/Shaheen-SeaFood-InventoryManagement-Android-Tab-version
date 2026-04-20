$con = New-Object System.Data.SqlClient.SqlConnection 'Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Khale\Desktop\Personal\C# Projects\Car Parts Inventory System\CarPartsInventorySystem\CarPartsInventorySystem\bin\Debug\Data\carparts.mdf;Integrated Security=True;Connect Timeout=30;'
$con.Open()
try {
    $cmd = $con.CreateCommand()
    
    # 1. Add 'type' to Customers
    $cmd.CommandText = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'type' AND Object_ID = Object_ID(N'customers')) ALTER TABLE customers ADD type NVARCHAR(50) DEFAULT 'Individual';"
    $cmd.ExecuteNonQuery()
    Write-Host "Added type to customers."

    # 2. Add 'type' to Suppliers
    $cmd.CommandText = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'type' AND Object_ID = Object_ID(N'suppliers')) ALTER TABLE suppliers ADD type NVARCHAR(50) DEFAULT 'Company';"
    $cmd.ExecuteNonQuery()
    Write-Host "Added type to suppliers."

    # 3. Create active Payments table
    $cmd.CommandText = @"
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='payments' AND xtype='U')
    CREATE TABLE payments (
        payment_id INT IDENTITY(1,1) PRIMARY KEY,
        entity_type NVARCHAR(20) NOT NULL, -- 'Customer' or 'Supplier'
        entity_id INT NOT NULL,
        amount DECIMAL(18,2) NOT NULL,
        payment_date DATETIME DEFAULT GETDATE(),
        notes NVARCHAR(255)
    )
"@
    $cmd.ExecuteNonQuery()
    Write-Host "Created payments table."

}
catch {
    Write-Host "Error: $($_.Exception.Message)"
}
finally {
    $con.Close()
}
