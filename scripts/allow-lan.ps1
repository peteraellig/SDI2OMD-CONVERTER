# Optional: run as Administrator on the sender. Does not change existing block rules.
param([string]$RemoteAddress = 'LocalSubnet', [string]$TcpPorts = '6400-6600')
$ErrorActionPreference = 'Stop'
$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = [Security.Principal.WindowsPrincipal]::new($identity)
if (!$principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw 'Run this script from an administrator PowerShell.'
}
foreach ($spec in @(
    @{Name='SDI-OMT-LAN-TCP'; DisplayName='SDI OMT - Private LAN video and audio'; Protocol='TCP'; LocalPort=$TcpPorts},
    @{Name='SDI-OMT-LAN-mDNS'; DisplayName='SDI OMT - Private LAN discovery'; Protocol='UDP'; LocalPort='5353'}
)) {
    $existing = Get-NetFirewallRule -Name $spec.Name -ErrorAction SilentlyContinue
    if ($existing) {
        Set-NetFirewallRule -Name $spec.Name -Enabled True -Direction Inbound -Action Allow -Profile Private -RemoteAddress $RemoteAddress -Protocol $spec.Protocol -LocalPort $spec.LocalPort
    } else {
        New-NetFirewallRule @spec -Direction Inbound -Action Allow -Profile Private -RemoteAddress $RemoteAddress | Out-Null
    }
}
Write-Host 'OMT inbound rules enabled for the private network.'
Write-Host 'Also allow the sender in any third-party firewall, if installed.'
