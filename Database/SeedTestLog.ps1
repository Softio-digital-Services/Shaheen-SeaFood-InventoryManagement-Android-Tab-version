$con = New-Object System.Data.SqlClient.SqlConnection 'Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Khale\Desktop\Personal\C# Projects\Car Parts Inventory System\CarPartsInventorySystem\CarPartsInventorySystem\bin\Debug\Data\carparts.mdf;Integrated Security=True;Connect Timeout=30;'
$con.Open()
try {
    $cmd = $con.CreateCommand()
    $cmd.CommandText = "INSERT INTO transactions (timestamp, action_type, part_name, description, username) VALUES (GETDATE(), 'TEST', 'System Test', 'Verifying Log View', 'Admin')"
    $cmd.ExecuteNonQuery()
    Write-Host "Test transaction inserted successfully."
}
catch {
    Write-Host "Error: $($_.Exception.Message)"
}
finally {
    $con.Close()
}
