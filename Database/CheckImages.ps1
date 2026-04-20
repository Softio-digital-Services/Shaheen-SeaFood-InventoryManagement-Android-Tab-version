Add-Type -AssemblyName System.Drawing
$path = "C:\Users\Khale\Desktop\Personal\C# Projects\Car Parts Inventory System\CarPartsInventorySystem\CarPartsInventorySystem\bin\Debug\Assets"
Get-ChildItem "$path\*.png" | ForEach-Object {
    try {
        $img = [System.Drawing.Image]::FromFile($_.FullName)
        Write-Host "$($_.Name): $($img.Width)x$($img.Height)"
        $img.Dispose()
    }
    catch {
        Write-Host "Error reading $($_.Name): $($_.Exception.Message)"
    }
}
