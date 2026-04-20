$con = New-Object System.Data.SqlClient.SqlConnection 'Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Khale\Desktop\Personal\C# Projects\Car Parts Inventory System\CarPartsInventorySystem\CarPartsInventorySystem\bin\Debug\Data\carparts.mdf;Integrated Security=True;Connect Timeout=30;'
$con.Open()
try {
    $cmd = $con.CreateCommand()

    # 1. Transactions
    $cmd.CommandText = @"
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='transactions' AND xtype='U')
    CREATE TABLE transactions (
        id INT IDENTITY(1,1) PRIMARY KEY,
        action_type NVARCHAR(50),
        part_name NVARCHAR(100),
        description NVARCHAR(255),
        username NVARCHAR(50),
        timestamp DATETIME DEFAULT GETDATE(),
        transaction_date DATETIME DEFAULT GETDATE() -- Alias for older queries
    )
"@
    $cmd.ExecuteNonQuery()
    Write-Host "Table 'transactions' checked."

    # 2. Customers
    $cmd.CommandText = @"
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='customers' AND xtype='U')
    CREATE TABLE customers (
        customer_id INT IDENTITY(1,1) PRIMARY KEY,
        full_name NVARCHAR(100),
        phone NVARCHAR(50),
        email NVARCHAR(100),
        address NVARCHAR(200),
        current_balance DECIMAL(18,2) DEFAULT 0,
        type NVARCHAR(50)
    )
"@
    $cmd.ExecuteNonQuery()
    Write-Host "Table 'customers' checked."

    # 3. Suppliers
    $cmd.CommandText = @"
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='suppliers' AND xtype='U')
    CREATE TABLE suppliers (
        supplier_id INT IDENTITY(1,1) PRIMARY KEY,
        company_name NVARCHAR(100),
        phone NVARCHAR(50),
        email NVARCHAR(100),
        address NVARCHAR(200),
        balance_due DECIMAL(18,2) DEFAULT 0,
        type NVARCHAR(50)
    )
"@
    $cmd.ExecuteNonQuery()
    Write-Host "Table 'suppliers' checked."

    # 4. Payments
    $cmd.CommandText = @"
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='payments' AND xtype='U')
    CREATE TABLE payments (
        payment_id INT IDENTITY(1,1) PRIMARY KEY,
        entity_type NVARCHAR(50), -- Customer or Supplier
        entity_id INT,
        amount DECIMAL(18,2),
        payment_date DATETIME DEFAULT GETDATE(),
        notes NVARCHAR(255)
    )
"@
    $cmd.ExecuteNonQuery()
    Write-Host "Table 'payments' checked."

}
catch {
    Write-Host "Error: $($_.Exception.Message)"
}
finally {
    $con.Close()
}
