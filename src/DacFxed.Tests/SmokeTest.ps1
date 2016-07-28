param([string]$newVersion, [string]$sourceDir)

$root = [System.IO.Path]::GetDirectoryName($myInvocation.MyCommand.Definition)
Write-Host "root: $($root)"

#Start-Process -Wait -FilePath powershell.exe "$($root)\Deploy\Setup.ps1 TedBert"

Write-Host "started tedbert....$($LASTEXITCODE)"

#Start-Process -Wait -FilePath powershell.exe "$($root)\..\DacFxed\DacFxed\UpdateModuleVersion.ps1 1.9.$($newVersion) $($sourceDir) $($root)\..\DacFxed\DacFxed\bin\Deploy\DacFxed\1.9.$($newVersion)"

Write-Host "done the version stuff $($LASTEXITCODE)"

cp -Path "$($sourceDir)" -Destination "$($env:USERPROFILE)\Documents\WindowsPowershell\Modules\DacFxed" -Verbose -Force -Recurse

ls "$($env:USERPROFILE)\Documents\WindowsPowershell\Modules\DacFxed"
Write-Host "ls 1"
ls  "$($env:USERPROFILE)\Documents\WindowsPowershell\Modules"
Write-Host "ls 2"
ls C:\Users\buildguest\Documents\WindowsPowerShell\Modules -Recurse
Write-Host "ls 3"


Write-Host "done copying...$($LASTEXITCODE)"

Get-Module -ListAvailable

Start-Process -Wait -FilePath powershell.exe "$($root)\Deploy\Deploy.ps1 $($root)\..\TestDacPac\bin\Release\TestDacPac.dacpac abc -verbose"


Write-Host "all done??? $($LASTEXITCODE)"

