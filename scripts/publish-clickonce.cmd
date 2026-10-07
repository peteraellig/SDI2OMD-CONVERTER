@echo off
setlocal
set "VSROOT="
for /f "usebackq delims=" %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.Component.MSBuild -property installationPath`) do set "VSROOT=%%i"
if not defined VSROOT (
  echo Visual Studio MSBuild wurde nicht gefunden.
  pause
  exit /b 1
)
"%VSROOT%\MSBuild\Current\Bin\MSBuild.exe" "%~dp0..\gui\SdiOmt.csproj" /restore /target:Publish /p:PublishProfile=ClickOnce /p:Configuration=Release /p:PublishDir="%~dp0..\publish\ClickOnce\\" /verbosity:minimal
if errorlevel 1 (
  echo ClickOnce-Export fehlgeschlagen.
  pause
  exit /b 1
)
powershell.exe -NoProfile -File "%~dp0check-clickonce.ps1" -PublishDirectory "%~dp0..\publish\ClickOnce"
if errorlevel 1 (
  echo ClickOnce-Startpruefung fehlgeschlagen.
  pause
  exit /b 1
)
echo Fertig: %~dp0..\publish\ClickOnce\setup.exe
pause
