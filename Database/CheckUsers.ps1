$dbPath = "C:\Users\Khale\Desktop\Personal\C# Projects\Car Parts Inventory System\CarPartsInventorySystem\CarPartsInventorySystem\bin\Debug\Data\carparts.mdf"
$connectionString = "Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=$dbPath;Integrated Security=True;Connect Timeout=30"

$query = "SELECT username, password, role FROM users"

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection $connectionString
    $connection.Open()
    
    $command = $connection.CreateCommand()
    $command.CommandText = $query
    
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $command
    $dataset = New-Object System.Data.DataSet
    $adapter.Fill($dataset)
    
    if ($dataset.Tables[0].Rows.Count -eq 0) {
        Write-Host "No users found in the database."
    } else {
        Write-Host "Users found:"
        foreach ($row in $dataset.Tables[0].Rows) {
            Write-Host "Username: $($row.username), Password: $($row.password), Role: $($row.role)"
        }
    }
    
    $connection.Close()
} catch {
    Write-Host "Error: $($_.Exception.Message)"
}
