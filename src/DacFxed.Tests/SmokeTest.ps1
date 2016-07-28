param([string]$newVersion, [string]$sourceDir)

function Start-ProcessWithLogging{
    param([string]$file, [string]$args)

$pinfo = New-Object System.Diagnostics.ProcessStartInfo
$pinfo.FileName = $file
$pinfo.RedirectStandardError = $true
$pinfo.RedirectStandardOutput = $true
$pinfo.UseShellExecute = $false
$pinfo.Arguments = $args
$p = New-Object System.Diagnostics.Process
$p.StartInfo = $pinfo
$p.Start() | Out-Null
$p.WaitForExit()
$stdout = $p.StandardOutput.ReadToEnd()
$stderr = $p.StandardError.ReadToEnd()
Write-Host "stdout: $stdout"
Write-Host "stderr: $stderr"
Write-Host "exit code: " + $p.ExitCode

}

$root = [System.IO.Path]::GetDirectoryName($myInvocation.MyCommand.Definition)
Write-Host "root: $($root)"

Start-ProcessWithLogging "powershell.exe" "-ExecutionPolicy RemoteSigned -File $($root)\Deploy\Setup.ps1 TedBert"

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

Start-ProcessWithLogging  "powershell.exe" "-ExecutionPolicy RemoteSigned -File $($root)\Deploy\Deploy.ps1 $($root)\..\TestDacPac\bin\Release\TestDacPac.dacpac abc -verbose"

Write-Host "all done??? $($LASTEXITCODE)"

