param([string]$dacpacPath, [string]$publicProfilePath, [string]$module )
Remove-Module DacFxed | Out-Null

$modulePath =(Join-Path ($env:PSModulePath.Split(";")[0]) DacFxed)


New-Item -Force -ItemType directory -Path $modulePath
Copy-Item -Path $module -Destination $modulePath
$psd1 = (Join-Path $modulePath "DacFxed.psd1")

(Get-Content $psd1).replace('$$VERSION$$', "99.9") | Set-Content $psd1
Import-Module DacFxed

Publish-Database -DacpacPath $dacpacPath -PublishProfilePath $publicProfilePath -verbose

