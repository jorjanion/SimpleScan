# Script de instalación rápida de SimpleScan para Windows
param(
    [string]$SourceExe = ""
)

$ErrorActionPreference = "Stop"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "       Instalador de SimpleScan           " -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# Locate executable
if ([string]::IsNullOrEmpty($SourceExe)) {
    $candidates = @(
        "$PSScriptRoot\..\dist\portable\SimpleScan.exe",
        "$PSScriptRoot\..\dist\framework-dependent\SimpleScan.exe",
        "$PSScriptRoot\SimpleScan.exe"
    )
    foreach ($c in $candidates) {
        if (Test-Path $c) {
            $SourceExe = (Resolve-Path $c).Path
            break
        }
    }
}

if (-not (Test-Path $SourceExe)) {
    Write-Host "Compilando la versión Release de SimpleScan..." -ForegroundColor Yellow
    $sdkDotnet = "$env:LocalAppData\dotnet\dotnet.exe"
    if (-not (Test-Path $sdkDotnet)) { $sdkDotnet = "dotnet" }
    
    & $sdkDotnet publish "$PSScriptRoot\..\src\SimpleScan.UI\SimpleScan.UI.csproj" -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o "$PSScriptRoot\..\dist\portable"
    $SourceExe = "$PSScriptRoot\..\dist\portable\SimpleScan.exe"
}

if (-not (Test-Path $SourceExe)) {
    Write-Error "No se pudo encontrar el archivo SimpleScan.exe para instalar."
    exit 1
}

# Target directory in LocalAppData (no admin rights needed!)
$installDir = "$env:LocalAppData\Programs\SimpleScan"
if (-not (Test-Path $installDir)) {
    New-Item -ItemType Directory -Path $installDir -Force | Out-Null
}

$targetExe = "$installDir\SimpleScan.exe"
Write-Host "Copiando SimpleScan a: $installDir" -ForegroundColor Green
Copy-Item -Path $SourceExe -Destination $targetExe -Force

# Create Desktop Shortcut
$wsh = New-Object -ComObject WScript.Shell
$desktop = [Environment]::GetFolderPath("Desktop")
$shortcutDesktop = $wsh.CreateShortcut("$desktop\SimpleScan.lnk")
$shortcutDesktop.TargetPath = $targetExe
$shortcutDesktop.WorkingDirectory = $installDir
$shortcutDesktop.Description = "SimpleScan - Escaneo ultrarrápido sin bloatware"
$shortcutDesktop.Save()
Write-Host "Acceso directo en el Escritorio creado." -ForegroundColor Green

# Create Start Menu Shortcut
$startMenu = [Environment]::GetFolderPath("Programs")
$shortcutStart = $wsh.CreateShortcut("$startMenu\SimpleScan.lnk")
$shortcutStart.TargetPath = $targetExe
$shortcutStart.WorkingDirectory = $installDir
$shortcutStart.Description = "SimpleScan - Escaneo ultrarrápido sin bloatware"
$shortcutStart.Save()
Write-Host "Acceso directo en el Menú Inicio creado." -ForegroundColor Green

# Write Uninstall script
$uninstallScript = @"
`$ErrorActionPreference = 'SilentlyContinue'
`$desktop = [Environment]::GetFolderPath('Desktop')
`$startMenu = [Environment]::GetFolderPath('Programs')
Remove-Item "`$desktop\SimpleScan.lnk" -Force
Remove-Item "`$startMenu\SimpleScan.lnk" -Force
Remove-Item -Path "$installDir" -Recurse -Force
Remove-Item -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\SimpleScan" -Force
[System.Windows.Forms.MessageBox]::Show('SimpleScan ha sido desinstalado correctamente.', 'SimpleScan', 'OK', 'Information')
"@
Set-Content -Path "$installDir\Uninstall.ps1" -Value $uninstallScript -Encoding UTF8

# Register in Windows Apps / Uninstall registry
$uninstallKey = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\SimpleScan"
if (-not (Test-Path $uninstallKey)) {
    New-Item -Path $uninstallKey -Force | Out-Null
}
Set-ItemProperty -Path $uninstallKey -Name "DisplayName" -Value "SimpleScan"
Set-ItemProperty -Path $uninstallKey -Name "DisplayVersion" -Value "1.0.0"
Set-ItemProperty -Path $uninstallKey -Name "Publisher" -Value "Jorge (jorjanion)"
Set-ItemProperty -Path $uninstallKey -Name "InstallLocation" -Value $installDir
Set-ItemProperty -Path $uninstallKey -Name "DisplayIcon" -Value "$targetExe,0"
Set-ItemProperty -Path $uninstallKey -Name "UninstallString" -Value "powershell.exe -ExecutionPolicy Bypass -File `"$installDir\Uninstall.ps1`""

Write-Host ""
Write-Host "¡Instalación completada con éxito!" -ForegroundColor Cyan
Write-Host "SimpleScan ya está disponible en tu Escritorio y en el Menú Inicio." -ForegroundColor Cyan
