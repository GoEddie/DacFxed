param(
	[string]$newVersion,
	[string] $sourcePath,
	[string] $destinationPath
)

function Set-PsdFile {

	$file = Get-Content (Join-Path $sourcePath "DacFxed.psd1" )
	$file = $file.Replace('$$VERSION$$', $newVersion )
	Set-Content -Path (Join-Path $destinationPath "DacFxed.psd1") -Value $file

}

	New-Item -ItemType Directory -Path $destinationPath -Force | Out-Null
	New-Item -ItemType Directory -Path (Join-Path $destinationPath "bin\dll") -Force | Out-Null
	New-Item -ItemType Directory -Path (Join-Path (Join-Path $destinationPath "bin\dll") "Extensions") -Force | Out-Null

"DacFxed.dll", "DacFxed.psd1", "DacFxedProxy.dll", "DacFxLoadProxy.dll", "Microsoft.Management.Infrastructure.dll", "Microsoft.VisualStudio.Data.Tools.Package.dll" | foreach-object($_) {
		Copy-Item -Path (Join-Path $sourcePath $_) -Destination (Join-Path $destinationPath $_)
} 

"Microsoft.Data.Tools.Schema.Sql.dll", "Microsoft.Data.Tools.Utilities.dll", "Microsoft.SqlServer.Dac.dll", "Microsoft.SqlServer.Dac.Extensions.dll", "Microsoft.SqlServer.TransactSql.ScriptDom.dll", "Microsoft.SqlServer.Types.dll" | foreach-object($_) {
		Copy-Item -Path (Join-Path $sourcePath $_) -Destination (Join-Path(Join-Path $destinationPath "bin\dll") $_)
} 


Set-PsdFile 
