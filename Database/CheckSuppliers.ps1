$con = New-Object System.Data.SqlClient.SqlConnection 'Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Khale\Desktop\Personal\C# Projects\Car Parts Inventory System\CarPartsInventorySystem\CarPartsInventorySystem\bin\Debug\Data\carparts.mdf;Integrated Security=True;Connect Timeout=30;'
$con.Open()
try {
    $cmd = $con.CreateCommand()
    $cmd.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'suppliers'"
    $rdr = $cmd.ExecuteReader()
    while ($rdr.Read()) {
        Write-Host $rdr["COLUMN_NAME"]
    }
}
catch {
    Write-Host "Error: $($_.Exception.Message)"
}
finally {
    $con.Close()
}
