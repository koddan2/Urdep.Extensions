using namespace System.IO;
using namespace System.Text;
using namespace System.Text.Encodings;

[CmdletBinding(SupportsShouldProcess = $true)]
param (
    [Parameter(Mandatory = $true)]
    [string]
    $InputFile,
    [string]
    $VariableMappingsJsonFile,
    [string]
    $BaseDirectory,
    [string]
    $WorkspaceDirectoryRelativePath = "workspace"
)

begin {
    $ErrorActionPreference = 'Stop'

    if ([string]::IsNullOrWhiteSpace($BaseDirectory)) {
        $BaseDirectory = $PSScriptRoot
    }

    Import-Module "$PSScriptRoot/auxiliary.psm1" -Force

    $workspace = [Path]::GetFullPath([Path]::Combine($BaseDirectory, $WorkspaceDirectoryRelativePath))
    Write-Verbose "WORKSPACE: $workspace"
    function Get-WorkspacePath ($name) { [Path]::GetFullPath([Path]::Combine($workspace, $name)) }

    if ((Test-Path $workspace -PathType Container)) {
        Remove-Item -Force -Recurse -Path $workspace
    }
    New-Item -Type Directory -Path $workspace -Force | Out-Null

    $inputFileAbsPath = Join-Path -Path $BaseDirectory -ChildPath $InputFile -Resolve
}

process {
    $step1_Target = Get-WorkspacePath "step1.sql"
    if ($PSCmdlet.ShouldProcess($step1_Target, "Step #1")) {
        Invoke-Using @(
            ([StreamReader]$reader = New-Object StreamReader $inputFileAbsPath)
            ([StreamWriter]$writer = New-Object StreamWriter ([File]::OpenWrite($step1_Target)))
        ) {
            Copy-FilteredLines $reader $writer @{
                BeginBlock = "--[[INTERACTIVE"
                EndBlock   = "--]]"
            }
        }
    }

    $step2_Target = Get-WorkspacePath "step2.sql"
    if ($PSCmdlet.ShouldProcess($step2_Target, "Step #2")) {
        Invoke-Using @(
            ([StreamReader]$reader = New-Object StreamReader ($step1_Target))
            ([StreamWriter]$writer = New-Object StreamWriter ([File]::OpenWrite($step2_Target)))
        ) {
            while (-not $reader.EndOfStream) {
                $line = $reader.ReadLine()
                if (-not [string]::IsNullOrWhiteSpace($line)) {
                    $updatedLine = ($line.
                        Replace('REPLACE-WITH($env:SystemDrive)', $env:SystemDrive).
                        Replace("/*REPLACE-WITH(@p0)*/", "@p0"))
                    $writer.WriteLine($updatedLine)
                }
            }
        }
    }

    $step3_Target = Get-WorkspacePath "step3.xml"
    if ($PSCmdlet.ShouldProcess($step3_Target, "Step #3")) {
        $templateFile = Join-Path $BaseDirectory "template.xml" -Resolve
        $template = Get-Content $templateFile
        $content = [File]::ReadAllText($step2_Target)
        Set-Content -Path $step3_Target -Value ($template.Replace("RENDER-HERE", $content))
    }

    $step4_Target = Get-WorkspacePath "result.xml"
    if ($PSCmdlet.ShouldProcess($step4_Target, "Step #4")) {
        $content = [File]::ReadAllText($step3_Target)
        $result = $content.
            Replace('REPLACE-WITH($output-name)', "this is the name").
            Replace('encoding="UTF-8"', 'encoding="ISO-8859-1"')
        [File]::WriteAllText($step4_Target, $result, [Encoding]::GetEncoding('iso-8859-1'))
    }
}

end {

}