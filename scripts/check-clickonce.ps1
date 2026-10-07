param(
    [string]$PublishDirectory = (Join-Path $PSScriptRoot '..\publish\ClickOnce')
)
$ErrorActionPreference = 'Stop'
[xml]$deployment = Get-Content -LiteralPath (Join-Path $PublishDirectory 'SdiOmt.application')
$applicationReference = $deployment.SelectSingleNode("//*[local-name()='dependentAssembly' and @dependencyType='install']")
if (!$applicationReference) { throw 'The ClickOnce application manifest is missing.' }
$applicationPath = Join-Path $PublishDirectory $applicationReference.GetAttribute('codebase')
[xml]$application = Get-Content -LiteralPath $applicationPath
$permissionSet = $application.SelectSingleNode("//*[local-name()='applicationRequestMinimum']/*[local-name()='PermissionSet']")
if (!$permissionSet -or $permissionSet.GetAttribute('Unrestricted') -ne 'true') {
    throw 'ClickOnce requires full trust for the .NET launcher and native SDI/OMT libraries.'
}
$executionLevel = $application.SelectSingleNode("//*[local-name()='requestedExecutionLevel']")
if (!$executionLevel -or $executionLevel.GetAttribute('level') -ne 'asInvoker') {
    throw 'The application must run without requesting administrator privileges.'
}
$entryPoint = $application.SelectSingleNode("//*[local-name()='entryPoint']/*[local-name()='commandLine']")
if (!$entryPoint -or $entryPoint.GetAttribute('file') -ne 'Launcher.exe') {
    throw 'The .NET ClickOnce launcher entry point is missing.'
}
$applicationDirectory = Split-Path $applicationPath
foreach ($payload in 'Launcher.exe', 'SdiOmt.exe', 'SdiOmt.dll', 'sdi_omt.exe', 'libomt.dll', 'libvmx.dll', 'hostfxr.dll') {
    if (!(Test-Path -LiteralPath (Join-Path $applicationDirectory "$payload.deploy"))) {
        throw "Missing ClickOnce payload: $payload.deploy"
    }
}
Write-Host 'ClickOnce manifest and required startup files checked successfully.'
