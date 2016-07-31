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
ls "$($root)\..\TestDacPac\bin\Release\extensions" -recurse

Publish-Database -DacpacPath $dacpacPath -PublishProfilePath $publicProfilePath -verbose -DacFxExtensionsPath "$($root)\..\TestDacPac\bin\Release\extensions\"

}


$root = [System.IO.Path]::GetDirectoryName($myInvocation.MyCommand.Definition)

Write-Host "root: $($root)"
Setup-LocalDb "TedBert"

Write-Host "started tedbert....$($LASTEXITCODE)"

Write-Host "done the version stuff $($LASTEXITCODE)"

cp -Path "$($sourceDir)" -Destination "$($env:USERPROFILE)\Documents\WindowsPowershell\Modules\DacFxed" -Verbose -Force -Recurse


Write-Host "done copying...$($LASTEXITCODE)"

Get-Module DacFxed

Do-Deploy "$($root)\..\TestDacPac\bin\Release\TestDacPac.dacpac" "$($root)\..\TestDacPac\bin\Release\TestDacPac.publish.xml" "www"

Write-Host "all done??? $($LASTEXITCODE)"
