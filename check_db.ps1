$dbPath = "c:\Users\emirt\Desktop\C#\PalmPlayer-main\data\player"
Add-Type -AssemblyName System.Data

$connection = New-Object System.Data.SQLite.SQLiteConnection("Data Source=$dbPath")
$connection.Open()

$command = $connection.CreateCommand()
$command.CommandText = "PRAGMA table_info(Songs);"
$reader = $command.ExecuteReader()

Write-Host "Songs table schema:"
while ($reader.Read()) {
    Write-Host "$($reader['cid']): $($reader['name']) - $($reader['type'])"
}

$reader.Close()

$command.CommandText = "SELECT COUNT(*) FROM Songs;"
$count = $command.ExecuteScalar()
Write-Host "`nTotal songs in database: $count"

$connection.Close()
