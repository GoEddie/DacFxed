param([string]$newVersion)

$root = [System.IO.Path]::GetDirectoryName($myInvocation.MyCommand.Definition)
Write-Host $root
Start-Process -Wait -FilePath powershell.exe "$($root)\Deploy\Setup.ps1 TedBert"

Start-Process -Wait -FilePath powershell.exe "$($root)\..\DacFxed\DacFxed\UpdateModuleVersion.ps1 1.9.$($newVersion) $($root)\..\DacFxed\DacFxed\bin\Release $($root)\..\DacFxed\DacFxed\bin\Deploy\DacFxed\1.9.$($newVersion)"

cp -Path "$($root)\..\DacFxed\DacFxed\bin\Deploy\DacFxed\1.9.$($newVersion)" -Destination "$($env:USERPROFILE)\Documents\WindowsPowershell\Modules\DacFxed"

Get-Module -ListAvailable

Start-Process -Wait -FilePath powershell.exe "$($root)\Deploy\Deploy.ps1 $($root)\..\TestDacPac\bin\Release\TestDacPac.dacpac abc"