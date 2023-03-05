<#
.SYNOPSIS
#>
[CmdletBinding()]
param (
    $manifestFilename = "manifest.txt"
)
process {
    if (!(test-path "./$manifestFilename")) {
        & .\TrackingCopyTool.exe --Directory ../ -g yes -v 3 -m $manifestFilename
    }
    & .\TrackingCopyTool.exe --Directory ../ --onlyvalidatefiles yes -m $manifestFilename
}