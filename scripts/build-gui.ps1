$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
& (Join-Path $PSScriptRoot 'build.ps1')
& dotnet build (Join-Path $projectRoot 'gui\SdiOmt.csproj') -c Release
if ($LASTEXITCODE) { throw 'WinForms build failed.' }
$guiOutput = Join-Path $projectRoot 'gui\bin\Release\net8.0-windows'
$release = Join-Path $projectRoot 'build\Release'
foreach ($name in @('SdiOmt.exe','SdiOmt.dll','SdiOmt.deps.json','SdiOmt.runtimeconfig.json','SdiOmt.pdb')) {
    Copy-Item -LiteralPath (Join-Path $guiOutput $name) -Destination $release -Force
}
Write-Host "Fertig: $release\SdiOmt.exe"
