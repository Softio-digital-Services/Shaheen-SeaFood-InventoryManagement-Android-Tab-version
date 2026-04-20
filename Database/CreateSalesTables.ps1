$con = New-Object System.Data.SqlClient.SqlConnection 'Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Khale\Desktop\Personal\C# Projects\Car Parts Inventory System\CarPartsInventorySystem\CarPartsInventorySystem\bin\Debug\Data\carparts.mdf;Integrated Security=True;Connect Timeout=30;'
$con.Open()
try {
    $cmd = $con.CreateCommand()
    
    # Create orders table
    $cmd.CommandText = @"
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='orders' AND xtype='U')
    CREATE TABLE orders (
        order_id INT IDENTITY(1,1) PRIMARY KEY,
        order_date DATETIME DEFAULT GETDATE(),
        total_amount DECIMAL(18,2) NOT NULL,
        user_name NVARCHAR(50)
    )
"@
    $cmd.ExecuteNonQuery()
    Write-Host "Table 'orders' checked/created."

    # Create order_items table
    $cmd.CommandText = @"
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='order_items' AND xtype='U')
    CREATE TABLE order_items (
        id INT IDENTITY(1,1) PRIMARY KEY,
        order_id INT NOT NULL,
        part_id INT NOT NULL,
        quantity INT NOT NULL,
        price_at_sale DECIMAL(18,2) NOT NULL,
        FOREIGN KEY (order_id) REFERENCES orders(order_id),
        FOREIGN KEY (part_id) REFERENCES parts(id)
    )
"@
    $cmd.ExecuteNonQuery()
    Write-Host "Table 'order_items' checked/created."

}
catch {
    Write-Host "Error: $($_.Exception.Message)"
}
finally {
    $con.Close()
}
