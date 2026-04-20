$dbPath = "C:\Users\Khale\Desktop\Personal\C# Projects\Car Parts Inventory System\CarPartsInventorySystem\CarPartsInventorySystem\bin\Debug\Data\carparts.mdf"
$connectionString = "Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=$dbPath;Integrated Security=True;Connect Timeout=30;Database=CarPartsDB"

$query = @"
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='transactions' AND xtype='U')
BEGIN
    CREATE TABLE transactions (
        id INT PRIMARY KEY IDENTITY(1,1),
        action_type NVARCHAR(50),
        part_name NVARCHAR(100),
        description NVARCHAR(255),
        timestamp DATETIME DEFAULT GETDATE(),
        username NVARCHAR(50)
    )
END
"@

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection $connectionString
    $connection.Open()
    $command = $connection.CreateCommand()
    $command.CommandText = $query
    $command.ExecuteNonQuery()
    $connection.Close()
    Write-Host "Transactions table created successfully."
}
catch {
    Write-Host "Error: $($_.Exception.Message)"
}
