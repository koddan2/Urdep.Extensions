using namespace System.IO;

function Convert-ReplaceVariables {
    param([string]$Text, [PSCustomObject]$Mappings)
    $result = $Text
    foreach($mapping in $Mappings.GetEnumerator()) {
        $result = $result.Replace($mapping.Key, $mapping.Value)
    }
    return $result
}

Export-ModuleMember Convert-ReplaceVariables

function Copy-TextByLine {
    [CmdletBinding()]
    param (
        [string]$InputFile,
        [string]$OutputFile,
        [scriptblock]$transformer
    )

    Invoke-Using @(
        ([StreamReader]$reader = New-Object StreamReader (Resolve-Path $InputFile))
        ([StreamWriter]$writer = New-Object StreamWriter ([File]::OpenWrite($OutputFile)))
    ) {
        while (-not $reader.EndOfStream) {
            $line = $reader.ReadLine()
            $updatedLine = Invoke-Command -ScriptBlock $transformer -ArgumentList $line
            if ($null -ne $updatedLine) {
                $writer.WriteLine($updatedLine)
            }
        }
    }
}
Export-ModuleMember Copy-TextByLine

function Copy-FilteredLines {
    [CmdletBinding()]
    param (
        [string]$InputFile,
        [string]$OutputFile,
        [PSCustomObject]$filters
    )
    
    begin {
        $skipping = $false
    }
    
    process {
        Invoke-Using @(
        ([StreamReader]$reader = New-Object StreamReader (Resolve-Path $InputFile))
        ([StreamWriter]$writer = New-Object StreamWriter ([File]::OpenWrite($OutputFile)))
        ) {
            while (-not $reader.EndOfStream) {
                $line = $reader.ReadLine()
                if ($line.StartsWith($filters.BeginBlock)) {
                    $skipping = $true
                    continue
                }
                elseif ($line.StartsWith($filters.EndBlock)) {
                    $skipping = $false
                    continue
                }

                if ($skipping -or [string]::IsNullOrWhiteSpace($line)) {
                    continue
                }

                $writer.WriteLine($line)
            }
        }
    }
    
    end {
    }
}
Export-ModuleMember Copy-FilteredLines

function Invoke-Using {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [array]
        $ArrayOfDisposables,

        [Parameter(Mandatory = $true)]
        [scriptblock]
        $ScriptBlock
    )

    try {
        . $ScriptBlock
    }
    finally {
        foreach ($disposable in $ArrayOfDisposables) {
            if ($null -ne $disposable -and $disposable -is [System.IDisposable]) {
                $disposable.Dispose()
            }
            else {
                Write-Warning "Value in `$ArrayOfDisposables was `$null or not [System.IDisposable]."
            }
        }
    }
}
Export-ModuleMember Invoke-Using