param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$SelfContained = $true
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot

$selfContainedValue = if ($SelfContained) { "true" } else { "false" }

Write-Host "Publishing GalagaClone..." -ForegroundColor Cyan

dotnet publish GalagaClone.csproj `
    -c $Configuration `
    -r $Runtime `
    --self-contained $selfContainedValue `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true

$publishDir = Join-Path $repoRoot "bin\$Configuration\net10.0-windows\$Runtime\publish"
Write-Host "Publish output: $publishDir" -ForegroundColor Green
