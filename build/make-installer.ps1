param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$AppVersion = "0.1.0"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot

$publishScript = Join-Path $PSScriptRoot "publish.ps1"
& $publishScript -Configuration $Configuration -Runtime $Runtime -SelfContained

$publishDir = Join-Path $repoRoot "bin\$Configuration\net10.0-windows\$Runtime\publish"
$innoScript = Join-Path $repoRoot "installer\GalagaClone.iss"

$iscc = Get-Command iscc -ErrorAction SilentlyContinue
if (-not $iscc)
{
    throw "Inno Setup Compiler (iscc.exe) not found in PATH. Install Inno Setup and add iscc.exe to PATH."
}

Write-Host "Building installer..." -ForegroundColor Cyan
& $iscc.Source "/DAppVersion=$AppVersion" "/DPublishDir=$publishDir" $innoScript

Write-Host "Installer created under installer\output" -ForegroundColor Green
