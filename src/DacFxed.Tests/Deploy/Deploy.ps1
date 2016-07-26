param([string]$dacpacPath, [string]$publicProfilePath, [string]$module )
Remove-Module DacFxed | Out-Null

$modulePath =(Join-Path ($env:PSModulePath.Split(";")[0]) DacFxed)

Import-Module DacFxed

Publish-Database -DacpacPath $dacpacPath -PublishProfilePath $publicProfilePath -verbose

