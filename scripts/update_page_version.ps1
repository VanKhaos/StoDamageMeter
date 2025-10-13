# Schnell-Script zum Updaten der Version in GitHub Pages

$oldVersion = "1.2.3"
$newVersion = "1.2.4"
$newDate = "13. Oktober 2025"

$content = Get-Content "docs/index.html" -Raw
$content = $content -replace "v$oldVersion", "v$newVersion"
$content = $content -replace "$oldVersion", "$newVersion"
$content = $content -replace "11\. Oktober 2025", "$newDate"

Set-Content "docs/index.html" -Value $content

Write-Host "âœ… Version in docs/index.html updated: $oldVersion -> $newVersion"
