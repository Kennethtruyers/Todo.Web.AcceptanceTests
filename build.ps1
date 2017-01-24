cls

$ErrorActionPreference = 'Stop'
if (!(Get-Command NuGet -ErrorAction SilentlyContinue) -and !(Test-Path "$env:LOCALAPPDATA\NuGet\NuGet.exe")) {
	Write-Host 'Downloading NuGet.exe'
	(New-Object System.Net.WebClient).DownloadFile("https://dist.nuget.org/win-x86-commandline/latest/nuget.exe", "$env:LOCALAPPDATA\NuGet\NuGet.exe")
} 
if (Test-Path "$env:LOCALAPPDATA\NuGet\NuGet.exe") { 
	Set-Alias NuGet (Resolve-Path $env:LOCALAPPDATA\NuGet\NuGet.exe)
} 
Write-Host 'Restoring NuGet packages'
NuGet restore

. '.\functions.ps1'
$invokeBuild = (Get-ChildItem('packages\Invoke-Build*\tools\Invoke-Build.ps1')).FullName | Sort-Object $_ | Select -Last 1
& $invokeBuild $args -File Tasks.ps1