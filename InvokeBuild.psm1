
$base = (Join-Path $PSScriptRoot "Tools" "Invoke-Build")

Set-Alias -Name Build-Checkpoint -Value (Join-Path $base Build-Checkpoint.ps1)
Set-Alias -Name Build-Parallel -Value (Join-Path $base Build-Parallel.ps1)
Set-Alias -Name Invoke-Build -Value (Join-Path $base Invoke-Build.ps1)
Export-ModuleMember -Alias Build-Checkpoint, Build-Parallel, Invoke-Build
