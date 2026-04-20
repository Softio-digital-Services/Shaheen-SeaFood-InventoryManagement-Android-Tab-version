$con = New-Object System.Data.SqlClient.SqlConnection 'Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Khale\Desktop\Personal\C# Projects\Car Parts Inventory System\CarPartsInventorySystem\CarPartsInventorySystem\bin\Debug\Data\carparts.mdf;Integrated Security=True;Connect Timeout=30;'
$con.Open()
try {
    $cmd = $con.CreateCommand()
    
    # 1. Update Customers (Receivables)
    # Check if 'current_balance' exists, if not add it
    $cmd.CommandText = @"
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'current_balance' AND Object_ID = Object_ID(N'customers'))
    BEGIN
        ALTER TABLE customers ADD current_balance DECIMAL(18,2) DEFAULT 0;
        PRINT 'Added current_balance to customers.';
    END
"@
    $cmd.ExecuteNonQuery()

    # 2. Update Suppliers (Payables)
    $cmd.CommandText = @"
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'balance_due' AND Object_ID = Object_ID(N'suppliers'))
    BEGIN
        ALTER TABLE suppliers ADD balance_due DECIMAL(18,2) DEFAULT 0;
        PRINT 'Added balance_due to suppliers.';
    END
"@
    $cmd.ExecuteNonQuery()

    # 3. Update Orders (Link to Customer & Payment Status)
    $cmd.CommandText = @"
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'customer_id' AND Object_ID = Object_ID(N'orders'))
    BEGIN
        ALTER TABLE orders ADD customer_id INT NULL;
        ALTER TABLE orders ADD payment_status NVARCHAR(50) DEFAULT 'Unpaid';
        ALTER TABLE orders ADD amount_paid DECIMAL(18,2) DEFAULT 0;
        PRINT 'Added finance columns to orders.';
    END
"@
    $cmd.ExecuteNonQuery()

    Write-Host "Database finance schema updated successfully."

}
catch {
    Write-Host "Error: $($_.Exception.Message)"
}
finally {
    $con.Close()
}
