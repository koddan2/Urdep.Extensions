
Add-Step {
    param($context)
    $target = Join-Path $context.Workspace "step1.sql"
    Copy-FilteredLines $context.Input $target @{
        BeginBlock = "--[[INTERACTIVE"
        EndBlock   = "--]]"
    }

    return $target
}

Add-Step {
    param($context)
    $target = Join-Path $context.Workspace "step2.sql"
    $mappings = @{
        'REPLACE-WITH($env:SystemDrive)' = $env:SystemDrive
        '/*REPLACE-WITH(@p0)*/'          = '@p0'
    }
    Copy-TextByLine $context.Input $target {
        param([string]$line)
        if (-not [string]::IsNullOrWhiteSpace($line)) {
            return (Convert-ReplaceVariables $line $mappings)
        }
        else {
            return $null
        }
    }

    return $target
}

Add-Step {
    param($context)
    $target = Join-Path $context.Workspace "step3.xml"
    $template = Get-Content "../Complex/template.xml" -Raw

    $mappings = @{
        'RENDER-HERE'                = (Get-Content $context.Input -Raw).Trim()
        'REPLACE-WITH($output-name)' = '$output-name'
    }
    $result = (Convert-ReplaceVariables $template $mappings)
    Set-Content $target $result

    return $target
}

Add-Step {
    param($context)
    $target = Join-Path $context.Workspace $context.Settings.ResultName
    Copy-Item $context.Input $target
    return $target
}