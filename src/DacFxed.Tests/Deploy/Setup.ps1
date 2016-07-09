param([string]$instanceName, [string]$version = "12.0" )

if((sqllocaldb i | findstr "instanceName") -eq "$instanceName")
{
    sqllocaldb stop $instanceName
    sqllocaldb delete $instanceName
}
Write-Host $version
sqllocaldb create $instanceName "$($version)" -s
sqllocaldb start $instanceName

