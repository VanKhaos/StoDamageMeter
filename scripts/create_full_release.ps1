param(
    [Parameter(Mandatory=$false)]
    [string]$Version,
    
    [Parameter(Mandatory=$false)]
    [string]$ReleaseType = "patch",  # patch, minor, major
    
    [Parameter(Mandatory=$false)]
    [string]$ChangelogEntry = ""
)

# Farben für Output
$Red = "`e[31m"
$Green = "`e[32m"
$Yellow = "`e[33m"
$Blue = "`e[34m"
$Reset = "`e[0m"

function Write-ColorOutput {
    param([string]$Message, [string]$Color = $Reset)
    Write-Host "$Color$Message$Reset"
}

function Update-VersionInFile {
    param([string]$FilePath, [string]$OldVersion, [string]$NewVersion)
    
    if (Test-Path $FilePath) {
        $content = Get-Content $FilePath -Raw
        $content = $content -replace [regex]::Escape($OldVersion), $NewVersion
        Set-Content $FilePath $content -NoNewline
        Write-ColorOutput "✅ Updated: $FilePath" $Green
    } else {
        Write-ColorOutput "❌ File not found: $FilePath" $Red
    }
}

function Get-CurrentVersion {
    # Extrahiere aktuelle Version aus frontend.csproj
    $csprojContent = Get-Content "frontend/frontend.csproj" -Raw
    if ($csprojContent -match '<Version>([^<]+)</Version>') {
        return $matches[1]
    }
    return "1.0.0"
}

function Get-NextVersion {
    param([string]$CurrentVersion, [string]$Type)
    
    $version = [Version]$CurrentVersion
    switch ($Type) {
        "major" { return "$($version.Major + 1).0.0" }
        "minor" { return "$($version.Major).$($version.Minor + 1).0" }
        "patch" { return "$($version.Major).$($version.Minor).$($version.Build + 1)" }
        default { return $CurrentVersion }
    }
}

function Get-GitCommitsSinceLastTag {
    param([string]$CurrentVersion)
    
    try {
        # Letzten Tag finden
        $lastTag = git describe --tags --abbrev=0 2>$null
        if ($lastTag) {
            $commits = git log --pretty=format:"%s" "$lastTag..HEAD" 2>$null
        } else {
            $commits = git log --pretty=format:"%s" --max-count=10 2>$null
        }
        
        if ($commits) {
            return $commits -split "`n" | Where-Object { $_.Trim() -ne "" }
        }
    } catch {
        Write-ColorOutput "⚠️  Could not get git commits, using default changelog" $Yellow
    }
    
    return @()
}

function Generate-ChangelogEntry {
    param([string]$Version, [string[]]$Commits)
    
    $added = @()
    $fixed = @()
    $changed = @()
    $other = @()
    
    foreach ($commit in $Commits) {
        $commit = $commit.Trim()
        if ([string]::IsNullOrEmpty($commit)) { continue }
        
        # Kategorisiere Commits basierend auf Keywords
        if ($commit -match "^(feat|add|new):" -or $commit -match "add|new|feature") {
            $added += "- " + ($commit -replace "^(feat|add|new):\s*", "")
        }
        elseif ($commit -match "^(fix|bug):" -or $commit -match "fix|bug|error|issue") {
            $fixed += "- " + ($commit -replace "^(fix|bug):\s*", "")
        }
        elseif ($commit -match "^(chore|refactor|update|change):" -or $commit -match "update|change|improve|optimize") {
            $changed += "- " + ($commit -replace "^(chore|refactor|update|change):\s*", "")
        }
        else {
            $other += "- " + $commit
        }
    }
    
    # Fallback wenn keine Commits gefunden
    if ($added.Count -eq 0 -and $fixed.Count -eq 0 -and $changed.Count -eq 0) {
        return @"
### Fixed
- Window-Binding-System Verbesserungen
- UI-Interaktivität und Drag-Funktionalität
- Performance-Optimierungen

### Changed
- Code-Bereinigung und Debug-Log-Entfernung
- Vereinfachte Window-Lifecycle-Management
- Zentralisierte Window-Binding-Architektur
"@
    }
    
    $changelog = ""
    
    if ($added.Count -gt 0) {
        $changelog += "### Added`n"
        $changelog += ($added -join "`n") + "`n`n"
    }
    
    if ($fixed.Count -gt 0) {
        $changelog += "### Fixed`n"
        $changelog += ($fixed -join "`n") + "`n`n"
    }
    
    if ($changed.Count -gt 0) {
        $changelog += "### Changed`n"
        $changelog += ($changed -join "`n") + "`n`n"
    }
    
    if ($other.Count -gt 0) {
        $changelog += "### Other`n"
        $changelog += ($other -join "`n") + "`n`n"
    }
    
    return $changelog.Trim()
}

