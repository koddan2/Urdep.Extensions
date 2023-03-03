<#
.SYNOPSIS
Installs TrackingCopyTool.exe into $target.

This script must be run in the same directory as the binary to install.
#>
[CmdletBinding()]
param (
    [string]$target
)
process {
    & .\TrackingCopyTool.exe install $target
}