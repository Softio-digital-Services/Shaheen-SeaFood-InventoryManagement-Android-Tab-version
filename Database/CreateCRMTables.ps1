$con = New-Object System.Data.SqlClient.SqlConnection 'Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Khale\Desktop\Personal\C# Projects\Car Parts Inventory System\CarPartsInventorySystem\CarPartsInventorySystem\bin\Debug\Data\carparts.mdf;Integrated Security=True;Connect Timeout=30;'
$con.Open()
try {
    $cmd = $con.CreateCommand()
    
    # Create customers
    $cmd.CommandText = @"
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='customers' AND xtype='U')
    CREATE TABLE customers (
        customer_id INT IDENTITY(1,1) PRIMARY KEY,
        full_name NVARCHAR(100) NOT NULL,
        phone NVARCHAR(20),
        email NVARCHAR(100),
        address NVARCHAR(255),
        date_added DATETIME DEFAULT GETDATE()
    )
"@
    $cmd.ExecuteNonQuery()
    Write-Host "Table 'customers' checked/created."

    # Create suppliers
    $cmd.CommandText = @"
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='suppliers' AND xtype='U')
    CREATE TABLE suppliers (
        supplier_id INT IDENTITY(1,1) PRIMARY KEY,
        company_name NVARCHAR(100) NOT NULL,
        contact_person NVARCHAR(100),
        phone NVARCHAR(20),
        email NVARCHAR(100),
        address NVARCHAR(255),
        date_added DATETIME DEFAULT GETDATE()
    )
"@
    $cmd.ExecuteNonQuery()
    Write-Host "Table 'suppliers' checked/created."

}
catch {
    Write-Host "Error: $($_.Exception.Message)"
}
finally {
    $con.Close()
}
