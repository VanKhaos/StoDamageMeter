# App Logs Monitor
# Überwacht die Anwendungs-Logs um zu sehen ob der FileWatcher arbeitet

param(
    [string]$LogPath = "Debug\logs\frontend_debug.log"
)

Write-Host "=== STO Damage Meter App Log Monitor ===" -ForegroundColor Cyan
Write-Host "Log-Datei: $LogPath" -ForegroundColor Yellow
Write-Host ""

# Prüfe ob Log existiert
if (-not (Test-Path $LogPath)) {
    Write-Host "WARNUNG: App-Log noch nicht vorhanden. Warte..." -ForegroundColor Yellow
    Write-Host "Starte die Anwendung und wechsle zum 'Live Combat' Tab!" -ForegroundColor Yellow
    
    # Warte bis Log existiert
    while (-not (Test-Path $LogPath)) {
        Start-Sleep -Seconds 1
    }
    
    Write-Host "Log-Datei gefunden!" -ForegroundColor Green
}

# Hole aktuelle Datei-Größe
$lastSize = (Get-Item $LogPath).Length
$startTime = Get-Date

Write-Host "Start-Zeit: $($startTime.ToString('HH:mm:ss'))" -ForegroundColor Green
Write-Host ""
Write-Host "Warte auf neue Log-Einträge... (Strg+C zum Beenden)" -ForegroundColor Cyan
Write-Host "=" * 80
Write-Host ""

try {
    while ($true) {
        Start-Sleep -Milliseconds 200
        
        # Hole aktuelle Datei-Größe
        $currentSize = (Get-Item $LogPath).Length
        
        if ($currentSize -gt $lastSize) {
            # Neue Daten vorhanden!
            $bytesToRead = $currentSize - $lastSize
            
            # Lese neue Zeilen
            $fileStream = [System.IO.File]::Open($LogPath, 'Open', 'Read', 'ReadWrite')
            $fileStream.Seek($lastSize, [System.IO.SeekOrigin]::Begin) | Out-Null
            
            $buffer = New-Object byte[] $bytesToRead
            $fileStream.Read($buffer, 0, $bytesToRead) | Out-Null
            $fileStream.Close()
            
            $newContent = [System.Text.Encoding]::UTF8.GetString($buffer)
            $newLines = $newContent -split "`n" | Where-Object { $_.Trim() -ne "" }
            
            foreach ($line in $newLines) {
                $timestamp = (Get-Date).ToString('HH:mm:ss.fff')
                
                # Farbige Ausgabe je nach Log-Level
                if ($line -match "\[ERR\]" -or $line -match "ERROR") {
                    Write-Host "[$timestamp] $line" -ForegroundColor Red
                }
                elseif ($line -match "\[WRN\]" -or $line -match "WARNING") {
                    Write-Host "[$timestamp] $line" -ForegroundColor Yellow
                }
                elseif ($line -match "FileWatcher" -or $line -match "NewLinesDetected" -or $line -match "LiveCombat") {
                    Write-Host "[$timestamp] $line" -ForegroundColor Green
                }
                elseif ($line -match "incremental_update" -or $line -match "live_parse") {
                    Write-Host "[$timestamp] $line" -ForegroundColor Cyan
                }
                else {
                    Write-Host "[$timestamp] $line" -ForegroundColor Gray
                }
            }
            
            $lastSize = $currentSize
        }
    }
}
catch {
    Write-Host ""
    Write-Host "Monitor beendet." -ForegroundColor Yellow
}

