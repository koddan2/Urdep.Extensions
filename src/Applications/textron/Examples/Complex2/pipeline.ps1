
Add-Step {
    param($context)
    # $context | Out-Default

    $target = Join-Path $context.Workspace "step1.sql"
    Write-Verbose $target
    Invoke-Using @(
        ([System.IO.StreamReader]$reader = New-Object System.IO.StreamReader (Resolve-Path $context.Input))
        ([System.IO.StreamWriter]$writer = New-Object System.IO.StreamWriter ([System.IO.File]::OpenWrite($target)))
    ) {
        Copy-FilteredLines $reader $writer @{
            BeginBlock = "--[[INTERACTIVE"
            EndBlock   = "--]]"
        }
    }

    return $target
}

Add-Step {
    param($context)
    Write-Warning $context.Input
}