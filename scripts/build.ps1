param()
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$installation = & $vswhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
$cmake = Join-Path $installation 'Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe'
if (!(Test-Path -LiteralPath $cmake)) { throw 'Visual Studio C++ and CMake tools are required.' }
Push-Location $projectRoot
try {
    & $cmake --preset windows-x64
    if ($LASTEXITCODE) { throw 'Configure failed.' }
    & $cmake --build --preset release
    if ($LASTEXITCODE) { throw 'Build failed.' }
} finally { Pop-Location }