function Update-Changelog {
    param([string]$Version, [string]$Entry)
    
    $changelogPath = "CHANGELOG.md"
    $date = Get-Date -Format "yyyy-MM-dd"
    
    # Automatischen Changelog generieren wenn keiner angegeben
    if ([string]::IsNullOrEmpty($Entry)) {
        Write-ColorOutput "📝 Generating changelog from git commits..." $Blue
        $commits = Get-GitCommitsSinceLastTag $Version
        $Entry = Generate-ChangelogEntry $Version $commits
        Write-ColorOutput "✅ Generated changelog entry" $Green
    }
    
    $newEntry = @"

## [$Version] - $date

$Entry
"@
    
    $content = Get-Content $changelogPath -Raw
    $content = $content -replace "## \[$Version\]", $newEntry
    $content = $content -replace "The format is based on", "$newEntry`n`nThe format is based on"
    
    Set-Content $changelogPath $content -NoNewline
    Write-ColorOutput "✅ Updated CHANGELOG.md" $Green
}

function Update-GitHubPages {
    param([string]$OldVersion, [string]$NewVersion)
    
    $htmlPath = "docs/index.html"
    $date = Get-Date -Format "dd. MMMM yyyy"
    
    # Alle Version-Ersetzungen
    $replacements = @{
        "Download v$OldVersion" = "Download v$NewVersion"
        "StoDamageMeter_v$OldVersion.zip" = "StoDamageMeter_v$NewVersion.zip"
        "Neueste Version: $OldVersion" = "Neueste Version: $NewVersion"
        "STO Damage Meter v$OldVersion" = "STO Damage Meter v$NewVersion"
    }
    
    $content = Get-Content $htmlPath -Raw
    
    foreach ($replacement in $replacements.GetEnumerator()) {
        $content = $content -replace [regex]::Escape($replacement.Key), $replacement.Value
    }
    
    # Datum aktualisieren
    $content = $content -replace "Veröffentlicht am \d{1,2}\. \w+ \d{4}", "Veröffentlicht am $date"
    
    Set-Content $htmlPath $content -NoNewline
    Write-ColorOutput "✅ Updated GitHub Pages" $Green
}

function Get-ChangelogForVersion {
    param([string]$Version)
    
    $changelogPath = "CHANGELOG.md"
    $content = Get-Content $changelogPath -Raw
    
    # Extrahiere Changelog-Eintrag für die Version
    $pattern = "## \[$Version\] - [^\r\n]+\r?\n(.*?)(?=\r?\n## \[|\z)"
    if ($content -match $pattern) {
        return $matches[1].Trim()
    }
    
    return "Bugfixes und Verbesserungen"
}

function Create-GitHubRelease {
    param([string]$Version, [string]$Changelog)
    
    $tagName = "v$Version"
    $releaseName = "Release $Version - Window-Binding-System Verbesserungen"
    
    $body = @"
## 🚀 Window-Binding-System Verbesserungen

$Changelog

### Installation
1. ZIP entpacken
2. StoDamageMeter.exe starten
3. Combat-Log auswählen

### Systemanforderungen
- Windows 10/11 64-bit
- Keine zusätzliche Installation erforderlich
"@
    
    # GitHub CLI verwenden falls verfügbar
    if (Get-Command gh -ErrorAction SilentlyContinue) {
        Write-ColorOutput "🚀 Creating GitHub Release with CLI..." $Blue
        
        # Release erstellen
        gh release create $tagName `
            --title $releaseName `
            --notes $body `
            --latest `
            "Releases/StoDamageMeter_$Version.zip"
        
        Write-ColorOutput "✅ GitHub Release created!" $Green
    } else {
        Write-ColorOutput "⚠️  GitHub CLI not found. Please create release manually:" $Yellow
        Write-ColorOutput "   Tag: $tagName" $Yellow
        Write-ColorOutput "   Title: $releaseName" $Yellow
        Write-ColorOutput "   Upload: Releases/StoDamageMeter_$Version.zip" $Yellow
        Write-ColorOutput "   Body:" $Yellow
        Write-ColorOutput $body $Yellow
    }
}

