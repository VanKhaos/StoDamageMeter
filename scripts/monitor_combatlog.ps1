# Live Combat Log Monitor
# Überwacht das STO Combat Log und zeigt neue Zeilen in Echtzeit an

param(
    [string]$LogPath = "G:\SteamLibrary\steamapps\common\Star Trek Online\Star Trek Online\Live\logs\GameClient\combatlog.log",
    [string]$OutputLog = "live_monitor.log"
)

Write-Host "=== STO Combat Log Live Monitor ===" -ForegroundColor Cyan
Write-Host "Log-Datei: $LogPath" -ForegroundColor Yellow
Write-Host "Output: $OutputLog" -ForegroundColor Yellow
Write-Host ""

# Prüfe ob Log existiert
if (-not (Test-Path $LogPath)) {
    Write-Host "FEHLER: Combat Log nicht gefunden!" -ForegroundColor Red
    Write-Host "Pfad: $LogPath" -ForegroundColor Red
    exit 1
}

# Hole aktuelle Datei-Größe
$lastSize = (Get-Item $LogPath).Length
$startTime = Get-Date
$lineCount = 0

Write-Host "Start-Zeit: $($startTime.ToString('HH:mm:ss'))" -ForegroundColor Green
Write-Host "Start-Größe: $([math]::Round($lastSize / 1KB, 2)) KB" -ForegroundColor Green
Write-Host ""
Write-Host "Warte auf neue Log-Einträge... (Strg+C zum Beenden)" -ForegroundColor Cyan
Write-Host "=" * 80
Write-Host ""

# Initialisiere Output-Log
"=== Live Combat Log Monitor Started ===" | Out-File $OutputLog
"Start-Zeit: $($startTime.ToString('yyyy-MM-dd HH:mm:ss'))" | Out-File $OutputLog -Append
"" | Out-File $OutputLog -Append

try {
    while ($true) {
        Start-Sleep -Milliseconds 500
        
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
                $lineCount++
                $timestamp = (Get-Date).ToString('HH:mm:ss.fff')
                
                # Farbige Ausgabe je nach Zeilen-Typ
                if ($line -match "::") {
                    Write-Host "[$timestamp] " -ForegroundColor DarkGray -NoNewline
                    
                    # Parse Zeile für bessere Ausgabe
                    $parts = $line -split "::", 2
                    if ($parts.Count -eq 2) {
                        $logTime = $parts[0]
                        $data = $parts[1]
                        
                        Write-Host "$logTime" -ForegroundColor Green -NoNewline
                        Write-Host " :: " -ForegroundColor DarkGray -NoNewline
                        
                        # Highlight Player-Name (erster Teil vor dem Komma)
                        $dataParts = $data -split ",", 2
                        if ($dataParts.Count -eq 2) {
                            Write-Host "$($dataParts[0])" -ForegroundColor Yellow -NoNewline
                            Write-Host ",$($dataParts[1])" -ForegroundColor White
                        } else {
                            Write-Host "$data" -ForegroundColor White
                        }
                    } else {
                        Write-Host "$line" -ForegroundColor White
                    }
                } else {
                    Write-Host "[$timestamp] $line" -ForegroundColor Gray
                }
                
                # In Log-Datei schreiben
                "[$timestamp] $line" | Out-File $OutputLog -Append
            }
            
            # Status-Update
            $duration = (Get-Date) - $startTime
            Write-Host ""
            Write-Host ">>> +$($newLines.Count) neue Zeilen | Gesamt: $lineCount | Laufzeit: $($duration.ToString('mm\:ss'))" -ForegroundColor Cyan
            Write-Host ""
            
            $lastSize = $currentSize
        }
    }
}
catch {
    Write-Host ""
    Write-Host "Monitor beendet." -ForegroundColor Yellow
    Write-Host "Statistik:" -ForegroundColor Cyan
    Write-Host "  Gesamte Zeilen: $lineCount" -ForegroundColor White
    Write-Host "  Laufzeit: $((Get-Date) - $startTime)" -ForegroundColor White
    Write-Host "  Log gespeichert: $OutputLog" -ForegroundColor White
}


