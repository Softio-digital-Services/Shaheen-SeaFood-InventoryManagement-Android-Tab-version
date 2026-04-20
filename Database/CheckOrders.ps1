$con = New-Object System.Data.SqlClient.SqlConnection 'Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Khale\Desktop\Personal\C# Projects\Car Parts Inventory System\CarPartsInventorySystem\CarPartsInventorySystem\bin\Debug\Data\carparts.mdf;Integrated Security=True;Connect Timeout=30;'
$con.Open()
try {
    $cmd = $con.CreateCommand()
    $cmd.CommandText = "SELECT TOP 1 * FROM orders"
    try { $cmd.ExecuteNonQuery(); Write-Host "orders table exists." } catch { Write-Host "orders table MISSING." }

    $cmd.CommandText = "SELECT TOP 1 * FROM order_items"
    try { $cmd.ExecuteNonQuery(); Write-Host "order_items table exists." } catch { Write-Host "order_items table MISSING." }
    
    # Check columns in orders
    $cmd.CommandText = "SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('orders')"
    $reader = $cmd.ExecuteReader()
    Write-Host "Columns in orders:"
    while ($reader.Read()) { Write-Host "- $($reader['name'])" }
    $reader.Close()

}
catch {
    Write-Host "Error: $($_.Exception.Message)"
}
finally {
    $con.Close()
}
