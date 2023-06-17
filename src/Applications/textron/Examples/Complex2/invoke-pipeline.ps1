[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [string]
    $PipelineScript,
    [Parameter(Mandatory = $true)]
    [string]
    $InputValue,
    [PSCustomObject]
    $Settings
)

begin {
    $errors = New-Object System.Collections.ArrayList
    if ([string]::IsNullOrWhiteSpace($PipelineScript)) { $errors.Add('Parameter $PipelineScript is required') }
    $PipelineScript = Resolve-Path -Path $PipelineScript
    if (-not (Test-Path $PipelineScript -PathType Leaf)) { $errors.Add('Parameter $PipelineScript does not point to a valid file') }

    Import-Module "$PSScriptRoot\auxiliary.psm1"

    if ($null -eq $Settings) {
        $Settings = @{}
    }

    if ([string]::IsNullOrWhiteSpace($Settings.Workspace)) {
        $Settings.Workspace = "$PSScriptRoot\workspace"
    }
    $workspace = $Settings.Workspace

    if (Test-Path $workspace -Type Container) {
        Remove-Item $workspace -Recurse -Force
    }
    New-Item $workspace -Type Directory | Write-Verbose
}

process {
    $steps = New-Object System.Collections.ArrayList
    function Add-Step {
        [CmdletBinding()]
        param (
            [Parameter(Mandatory = $true)]
            [scriptblock]
            $Body
        )
        $steps.Add($Body) | Out-Null
    }
    & $PipelineScript

    $result = $InputValue
    foreach ($step in $steps) {
        $context = @{
            Input     = $result
            Settings  = $settings
            Workspace = $workspace
        }
        $result = Invoke-Command -ScriptBlock $step -ArgumentList $context
    }
}

end {
    if ($errors.Count -gt 0) {
        throw $errors
    }
}