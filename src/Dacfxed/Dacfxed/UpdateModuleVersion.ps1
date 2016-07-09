param(
	[string] $newVersion,
	[string] $path )

(Get-Content $path).replace('$$VERSION$$', $newVersion) | Set-Content $path