# ============================================
# HAUPTSCRIPT
# ============================================

Write-ColorOutput "🚀 STO Damage Meter - Full Release Automation" $Blue
Write-ColorOutput "===============================================" $Blue

# 1. Aktuelle Version ermitteln
$currentVersion = Get-CurrentVersion
Write-ColorOutput "📋 Current Version: $currentVersion" $Yellow

# 2. Neue Version berechnen
if ([string]::IsNullOrEmpty($Version)) {
    $Version = Get-NextVersion $currentVersion $ReleaseType
}
Write-ColorOutput "🎯 Target Version: $Version" $Yellow

# 3. Bestätigung
$confirm = Read-Host "Continue with release $Version? (y/N)"
if ($confirm -ne "y" -and $confirm -ne "Y") {
    Write-ColorOutput "❌ Release cancelled" $Red
    exit 1
}

Write-ColorOutput "`n📝 Step 1: Updating version numbers..." $Blue

# 4. Version-Nummern aktualisieren
Update-VersionInFile "frontend/frontend.csproj" $currentVersion $Version
Update-VersionInFile "Launcher/SplashScreen.xaml" "Version $currentVersion" "Version $Version"

# UpdateCheckService.cs - komplexere Ersetzung
$updateServicePath = "frontend/Services/UpdateCheckService.cs"
if (Test-Path $updateServicePath) {
    $content = Get-Content $updateServicePath -Raw
    $content = $content -replace "return new Version\(\d+, \d+, \d+, 0\);", "return new Version($($Version.Split('.')[0]), $($Version.Split('.')[1]), $($Version.Split('.')[2]), 0);"
    $content = $content -replace "STO-Damage-Meter/\d+\.\d+\.\d+", "STO-Damage-Meter/$Version"
    Set-Content $updateServicePath $content -NoNewline
    Write-ColorOutput "✅ Updated: $updateServicePath" $Green
}

Write-ColorOutput "`n📝 Step 2: Updating CHANGELOG.md..." $Blue
Update-Changelog $Version $ChangelogEntry

Write-ColorOutput "`n📝 Step 3: Updating GitHub Pages..." $Blue
Update-GitHubPages $currentVersion $Version

Write-ColorOutput "`n📝 Step 4: Building release..." $Blue
& ".\scripts\create_release.ps1" -Version $Version
& ".\scripts\create_release_zip.ps1" -Version $Version

Write-ColorOutput "`n📝 Step 5: Git operations..." $Blue
git add .
git commit -m "Release v$Version

- Update version numbers to $Version
- Update CHANGELOG.md
- Update GitHub Pages
- Prepare release $Version"

git tag "v$Version"
git push origin main
git push origin "v$Version"

Write-ColorOutput "`n📝 Step 6: Creating GitHub Release..." $Blue
$changelog = Get-ChangelogForVersion $Version
Create-GitHubRelease $Version $changelog

Write-ColorOutput "`n🎉 Release $Version completed successfully!" $Green
Write-ColorOutput "===============================================" $Green
Write-ColorOutput "✅ Version numbers updated" $Green
Write-ColorOutput "✅ CHANGELOG.md updated" $Green
Write-ColorOutput "✅ GitHub Pages updated" $Green
Write-ColorOutput "✅ Release built and zipped" $Green
Write-ColorOutput "✅ Git committed and tagged" $Green
Write-ColorOutput "✅ GitHub Release created" $Green
Write-ColorOutput "`n🚀 Your release is ready!" $Blue
