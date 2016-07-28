param([string]$newVersion, [string]$sourceDir)

function Setup-LocalDb{
    param([string]$instanceName, [string]$version = "12.0" )

if((sqllocaldb i | findstr "instanceName") -eq $instanceName)
{
    sqllocaldb stop $instanceName
    sqllocaldb delete $instanceName
}
Write-Host $version
sqllocaldb create $instanceName "$($version)" -s
sqllocaldb start $instanceName

}

function Do-Deploy{
    param([string]$dacpacPath, [string]$publicProfilePath, [string]$module )

Remove-Module DacFxed 2> $null

$modulePath =(Join-Path ($env:PSModulePath.Split(";")[0]) DacFxed)

Import-Module DacFxed
Write-Host "extensions dir: $($root)\..\TestDacPac\bin\Release\extensions"
ls "$($root)\..\TestDacPac\bin\Release\extensions"

Publish-Database -DacpacPath $dacpacPath -PublishProfilePath $publicProfilePath -verbose -DacFxExtensionsPath "$($root)\..\TestDacPac\bin\Release\extensions\agile-sql-club"


}

$root = [System.IO.Path]::GetDirectoryName($myInvocation.MyCommand.Definition)
Write-Host "root: $($root)"
Setup-LocalDb "TedBert"
#Start-ProcessWithLogging "powershell.exe" "-ExecutionPolicy RemoteSigned -File $($root)\Deploy\Setup.ps1 TedBert"

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

Get-Module DacFxedS

#Start-ProcessWithLogging  "powershell.exe" "-ExecutionPolicy RemoteSigned -File $($root)\Deploy\Deploy.ps1 $($root)\..\TestDacPac\bin\Release\TestDacPac.dacpac abc -verbose"
Do-Deploy "$($root)\..\TestDacPac\bin\Release\TestDacPac.dacpac" "$($root)\..\TestDacPac\bin\Release\TestDacPac.publish.xml" "www"

Write-Host "all done??? $($LASTEXITCODE)"

