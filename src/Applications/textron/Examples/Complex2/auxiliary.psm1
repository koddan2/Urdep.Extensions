
function Copy-FilteredLines {
    [CmdletBinding()]
    param (
        [System.IO.StreamReader]$reader,
        [System.IO.StreamWriter]$writer,
        [PSCustomObject]$filters
    )
    
    begin {
        $skipping = $false
    }
    
    process {
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
    
    end {
        $writer.Flush()
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